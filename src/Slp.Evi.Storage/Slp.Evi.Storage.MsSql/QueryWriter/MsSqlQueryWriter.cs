using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngleSharp.Common;
using Slp.Evi.Database;
using Slp.Evi.Relational.Algebra;
using Slp.Evi.Storage.Common.FSharpExtensions;
using Slp.Evi.Storage.MsSql.Database;

namespace Slp.Evi.Storage.MsSql.QueryWriter
{
    class MsSqlQueryWriter
        : ISqlDatabaseWriter<MsSqlQuery>
    {
        /// <inheritdoc />
        public MsSqlQuery WriteQuery(SqlQuery query)
        {
            StringBuilder sb = new StringBuilder();
            WriteQuery(sb, query);
            return new MsSqlQuery(sb.ToString());
        }

        private void WriteQuery(StringBuilder sb, SqlQuery sqlQuery)
        {
            if (sqlQuery.IsDistinct && !sqlQuery.Ordering.IsEmpty)
            {
                sb.Append("SELECT * FROM (");
            }

            var variablesMappings = new Dictionary<string, List<Variable>>();
            foreach (var variable in sqlQuery.Variables)
            {
                if (sqlQuery.NamingProvider.TryGetVariableName(variable, out var varName))
                {
                    if (!variablesMappings.TryGetValue(varName, out var varList))
                    {
                        varList = new List<Variable>();
                        variablesMappings.Add(varName, varList);
                    }

                    varList.Add(variable);
                }
                else
                {
                    throw new InvalidOperationException("Ended up with a variable without name");
                }
            }

            if (variablesMappings.Count == 0)
            {
                variablesMappings.Add("c", new List<Variable>());
            }

            var variables = variablesMappings.Keys.OrderBy(x => x).ToList();

            var innerLimit = sqlQuery.InnerQueries.Length == 1 && sqlQuery.Ordering.IsEmpty && sqlQuery.Offset.IsNone()
                ? sqlQuery.Limit.ToNullable()
                : null;

            var firstInnerQuery = true;
            foreach (var innerQuery in sqlQuery.InnerQueries)
            {
                if (!firstInnerQuery)
                {
                    if (sqlQuery.IsDistinct)
                    {
                        sb.Append(" UNION ");
                    }
                    else
                    {
                        sb.Append(" UNION ALL ");
                    }
                }
                else
                {
                    firstInnerQuery = false;
                }

                WriteInnerQuery(sb, innerQuery, variables, variablesMappings, sqlQuery.IsDistinct, innerLimit);
            }

            if (sqlQuery.IsDistinct && !sqlQuery.Ordering.IsEmpty)
            {
                sb.Append(")");
            }

            // TODO: Add ordering and offset and limit

            throw new NotImplementedException();
        }

        private void WriteInnerQuery(StringBuilder sb, QueryContent query, List<string> variables, Dictionary<string, List<Variable>> variablesMappings, bool isDistinct, int? innerLimit)
        {
            sb.Append("SELECT");

            if (isDistinct)
            {
                sb.Append(" DISTINCT");
            }

            if (innerLimit.HasValue)
            {
                sb.Append(" TOP");
                sb.Append(innerLimit.Value);
            }

            if (query.IsSelectQuery)
            {
                WriteInnerSelectQuery(sb, ((QueryContent.SelectQuery) query).Item, variables, variablesMappings);
            }
            else if (query.IsNoResultQuery)
            {
                WriteInnerNoResultQuery(sb, variables, variablesMappings);
            }
            else if (query.IsSingleEmptyResultQuery)
            {
                WriteInnerSingleEmptyResultQuery(sb, variables, variablesMappings);
            }
            else
            {
                throw new ArgumentException("Produced query does not have supported type", nameof(query));
            }
        }

        private void WriteInnerSingleEmptyResultQuery(StringBuilder sb, List<string> variables, Dictionary<string, List<Variable>> variablesMappings)
        {
            throw new NotImplementedException();
        }

        private void WriteInnerNoResultQuery(StringBuilder sb, List<string> variables, Dictionary<string, List<Variable>> variablesMappings)
        {
            throw new NotImplementedException();
        }

        private void WriteInnerSelectQuery(StringBuilder sb, InnerQuery query, List<string> variables, Dictionary<string, List<Variable>> variablesMappings)
        {
            var isFirstVariable = true;
            foreach (var variableName in variables)
            {
                if (!isFirstVariable)
                {
                    sb.Append(",");
                }
                else
                {
                    isFirstVariable = false;
                }

                var providedVariables = variablesMappings.GetOrDefault(variableName, new List<Variable>())
                    .Where(var => query.ProvidedVariables.Contains(var))
                    .ToList();

                if (providedVariables.Count > 1)
                {
                    throw new InvalidOperationException($"There are more provided variables for name {variableName}");
                }
                else if (providedVariables.Count == 1)
                {
                    var variable = providedVariables[0];
                    WriteVariable(sb, query, variable);
                }
                else
                {
                    WriteExpression(sb, query, Expression.Null);
                }

                sb.Append(" AS ");
                sb.Append(variableName);
            }

            sb.Append(" FROM ");

            var isFirstSource = true;
            foreach (var innerSource in query.Sources)
            {
                if (!isFirstSource)
                {
                    sb.Append(", ");
                }
                else
                {
                    isFirstSource = false;
                }

                WriteInnerSource(sb, innerSource);

                sb.Append(" AS ");

                if (query.NamingProvider.TryGetSourceName(innerSource, out var sourceName))
                {
                    sb.Append(sourceName);
                }
                else
                {
                    throw new InvalidOperationException($"Name for source has not been found. Source: {innerSource}");
                }
            }

            // Left outer join, filters
            throw new NotImplementedException();
        }

        private void WriteInnerSource(StringBuilder sb, InnerSource innerSource)
        {
            if (innerSource.IsInnerTable)
            {
                var table = (InnerSource.InnerTable) innerSource;
                sb.Append(table.Item1.Schema.Name);
            }
            else if (innerSource.IsInnerSource)
            {
                sb.Append("(");
                var source = (InnerSource.InnerSource) innerSource;
                WriteQuery(sb, source.Item);
                sb.Append(")");
            }
            else
            {
                throw new InvalidOperationException("Source has to be either a table or inner source");
            }
        }

        private void WriteVariable(StringBuilder sb, InnerQuery query, Variable variable)
        {
            if (query.NamingProvider.TryGetSource(variable, out var innerSource))
            {
                WriteSourcedVariable(sb, query, innerSource, variable);
            }
            else
            {
                if (variable.IsAssigned)
                {
                    var assignedVariable = ((Variable.Assigned) variable).Item;
                    var assignment = query.Assignments.Single(x => x.Variable == assignedVariable);
                    WriteExpression(sb, query, assignment.Expression);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Variable {variable} has not been found in any source and still, it is not assigned.");
                }
            }
        }

        private static void WriteSourcedVariable(StringBuilder sb, InnerQuery query, InnerSource innerSource, Variable variable)
        {
            if (query.NamingProvider.TryGetSourceName(innerSource, out var innerSourceName))
            {
                sb.Append(" ");
                sb.Append(innerSourceName);
                sb.Append(".");

                if (innerSource.NamingProvider.TryGetVariableName(variable, out var variableName))
                {
                    sb.Append(variableName);
                }
                else 
                {
                    throw new InvalidOperationException("Cannot find name for a variable from source");
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot find name for an existing variable source");
            }
        }

        private void WriteExpression(StringBuilder sb, InnerQuery query, Expression assignmentExpression)
        {
            throw new NotImplementedException();
        }
    }
}
