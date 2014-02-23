using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Sparql
{
    public class SparqlAlgebraBuilder
    {
        private QueryContext context;

        public SparqlAlgebraBuilder(QueryContext context)
        {
            this.context = context;
        }

        public ISparqlQuery Process()
        {
            var originalQuery = this.context.OriginalQuery;

            switch (originalQuery.QueryType)
            {
                case SparqlQueryType.Ask:
                    return ProcessAsk(originalQuery);
                    break;
                case SparqlQueryType.Construct:
                    return ProcessConstruct(originalQuery);
                    break;
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    return ProcessDescribe(originalQuery);
                    break;
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    return ProcessSelect(originalQuery);
                    break;
                default:
                    throw new Exception("Cannot handle unknown query type");
            }
        }

        private ISparqlQuery ProcessAsk(SparqlQuery originalQuery)
        {
            throw new NotImplementedException();
        }

        private ISparqlQuery ProcessConstruct(SparqlQuery originalQuery)
        {
            throw new NotImplementedException();
        }

        private ISparqlQuery ProcessDescribe(SparqlQuery originalQuery)
        {
            throw new NotImplementedException();
        }

        private ISparqlQuery ProcessSelect(SparqlQuery originalQuery)
        {
            var originalAlgebra = originalQuery.ToAlgebra();

            var resultingAlgebra = ProcessAlgebra(originalAlgebra);

            return resultingAlgebra;
        }

        private ISparqlQuery ProcessAlgebra(VDS.RDF.Query.Algebra.ISparqlAlgebra originalAlgebra)
        {
            if (originalAlgebra is VDS.RDF.Query.Algebra.Select)
            {
                var orSel = (VDS.RDF.Query.Algebra.Select)originalAlgebra;
                var innerAlgebra = ProcessAlgebra(orSel.InnerAlgebra);

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
                return ProcessITriplePatterns(orBgp.TriplePatterns);
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

        private ISparqlQuery ProcessITriplePatterns(IEnumerable<VDS.RDF.Query.Patterns.ITriplePattern> enumerable)
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
