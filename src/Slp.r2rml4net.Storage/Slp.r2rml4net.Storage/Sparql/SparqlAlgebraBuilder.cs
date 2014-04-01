using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

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
            else if (originalAlgebra is IBgp)
            {
                var orBgp = (IBgp)originalAlgebra;
                return ProcessITriplePatterns(orBgp.TriplePatterns, context);
            }
            else if(originalAlgebra is Slice)
            {
                var orSlice = (Slice)originalAlgebra;
                var inner = ProcessAlgebra(orSlice.InnerAlgebra, context);
                var slice = new SliceOp(inner);

                if (orSlice.Limit > -1)
                    slice.Limit = orSlice.Limit;
                if (orSlice.Offset > 0)
                    slice.Offset = orSlice.Offset;

                return slice;
            }
            else if(originalAlgebra is OrderBy)
            {
                var orOrderBy = (OrderBy)originalAlgebra;
                var inner = ProcessAlgebra(orOrderBy.InnerAlgebra, context);
                var order = new OrderByOp(inner);

                var ordering = orOrderBy.Ordering;

                while(ordering != null)
                {
                    order.AddOrdering(ProcessExpression(ordering.Expression, context), ordering.Descending);
                    ordering = ordering.Child;
                }

                return order;
            }
            else if(originalAlgebra is Distinct)
            {
                var orDistinct = (Distinct)originalAlgebra;
                var inner = ProcessAlgebra(orDistinct.InnerAlgebra, context);

                return new DistinctOp(inner);
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

        private ISparqlQueryExpression ProcessExpression(ISparqlExpression sparqlExpression, QueryContext context)
        {
            if(sparqlExpression is VariableTerm)
            {
                var orVar = (VariableTerm)sparqlExpression;
                return new VariableExpression(orVar.Variables.Single());
            }

            throw new NotImplementedException();

// http://www.dotnetrdf.org/api/dotNetRDF~VDS.RDF.Query.Expressions.ISparqlExpression.html
//AdditionExpression
//DivisionExpression
//MinusExpression
//MultiplicationExpression
//SubtractionExpression
//BaseBinaryExpression
//BaseUnaryExpression
//EqualsExpression
//GreaterThanExpression
//GreaterThanOrEqualToExpression
//LessThanExpression
//LessThanOrEqualToExpression
//NotEqualsExpression
//AndExpression
//NotExpression
//OrExpression
//BNodeFunction
//EFunction
//LocalNameFunction
//MaxFunction
//MinFunction
//NamespaceFunction
//NowFunction
//PiFunction
//Sha1Function
//StringJoinFunction
//SubstringFunction
//MD5HashFunction
//Sha256HashFunction
//CartesianFunction
//CubeFunction
//EFunction
//FactorialFunction
//LeviathanNaturalLogFunction
//LogFunction
//PowerFunction
//PythagoreanDistanceFunction
//RandomFunction
//ReciprocalFunction
//RootFunction
//SquareFunction
//SquareRootFunction
//TenFunction
//BaseTrigonometricFunction
//CosecantFunction
//CosineFunction
//CotangentFunction
//DegreesToRadiansFunction
//RadiansToDegreesFunction
//SecantFunction
//SineFunction
//TangentFunction
//BoundFunction
//ExistsFunction
//IsBlankFunction
//IsIriFunction
//IsLiteralFunction
//IsNumericFunction
//IsUriFunction
//LangMatchesFunction
//RegexFunction
//SameTermFunction
//CallFunction
//CoalesceFunction
//BNodeFunction
//IriFunction
//StrDtFunction
//StrLangFunction
//DayFunction
//HoursFunction
//MinutesFunction
//MonthFunction
//NowFunction
//SecondsFunction
//TimezoneFunction
//TZFunction
//YearFunction
//BaseHashFunction
//MD5HashFunction
//Sha1HashFunction
//Sha256HashFunction
//Sha384HashFunction
//Sha512HashFunction
//IfElseFunction
//AbsFunction
//CeilFunction
//FloorFunction
//RandFunction
//RoundFunction
//BaseSetFunction
//InFunction
//NotInFunction
//BaseBinaryStringFunction
//BaseUUIDFunction
//ConcatFunction
//ContainsFunction
//DataType11Function
//DataTypeFunction
//EncodeForUriFunction
//LangFunction
//LCaseFunction
//ReplaceFunction
//StrAfterFunction
//StrBeforeFunction
//StrEndsFunction
//StrFunction
//StrLenFunction
//StrStartsFunction
//StrUUIDFunction
//SubStrFunction
//UCaseFunction
//UUIDFunction
//UnknownFunction
//BooleanFunction
//BaseCast
//BooleanCast
//DateTimeCast
//DecimalCast
//DoubleCast
//FloatCast
//IntegerCast
//StringCast
//BaseUnaryDateTimeFunction
//DayFromDateTimeFunction
//HoursFromDateTimeFunction
//MinutesFromDateTimeFunction
//MonthFromDateTimeFunction
//SecondsFromDateTimeFunction
//TimezoneFromDateTimeFunction
//YearFromDateTimeFunction
//AbsFunction
//CeilingFunction
//FloorFunction
//RoundFunction
//RoundHalfToEvenFunction
//BaseBinaryStringFunction
//BaseUnaryStringFunction
//CompareFunction
//ConcatFunction
//ContainsFunction
//EncodeForUriFunction
//EndsWithFunction
//EscapeHtmlUriFunction
//LowerCaseFunction
//NormalizeSpaceFunction
//NormalizeUnicodeFunction
//ReplaceFunction
//StartsWithFunction
//StringLengthFunction
//SubstringAfterFunction
//SubstringBeforeFunction
//SubstringFunction
//UpperCaseFunction
//AggregateTerm
//AllModifier
//ConstantTerm
//DistinctModifier
//GraphPatternTerm
//VariableTerm
        }

        private ISparqlQuery ProcessITriplePatterns(IEnumerable<ITriplePattern> enumerable, QueryContext context)
        {
            var triples = enumerable.OfType<TriplePattern>();
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
