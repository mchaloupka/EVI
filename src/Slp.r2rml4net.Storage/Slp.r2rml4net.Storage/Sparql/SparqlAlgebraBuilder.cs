using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace Slp.r2rml4net.Storage.Sparql
{
    public class SparqlAlgebraBuilder
    {
        public SparqlAlgebraBuilder()
        {
        }

        public ISparqlQuery Process(QueryContext context)
        {
            var originalQuery = context.OriginalQuery;

            switch (originalQuery.QueryType)
            {
                case SparqlQueryType.Ask:
                    return ProcessAsk(originalQuery, context);
                case SparqlQueryType.Construct:
                    return ProcessConstruct(originalQuery, context);
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    return ProcessDescribe(originalQuery, context);
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    return ProcessSelect(originalQuery, context);
                default:
                    throw new Exception("Cannot handle unknown query type");
            }
        }

        private ISparqlQuery ProcessAsk(SparqlQuery originalQuery, QueryContext context)
        {
            throw new NotImplementedException();
        }

        private ISparqlQuery ProcessConstruct(SparqlQuery originalQuery, QueryContext context)
        {
            throw new NotImplementedException();
        }

        private ISparqlQuery ProcessDescribe(SparqlQuery originalQuery, QueryContext context)
        {
            throw new NotImplementedException();
        }

        private ISparqlQuery ProcessSelect(SparqlQuery originalQuery, QueryContext context)
        {
            var originalAlgebra = originalQuery.ToAlgebra();

            var resultingAlgebra = ProcessAlgebra(originalAlgebra, context);

            return resultingAlgebra;
        }

        private ISparqlQuery ProcessAlgebra(ISparqlAlgebra originalAlgebra, QueryContext context)
        {
            if (originalAlgebra is Select)
            {
                var orSel = (Select)originalAlgebra;
                var innerAlgebra = ProcessAlgebra(orSel.InnerAlgebra, context);

                if (!orSel.IsSelectAll)
                {
                    return new SelectOp(innerAlgebra, orSel.SparqlVariables);
                }
                else
                {
                    return new SelectOp(innerAlgebra);
                }
            }
            else if (originalAlgebra is VDS.RDF.Query.Algebra.Bgp)
            {
                var orBgp = (VDS.RDF.Query.Algebra.Bgp)originalAlgebra;
                return ProcessITriplePatterns(orBgp.TriplePatterns, context);
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

        private ISparqlQuery ProcessITriplePatterns(IEnumerable<VDS.RDF.Query.Patterns.ITriplePattern> enumerable, QueryContext context)
        {
            var triples = enumerable.OfType<VDS.RDF.Query.Patterns.TriplePattern>();
            ISparqlQuery triplesOp = null;

            // Process triples
            if (triples.Any())
            {
                if (triples.Count() > 1)
                {
                    var join = new JoinOp();

                    foreach (var triple in triples)
                    {
                        join.AddToJoin(new BgpOp(triple.Object, triple.Predicate, triple.Subject));
                    }

                    triplesOp = join;
                }
                else
                {
                    var triple = triples.First();
                    triplesOp = new BgpOp(triple.Object, triple.Predicate, triple.Subject);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return triplesOp;

            // TODO: Process other parts

//http://www.dotnetrdf.org/api/dotNetRDF~VDS.RDF.Query.Patterns.ITriplePattern.html
//BindPattern
//FilterPattern
//LetPattern
//PropertyFunctionPattern
//PropertyPathPattern
//SubQueryPattern
//TriplePattern
        }
    }
}
