using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Condition;
using Slp.r2rml4net.Storage.Relational.Query.Source;
using Slp.r2rml4net.Storage.Relational.Query.ValueBinder;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Modifiers;
using Slp.r2rml4net.Storage.Sparql.Algebra.Patterns;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Relational.Builder
{
    /// <summary>
    /// Relational builder
    /// </summary>
    public class RelationalBuilder
        : IModifierVisitor, IPatternVisitor
    {
        /// <summary>
        /// Processes the specified algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The context.</param>
        /// <returns>RelationalQuery.</returns>
        /// <exception cref="System.ArgumentException">Unexpected type;algebra</exception>
        public RelationalQuery Process(ISparqlQuery algebra, QueryContext context)
        {
            if (algebra is IModifier)
            {
                return (RelationalQuery)((IModifier) algebra).Accept(this, context);
            }
            else if (algebra is IGraphPattern)
            {
                return (RelationalQuery)((IGraphPattern)algebra).Accept(this, context);
            }
            else
            {
                throw new ArgumentException("Unexpected type", "algebra");
            }
        }

        #region PatternVisitor
        /// <summary>
        /// Visits <see cref="EmptyPattern" />
        /// </summary>
        /// <param name="emptyPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EmptyPattern emptyPattern, object data)
        {
            return new RelationalQuery(
                new CalculusModel(
                    new ICalculusVariable[] {},
                    new ICondition[] {}),
                new IValueBinder[] {});
        }

        /// <summary>
        /// Visits <see cref="FilterPattern" />
        /// </summary>
        /// <param name="filterPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(FilterPattern filterPattern, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="NotMatchingPattern" />
        /// </summary>
        /// <param name="notMatchingPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NotMatchingPattern notMatchingPattern, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="GraphPattern" />
        /// </summary>
        /// <param name="graphPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(GraphPattern graphPattern, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="JoinPattern" />
        /// </summary>
        /// <param name="joinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(JoinPattern joinPattern, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="LeftJoinPattern" />
        /// </summary>
        /// <param name="leftJoinPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(LeftJoinPattern leftJoinPattern, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="MinusPattern" />
        /// </summary>
        /// <param name="minusPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(MinusPattern minusPattern, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="TriplePattern" />
        /// </summary>
        /// <param name="triplePattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        /// <exception cref="System.InvalidOperationException">The triple pattern should not be present when transforming to relational form.</exception>
        public object Visit(TriplePattern triplePattern, object data)
        {
            throw new InvalidOperationException("The triple pattern should not be present when transforming to relational form.");
        }

        /// <summary>
        /// Visits <see cref="UnionPattern" />
        /// </summary>
        /// <param name="unionPattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(UnionPattern unionPattern, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="RestrictedTriplePattern" />
        /// </summary>
        /// <param name="restrictedTriplePattern">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(RestrictedTriplePattern restrictedTriplePattern, object data)
        {
            var context = (QueryContext) data;
            List<ICondition> conditions = new List<ICondition>();
            List<IValueBinder> valueBinders = new List<IValueBinder>();

            ISqlCalculusSource source;
            ISqlCalculusSource refSource;
            
            ProcessTriplePatternSource(restrictedTriplePattern, conditions, out source, out refSource, context);
            ProcessTriplePatternSubject(restrictedTriplePattern, conditions, valueBinders, source, context);
            ProcessTriplePatternPredicate(restrictedTriplePattern, conditions, valueBinders, source, context);

            if (restrictedTriplePattern.RefObjectMap != null)
            {
                ProcessTriplePatternRefObject(restrictedTriplePattern, conditions, valueBinders, source, refSource, context);

                conditions.Add(new TupleFromSourceCondition(refSource.Variables, refSource));
            }
            else
            {
                ProcessTriplePatternObject(restrictedTriplePattern, conditions, valueBinders, source, context);
            }

            conditions.Add(new TupleFromSourceCondition(source.Variables, source));

            return new RelationalQuery(
                new CalculusModel(
                    valueBinders.SelectMany(x => x.NeededCalculusVariables),
                    conditions),
                valueBinders);
        }
        #endregion

        #region ModifierVisitor
        /// <summary>
        /// Visits <see cref="SelectModifier" />
        /// </summary>
        /// <param name="selectModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SelectModifier selectModifier, object data)
        {
            var inner = Process(selectModifier.InnerQuery, (QueryContext) data);

            var requestedVariables = selectModifier.Variables.ToList();
            var providedVariables = inner.ValueBinders.Select(x => x.VariableName).ToList();

            var neededToAdd = requestedVariables.Where(x => !providedVariables.Contains(x)).ToList();
            var neededToRemove = providedVariables.Where(x => !requestedVariables.Contains(x)).ToList();

            if ((neededToAdd.Count == 0) && (neededToRemove.Count == 0))
            {
                return inner;
            }
            else
            {
                var valueBinders = new List<IValueBinder>(inner.ValueBinders
                    .Where(x => requestedVariables.Contains(x.VariableName)));

                valueBinders.AddRange(
                    neededToAdd.Select(addName => new EmptyValueBinder(addName)).Cast<IValueBinder>());

                return new RelationalQuery(inner.Model, valueBinders);
            }
        }
        #endregion

        /// <summary>
        /// Processes the triple pattern source.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="source">The source.</param>
        /// <param name="refSource">The reference source.</param>
        /// <param name="context">The context.</param>
        private void ProcessTriplePatternSource(RestrictedTriplePattern triplePattern, List<ICondition> conditions, out ISqlCalculusSource source, out ISqlCalculusSource refSource, QueryContext context)
        {
            refSource = null;
            source = null;

            source = GetTripleMapSource(triplePattern.TripleMap, context);

            if (triplePattern.RefObjectMap != null)
            {
                refSource = GetTripleMapSource(triplePattern.RefObjectMap.ParentTriplesMap, context);

                foreach (var joinCondition in triplePattern.RefObjectMap.JoinConditions)
                {
                    var sourceCalculusVariable = source.GetVariable(joinCondition.ChildColumn);
                    var refSourceCalculusVariable = refSource.GetVariable(joinCondition.ParentColumn);

                    conditions.Add(new EqualVariablesCondition(sourceCalculusVariable, refSourceCalculusVariable));
                }
            }
        }

        /// <summary>
        /// Gets the triple map source.
        /// </summary>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="context">The context.</param>
        /// <returns>ISqlCalculusSource.</returns>
        /// <exception cref="System.ArgumentException">Unknown source;tripleMap</exception>
        private static ISqlCalculusSource GetTripleMapSource(ITriplesMap tripleMap, QueryContext context)
        {
            var sqlTableName = context.Mapping.Cache.GetSqlTable(tripleMap);
            var sqlStatement = context.Mapping.Cache.GetSqlStatement(tripleMap);

            if (!string.IsNullOrEmpty(sqlStatement))
            {
                throw new NotImplementedException();
            }
            else if (!string.IsNullOrEmpty(sqlTableName))
            {
                return new SqlTable(context.SchemaProvider.GetTableInfo(sqlTableName));
            }
            else
            {
                throw new ArgumentException("Unknown source", "tripleMap");
            }
        }

        /// <summary>
        /// Processes the triple pattern predicate.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The query context.</param>
        private void ProcessTriplePatternPredicate(RestrictedTriplePattern triplePattern, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource source, QueryContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the triple pattern subject.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The query context.</param>
        private void ProcessTriplePatternSubject(RestrictedTriplePattern triplePattern, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource source, QueryContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the triple pattern object.
        /// </summary>
        /// <param name="restrictedTriplePattern">The restricted triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="source">The source.</param>
        /// <param name="context">The query context.</param>
        private void ProcessTriplePatternObject(RestrictedTriplePattern restrictedTriplePattern, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource source, QueryContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the triple pattern reference object.
        /// </summary>
        /// <param name="restrictedTriplePattern">The restricted triple pattern.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="valueBinders">The value binders.</param>
        /// <param name="source">The source.</param>
        /// <param name="refSource">The reference source.</param>
        /// <param name="context">The query context.</param>
        private void ProcessTriplePatternRefObject(RestrictedTriplePattern restrictedTriplePattern, List<ICondition> conditions, List<IValueBinder> valueBinders, ISqlCalculusSource source, ISqlCalculusSource refSource, QueryContext context)
        {
            throw new NotImplementedException();
        }
    }
}
