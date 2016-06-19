using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;
using FilterPattern = Slp.Evi.Storage.Sparql.Algebra.Patterns.FilterPattern;
using ISparqlExpression = VDS.RDF.Query.Expressions.ISparqlExpression;
using Slp.Evi.Storage.Utils;
using VDS.RDF.Parsing;

namespace Slp.Evi.Storage.Sparql.Builder
{
    /// <summary>
    /// SPARQL algebra builder.
    /// </summary>
    public class SparqlBuilder
    {
        /// <summary>
        /// Processes the specified context.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        /// <exception cref="System.Exception">Cannot handle unknown query type</exception>
        public ISparqlQuery Process(QueryContext context)
        {
            switch (context.OriginalQuery.QueryType)
            {
                case SparqlQueryType.Ask:
                    return ProcessAsk(context);
                case SparqlQueryType.Construct:
                    return ProcessConstruct(context);
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    return ProcessDescribe(context);
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    return ProcessSelect(context);
                default:
                    throw new Exception("Cannot handle unknown query type");
            }
        }

        /// <summary>
        /// Processes the ask query.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private ISparqlQuery ProcessAsk(QueryContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the construct query.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        private ISparqlQuery ProcessConstruct(QueryContext context)
        {
            return ProcessAlgebra(context.OriginalAlgebra, context);
        }

        /// <summary>
        /// Processes the describe query.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private ISparqlQuery ProcessDescribe(QueryContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the select query.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        private ISparqlQuery ProcessSelect(QueryContext context)
        {
            return ProcessAlgebra(context.OriginalAlgebra, context);
        }

        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="originalAlgebra">The original algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private ISparqlQuery ProcessAlgebra(ISparqlAlgebra originalAlgebra, QueryContext context)
        {
            if (originalAlgebra is Select)
            {
                var orSel = (Select)originalAlgebra;
                var innerAlgebra = ProcessAlgebra(orSel.InnerAlgebra, context);

                if (!orSel.IsSelectAll)
                {
                    return new SelectModifier(innerAlgebra, orSel.SparqlVariables.Select(x => x.Name).ToList());
                }
                else
                {
                    return new SelectModifier(innerAlgebra, innerAlgebra.Variables);
                }
            }
            else if (originalAlgebra is IBgp)
            {
                var orBgp = (IBgp)originalAlgebra;
                return ProcessTriplePatterns(orBgp.TriplePatterns, context);
            }
            else if (originalAlgebra is Union)
            {
                var orUnion = (Union)originalAlgebra;
                var left = (IGraphPattern)ProcessAlgebra(orUnion.Lhs, context);
                var right = (IGraphPattern)ProcessAlgebra(orUnion.Rhs, context);
                return new UnionPattern(new IGraphPattern[] { left, right });
            }
            else if (originalAlgebra is LeftJoin)
            {
                var leftJoin = (LeftJoin)originalAlgebra;
                var left = (IGraphPattern)ProcessAlgebra(leftJoin.Lhs, context);
                var right = (IGraphPattern)ProcessAlgebra(leftJoin.Rhs, context);
                var condition = ProcessCondition(leftJoin.Filter.Expression, context);

                return new LeftJoinPattern(left, right, condition);
            }
            else if (originalAlgebra is Filter)
            {
                var filter = (Filter) originalAlgebra;
                var inner = (IGraphPattern) ProcessAlgebra(filter.InnerAlgebra, context);
                var innerExpression = ProcessCondition(filter.SparqlFilter.Expression, context);

                return new FilterPattern(inner, innerExpression);
            }
            else if (originalAlgebra is Join)
            {
                var join = (Join) originalAlgebra;
                var left = (IGraphPattern) ProcessAlgebra(join.Lhs, context);
                var right = (IGraphPattern) ProcessAlgebra(join.Rhs, context);

                return new JoinPattern(new IGraphPattern[] {left, right});
            }
            else if (originalAlgebra is Extend)
            {
                var extend = (Extend) originalAlgebra;
                var inner = (IGraphPattern) ProcessAlgebra(extend.InnerAlgebra, context);

                var expression = ProcessExpression(extend.AssignExpression, context);

                return new ExtendPattern(inner, extend.VariableName, expression);
            }

            throw new NotImplementedException();

            // http://www.dotnetrdf.org/api/~VDS.RDF.Query.Algebra.html
            //Ask
            //AskAnyTriples
            //AskBgp
            //AskUnion
            //BaseArbitraryLengthPathOperator
            //BaseMultiset
            //BasePathOperator
            //BaseSet
            //Bgp
            //Bindings
            //Distinct
            //ExistsJoin
            //Extend
            //Filter
            //FilteredProduct
            //FullTextQuery
            //Graph
            //GroupBy
            //GroupMultiset
            //Having
            //IdentityFilter
            //IdentityMultiset
            //Join
            //LazyBgp
            //LazyUnion
            //LeftJoin
            //Minus
            //Multiset
            //NegatedPropertySet
            //NullMultiset
            //NullOperator
            //OneOrMorePath
            //OrderBy
            //ParallelJoin
            //ParallelUnion
            //PartitionedMultiset
            //PropertyFunction
            //PropertyPath
            //Reduced
            //SameTermFilter
            //Select
            //SelectDistinctGraphs
            //Service
            //Set
            //SetDistinctnessComparer
            //SingleValueRestrictionFilter
            //Slice
            //SubQuery
            //Table
            //Union
            //VariableRestrictionFilter
            //ZeroLengthPath
            //ZeroOrMorePath

        }

