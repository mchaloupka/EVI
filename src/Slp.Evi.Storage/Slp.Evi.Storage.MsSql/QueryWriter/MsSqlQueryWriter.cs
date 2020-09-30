using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AngleSharp.Common;
using Microsoft.FSharp.Collections;
using Slp.Evi.Common;
using Slp.Evi.Common.Database;
using Slp.Evi.Database;
using Slp.Evi.Relational.Algebra;
using Slp.Evi.Storage.Common.FSharpExtensions;
using Slp.Evi.Storage.MsSql.Database;

namespace Slp.Evi.Storage.MsSql.QueryWriter
{
    public class MsSqlQueryWriter
        : ISqlDatabaseWriter<MsSqlQuery>
    {
        private readonly MsSqlDatabaseSchema _databaseSchema;

        public MsSqlQueryWriter(MsSqlDatabaseSchema databaseSchema)
        {
            _databaseSchema = databaseSchema;
        }

        /// <inheritdoc />
        public MsSqlQuery WriteQuery(SqlQuery query)
        {
            StringBuilder sb = new StringBuilder();
            WriteQuery(sb, query);
            return new MsSqlQuery(sb.ToString());
        }

        private void WriteQuery(StringBuilder sb, SqlQuery sqlQuery)
        {
            bool isTrivialQuery;
            if (sqlQuery.Ordering.IsEmpty && sqlQuery.Limit.IsSome())
            {
                sb.Append("SELECT TOP ");
                sb.Append(sqlQuery.Limit.Value);
                sb.Append(" * FROM (");
                isTrivialQuery = false;
            }
            else if(!sqlQuery.Ordering.IsEmpty)
            {
                sb.Append("SELECT * FROM (");
                isTrivialQuery = false;
            }
            else
            {
                isTrivialQuery = true;
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

            var firstInnerQuery = true;
            foreach (var innerQuery in sqlQuery.InnerQueries)
            {
                if (firstInnerQuery)
                {
                    firstInnerQuery = false;
                }
                else
                {
                    sb.Append(sqlQuery.IsDistinct ? " UNION " : " UNION ALL ");
                }

                WriteInnerQuery(sb, innerQuery, variables, variablesMappings, sqlQuery.IsDistinct);
            }

            if (!isTrivialQuery)
            {
                sb.Append(") AS _");
            }

            var firstOrderBy = true;
            foreach (var ordering in sqlQuery.Ordering)
            {
                if (firstOrderBy)
                {
                    sb.Append(" ORDER BY ");
                    firstOrderBy = false;
                }
                else
                {
                    sb.Append(", ");
                }

                WriteExpression(sb, sqlQuery, ordering.Expression);

                if (ordering.Direction.IsDescending)
                {
                    sb.Append(" DESC");
                }
            }

            if (sqlQuery.Offset.IsSome())
            {
                if (sqlQuery.Ordering.IsEmpty)
                {
                    throw new Exception("To enable offset and limit, it is needed to use also order by clause");
                }
            }

            if (!sqlQuery.Ordering.IsEmpty)
            {
                var offset = sqlQuery.Offset.ToNullable();
                var limit = sqlQuery.Limit.ToNullable();

                sb.Append(" OFFSET ");
                sb.Append(offset ?? 0);
                sb.Append(" ROWS");

                if (limit.HasValue)
                {
                    sb.Append(" FETCH FIRST ");
                    sb.Append(limit.Value);
                    sb.Append(" ROWS ONLY");
                }
            }
        }

        private void WriteInnerQuery(StringBuilder sb, QueryContent query, List<string> variables, Dictionary<string, List<Variable>> variablesMappings, bool isDistinct)
        {
            sb.Append("SELECT");

            if (isDistinct)
            {
                sb.Append(" DISTINCT");
            }

            if (query.IsSelectQuery)
            {
                WriteInnerSelectQueryContent(sb, ((QueryContent.SelectQuery) query).Item, variables, variablesMappings);
            }
            else if (query.IsNoResultQuery)
            {
                WriteInnerNoResultQueryContent(sb, variables);
            }
            else if (query.IsSingleEmptyResultQuery)
            {
                WriteInnerSingleEmptyResultQueryContent(sb, variables);
            }
            else
            {
                throw new ArgumentException("Produced query does not have supported type", nameof(query));
            }
        }

        private void WriteInnerSingleEmptyResultQueryContent(StringBuilder sb, List<string> variables)
        {
            var firstVariable = true;

            foreach (var variable in variables)
            {
                if (firstVariable)
                {
                    sb.Append(" ");
                    firstVariable = false;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append("NULL AS ");
                sb.Append(variable);
            }
        }

        private void WriteInnerNoResultQueryContent(StringBuilder sb, List<string> variables)
        {
            WriteInnerSingleEmptyResultQueryContent(sb, variables);
            sb.Append(" WHERE 1=0");
        }

        private void WriteInnerSelectQueryContent(StringBuilder sb, InnerQuery query, List<string> variables, Dictionary<string, List<Variable>> variablesMappings)
        {
            var isFirstVariable = true;
            foreach (var variableName in variables)
            {
                if (isFirstVariable)
                {
                    sb.Append(" ");
                    isFirstVariable = false;
                }
                else
                {
                    sb.Append(", ");
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
                    WriteExpression(sb, query, new TypedExpression(_databaseSchema.NullType, _databaseSchema.NullType, TypedExpressionContent.Null));
                }

                sb.Append(" AS ");
                sb.Append(variableName);
            }

            sb.Append(" FROM");

            var isFirstSource = true;
            foreach (var innerSource in query.Sources)
            {
                sb.Append(!isFirstSource ? " INNER JOIN " : " ");

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

                if (!isFirstSource)
                {
                    sb.Append(" ON 1=1");
                }
                else
                {
                    isFirstSource = false;
                }
            }

            foreach (var leftJoined in query.LeftJoinedSources)
            {
                sb.Append(" LEFT JOIN ");
                WriteInnerSource(sb, leftJoined.Item1);
                sb.Append(" AS ");
                if (query.NamingProvider.TryGetSourceName(leftJoined.Item1, out var sourceName))
                {
                    sb.Append(sourceName);
                }
                else
                {
                    throw new InvalidOperationException($"Name for source has not been found. Source: {leftJoined}");
                }
                sb.Append(" ON ");
                WriteCondition(sb, query, leftJoined.Item2);
            }

            var isFirstCondition = true;
            foreach (var condition in query.Filters)
            {
                if (!isFirstCondition)
                {
                    sb.Append(" AND ");
                }
                else
                {
                    sb.Append(" WHERE ");
                    isFirstCondition = false;
                }

                WriteCondition(sb, query, condition);
            }
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

        private static void WriteVariable(StringBuilder sb, SqlQuery query, Variable variable)
        {
            if (query.NamingProvider.TryGetVariableName(variable, out var name))
            {
                sb.Append(name);
            }
            else
            {
                throw new InvalidOperationException("Cannot find name for a variable in query");
            }
        }

        private static void WriteVariable(StringBuilder sb, InnerQuery query, Variable variable)
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

        private static void WriteExpression(StringBuilder sb, InnerQuery query, TypedExpression expression)
        {
            var writer = new MsSqlExpressionWriter(sb, (x, variable) => WriteVariable(x, query, variable));
            SqlDatabaseWriterHelper.ProcessExpression(writer, expression);
        }

        private static void WriteExpression(StringBuilder sb, SqlQuery query, TypedExpression expression)
        {
            var writer = new MsSqlExpressionWriter(sb, (x, variable) => WriteVariable(x, query, variable));
            SqlDatabaseWriterHelper.ProcessExpression(writer, expression);
        }

        private static void WriteCondition(StringBuilder sb, InnerQuery query, TypedCondition condition)
        {
            var writer = new MsSqlExpressionWriter(sb, (x, variable) => WriteVariable(x, query, variable));
            SqlDatabaseWriterHelper.ProcessCondition(writer, condition);
        }

        private class MsSqlExpressionWriter: SqlDatabaseWriterHelper.ISqlExpressionWriter
        {
            private readonly StringBuilder _sb;
            private readonly Action<StringBuilder, Variable> _writeVariableAction;

            public MsSqlExpressionWriter(StringBuilder sb, Action<StringBuilder, Variable> writeVariableAction)
            {
                _sb = sb;
                _writeVariableAction = writeVariableAction;
            }

            private void ProcessCondition(TypedCondition condition)
            {
                SqlDatabaseWriterHelper.ProcessCondition(this, condition);
            }

            private void ProcessExpression(TypedExpression expression)
            {
                SqlDatabaseWriterHelper.ProcessExpression(this, expression);
            }

            private void ProcessLiteral(Literal literal)
            {
                SqlDatabaseWriterHelper.ProcessLiteral(this, literal);
            }

            /// <inheritdoc />
            public void WriteNull()
            {
                _sb.Append("NULL");
            }

            /// <inheritdoc />
            public void WriteBinaryNumericOperation(Algebra.ArithmeticOperator @operator, TypedExpression leftOperand, TypedExpression rightOperand)
            {
                _sb.Append("(");

                ProcessExpression(leftOperand);

                if (@operator.IsAdd)
                {
                    _sb.Append("+");
                }
                else if (@operator.IsSubtract)
                {
                    _sb.Append("-");
                }
                else if (@operator.IsMultiply)
                {
                    _sb.Append("*");
                }
                else if (@operator.IsDivide)
                {
                    _sb.Append("/");
                }
                else
                {
                    throw new InvalidOperationException($"Operator not yet supported: {@operator}");
                }

                ProcessExpression(rightOperand);

                _sb.Append(")");
            }

            /// <inheritdoc />
            public void WriteSwitch(FSharpList<TypedCaseStatement> caseStatements)
            {
                _sb.Append("CASE");

                foreach (var caseStatement in caseStatements)
                {
                    _sb.Append(" WHEN ");
                    ProcessCondition(caseStatement.Condition);
                    _sb.Append(" THEN ");
                    ProcessExpression(caseStatement.Expression);
                }

                _sb.Append(" END");
            }

            /// <inheritdoc />
            public void WriteCoalesce(FSharpList<TypedExpression> expressions)
            {
                _sb.Append("COALESCE(");

                var isFirstExpression = true;
                foreach (var expression in expressions)
                {
                    if (isFirstExpression)
                    {
                        isFirstExpression = false;
                    }
                    else
                    {
                        _sb.Append(", ");
                    }

                    ProcessExpression(expression);
                }

                _sb.Append(")");
            }

            /// <inheritdoc />
            public void WriteVariable(Variable variable)
            {
                _writeVariableAction(_sb, variable);
            }

            /// <inheritdoc />
            public void WriteIriSafeVariable(Variable variable)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteConcatenation(FSharpList<TypedExpression> expressions)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteBooleanExpression(TypedCondition condition)
            {
                _sb.Append("IIF(");
                ProcessCondition(condition);
                _sb.Append(",1,0)");
            }

            /// <inheritdoc />
            public void WriteConstant(string literal)
            {
                _sb.Append("'");
                _sb.Append(literal.Replace("\'", "\'\'"));
                _sb.Append("'");
            }

            /// <inheritdoc />
            public void WriteConstant(double literal)
            {
                _sb.Append(literal.ToString(CultureInfo.InvariantCulture));
            }

            /// <inheritdoc />
            public void WriteConstant(int literal)
            {
                _sb.Append(literal);
            }

            /// <inheritdoc />
            public void WriteConstant(DateTime literal)
            {
                _sb.AppendFormat("\'{0:yyyy-MM-dd HH:mm:ss.fff}\'", literal);
            }

            /// <inheritdoc />
            public void WriteTrue()
            {
                _sb.Append("1=1");
            }

            /// <inheritdoc />
            public void WriteFalse()
            {
                _sb.Append("1=0");
            }

            /// <inheritdoc />
            public void WriteComparison(Algebra.Comparisons comparison, TypedExpression leftOperand, TypedExpression rightOperand)
            {
                ProcessExpression(leftOperand);

                if (comparison.IsEqualTo)
                {
                    _sb.Append("=");
                }
                else if (comparison.IsGreaterOrEqualThan)
                {
                    _sb.Append(">=");
                }
                else if (comparison.IsGreaterThan)
                {
                    _sb.Append(">");
                }
                else if (comparison.IsLessOrEqualThan)
                {
                    _sb.Append("<=");
                }
                else if (comparison.IsLessThan)
                {
                    _sb.Append("<");
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported comparison operator: {comparison}");
                }

                ProcessExpression(rightOperand);
            }

            /// <inheritdoc />
            public void WriteConjunction(FSharpList<TypedCondition> conditions)
            {
                _sb.Append("(");
                var isFirst = true;

                foreach (var condition in conditions)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        _sb.Append(" AND ");
                    }

                    ProcessCondition(condition);
                }

                _sb.Append(")");
            }

            /// <inheritdoc />
            public void WriteDisjunction(FSharpList<TypedCondition> conditions)
            {
                _sb.Append("(");
                var isFirst = true;

                foreach (var condition in conditions)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        _sb.Append(" OR ");
                    }

                    ProcessCondition(condition);
                }

                _sb.Append(")");
            }

            /// <inheritdoc />
            public void WriteEqualVariableTo(Variable variable, Literal literal)
            {
                WriteVariable(variable);
                _sb.Append("=");
                ProcessLiteral(literal);
            }

            /// <inheritdoc />
            public void WriteEqualVariables(Variable leftVariable, Variable rightVariable)
            {
                WriteVariable(leftVariable);
                _sb.Append("=");
                WriteVariable(rightVariable);
            }

            /// <inheritdoc />
            public void WriteIsNull(Variable variable)
            {
                WriteVariable(variable);
                _sb.Append(" IS NULL");
            }

            /// <inheritdoc />
            public void WriteLanguageMatch(TypedExpression langExpression, TypedExpression langRangeExpression)
            {
                _sb.Append("LOWER(");
                ProcessExpression(langExpression);
                _sb.Append(") LIKE LOWER(");
                ProcessExpression(langRangeExpression); // TODO: Should be handled so the language matching will work properly
                _sb.Append(")");
            }

            /// <inheritdoc />
            public void WriteLikeMatch(TypedExpression expression, string pattern)
            {
                ProcessExpression(expression);
                _sb.Append(" LIKE \'");
                _sb.Append(pattern);
                _sb.Append('\'');
            }

            /// <inheritdoc />
            public void WriteNot(TypedCondition condition)
            {
                _sb.Append("NOT(");
                ProcessCondition(condition);
                _sb.Append(")");
            }

            /// <inheritdoc />
            public void WriteCastedExpression(ISqlColumnType actualType, ISqlColumnType expectedType, Action writeExpressionFunc)
            {
                if (!actualType.Equals(expectedType))
                {
                    _sb.Append("CAST(");
                    writeExpressionFunc();
                    _sb.Append(" AS ");

                    if (!(expectedType is MsSqlColumnType columnType))
                    {
                        throw new InvalidOperationException($"Trying to cast to a type that is not specific to MS SQL, type: {expectedType.GetType()}");
                    }
                    else
                    {
                        _sb.Append(columnType.DbString);
                    }

                    _sb.Append(")");
                }
                else
                {
                    writeExpressionFunc();
                }
            }
        }
    }
}