        private ISparqlCondition ProcessCondition(ISparqlExpression expression, QueryContext context)
        {
            var processed = ProcessExpression(expression, context);

            if (processed is ISparqlCondition)
            {
                return (ISparqlCondition)processed;
            }
            else if (processed is NodeExpression)
            {
                var nodeExpression = (NodeExpression)processed;
                var node = nodeExpression.Node;

                if (node.EffectiveType == XmlSpecsHelper.XmlSchemaDataTypeBoolean)
                {
                    if (node.AsBoolean())
                    {
                        return new BooleanTrueExpression();
                    }
                    else
                    {
                        return new BooleanFalseExpression();
                    }
                }
            }

            throw new ArgumentException("The expression needs to be convertible to condition", nameof(expression));
        }

        private Algebra.ISparqlExpression ProcessExpression(ISparqlExpression expression, QueryContext context)
        {
            if (expression is BoundFunction)
            {
                var boundFunction = (BoundFunction) expression;
                return new IsBoundExpression(boundFunction.Variables.Single());
            }
            else if (expression is AndExpression)
            {
                var conjunction = (AndExpression)expression;
                var left = ProcessCondition(conjunction.Arguments.ElementAt(0), context);
                var right = ProcessCondition(conjunction.Arguments.ElementAt(1), context);
                return new ConjunctionExpression(new ISparqlCondition[] {left, right});
            }
            else if (expression is OrExpression)
            {
                var conjunction = (OrExpression)expression;
                var left = ProcessCondition(conjunction.Arguments.ElementAt(0), context);
                var right = ProcessCondition(conjunction.Arguments.ElementAt(1), context);
                return new DisjunctionExpression(new ISparqlCondition[] { left, right });
            }
            else if (expression is NotExpression)
            {
                var notExpression = (NotExpression) expression;
                return new NegationExpression(ProcessCondition(notExpression.Arguments.Single(), context));
            }
            else if (expression is GreaterThanExpression)
            {
                var comparison = (GreaterThanExpression) expression;
                var left = ProcessExpression(comparison.Arguments.ElementAt(0), context);
                var right = ProcessExpression(comparison.Arguments.ElementAt(1), context);
                return new ComparisonExpression(left, right, ComparisonTypes.GreaterThan);
            }
            else if (expression is GreaterThanOrEqualToExpression)
            {
                var comparison = (GreaterThanOrEqualToExpression)expression;
                var left = ProcessExpression(comparison.Arguments.ElementAt(0), context);
                var right = ProcessExpression(comparison.Arguments.ElementAt(1), context);
                return new ComparisonExpression(left, right, ComparisonTypes.GreaterOrEqualThan);
            }
            else if (expression is LessThanExpression)
            {
                var comparison = (LessThanExpression)expression;
                var left = ProcessExpression(comparison.Arguments.ElementAt(0), context);
                var right = ProcessExpression(comparison.Arguments.ElementAt(1), context);
                return new ComparisonExpression(left, right, ComparisonTypes.LessThan);
            }
            else if (expression is LessThanOrEqualToExpression)
            {
                var comparison = (LessThanOrEqualToExpression)expression;
                var left = ProcessExpression(comparison.Arguments.ElementAt(0), context);
                var right = ProcessExpression(comparison.Arguments.ElementAt(1), context);
                return new ComparisonExpression(left, right, ComparisonTypes.LessOrEqualThan);
            }
            else if (expression is EqualsExpression)
            {
                var comparison = (EqualsExpression)expression;
                var left = ProcessExpression(comparison.Arguments.ElementAt(0), context);
                var right = ProcessExpression(comparison.Arguments.ElementAt(1), context);
                return new ComparisonExpression(left, right, ComparisonTypes.EqualTo);
            }
            else if (expression is NotEqualsExpression)
            {
                var comparison = (NotEqualsExpression)expression;
                var left = ProcessExpression(comparison.Arguments.ElementAt(0), context);
                var right = ProcessExpression(comparison.Arguments.ElementAt(1), context);
                return new ComparisonExpression(left, right, ComparisonTypes.NotEqualTo);
            }
            else if (expression is VariableTerm)
            {
                var term = (VariableTerm) expression;
                var variable = term.Variables.Single();
                return new VariableExpression(variable);
            }
            else if (expression is ConstantTerm)
            {
                var term = (ConstantTerm) expression;
                return new NodeExpression(term.Node());
            }

            throw new NotImplementedException();
        }

        private IGraphPattern ProcessTriplePatterns(IEnumerable<ITriplePattern> triplePatterns, QueryContext context)
        {
            List<IGraphPattern> joinedQueries = new List<IGraphPattern>();

            foreach (var triplePattern in triplePatterns.OfType<VDS.RDF.Query.Patterns.TriplePattern>())
            {
                var processed = new Algebra.Patterns.TriplePattern(triplePattern.Subject, triplePattern.Predicate,
                    triplePattern.Object);
                joinedQueries.Add(processed);
            }

            IGraphPattern currentQuery;
            if (joinedQueries.Count == 0)
            {
                currentQuery = new EmptyPattern();
            }
            else if (joinedQueries.Count == 1)
            {
                currentQuery = joinedQueries[0];
            }
            else
            {
                currentQuery = new JoinPattern(joinedQueries);
            }

            // TODO:
            //    http://www.dotnetrdf.org/api/dotNetRDF~VDS.RDF.Query.Patterns.ITriplePattern.html
            //    //BindPattern
            //    //FilterPattern
            //    //LetPattern
            //    //PropertyFunctionPattern
            //    //PropertyPathPattern
            //    //SubQueryPattern
            //    //TriplePattern

            return currentQuery;
        }
    }
}
