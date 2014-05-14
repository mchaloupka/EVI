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
using VDS.RDF.Query.Paths;
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

        private ISparqlQuery ProcessAsk(QueryContext context)
        {
            throw new NotImplementedException();
        }

        private ISparqlQuery ProcessConstruct(QueryContext context)
        {
            return ProcessAlgebra(context.OriginalAlgebra, context);
        }

        private ISparqlQuery ProcessDescribe(QueryContext context)
        {
            throw new NotImplementedException();
        }

        private ISparqlQuery ProcessSelect(QueryContext context)
        {
            return ProcessAlgebra(context.OriginalAlgebra, context);
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
            else if(originalAlgebra is Reduced)
            {
                var orReduced = (Reduced)originalAlgebra;
                var inner = ProcessAlgebra(orReduced.InnerAlgebra, context);

                return new ReducedOp(inner);
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

        private ISparqlQuery ProcessITriplePatterns(IEnumerable<ITriplePattern> triplePatterns, QueryContext context)
        {
            ISparqlQuery current = new OneEmptySolutionOp();

            foreach (var part in triplePatterns)
            {
                if (part is TriplePattern)
                {
                    var triplePattern = (TriplePattern)part;
                    current = JoinWithCurrentTriplePattern(current, new BgpOp(triplePattern.Subject, triplePattern.Predicate, triplePattern.Object), context);
                }
                else if (part is PropertyPathPattern)
                {
                    var propertyPathPattern = (PropertyPathPattern)part;
                    var processed = ProcessPropertyPath(propertyPathPattern, context);
                    current = JoinWithCurrentTriplePattern(current, processed, context);
                }
                else if (part is BindPattern)
                {
                    var bindPattern = (BindPattern)part;
                    var varName = bindPattern.VariableName;
                    var expression = ProcessExpression(bindPattern.AssignExpression, context);
                    current = new BindOp(varName, expression, current);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return current;

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

        private ISparqlQuery JoinWithCurrentTriplePattern(ISparqlQuery current, ISparqlQuery query, QueryContext context)
        {
            if (current is OneEmptySolutionOp)
            {
                return query;
            }
            else
            {
                var joinOp = current as JoinOp;

                if (joinOp == null)
                {
                    joinOp = new JoinOp();
                    joinOp.AddToJoin(current);
                    current = joinOp;
                }

                joinOp.AddToJoin(query);
                return joinOp;
            }
        }

        private ISparqlQuery ProcessPropertyPath(PropertyPathPattern pathPattern, QueryContext context)
        {
            return ProcessPropertyPath(pathPattern.Subject, pathPattern.Path, pathPattern.Object, context);
        }

        private ISparqlQuery ProcessPropertyPath(PatternItem subject, ISparqlPath path, PatternItem obj, QueryContext context)
        {
            if(path is SequencePath)
            {
                var sPath = (SequencePath)path;
                var helpVar = context.CreateSparqlVariable();
                var helpPattern = new VariablePattern(helpVar);

                var join = new JoinOp();
                var lRes = ProcessPropertyPath(subject, sPath.LhsPath, helpPattern, context);
                var rRes = ProcessPropertyPath(helpPattern, sPath.RhsPath, obj, context);

                join.AddToJoin(lRes);
                join.AddToJoin(rRes);
                return join;
            }
            else if (path is Property)
            {
                var pPath = (Property)path;
                var predicatePattern = new NodeMatchPattern(pPath.Predicate);
                return new BgpOp(subject, predicatePattern, obj);
            }
            else
            {
                throw new NotImplementedException();
            }


// http://www.dotnetrdf.org/api/dotNetRDF~VDS.RDF.Query.Paths.ISparqlPath.html
//AlternativePath	 Represents Alternative Paths
//BaseBinaryPath	 Abstract Base Class for Binary Path operators
//BaseUnaryPath	 Abstract Base Class for Unary Path operators
//Cardinality	 Represents a Cardinality restriction on a Path
//FixedCardinality	 Represents a Fixed Cardinality restriction on a Path
//InversePath	 Represents an Inverse Path
//NegatedSet	 Represents a Negated Property Set
//NOrMore	 Represents a N or More cardinality restriction on a Path
//NToM	 Represents a N to M cardinality restriction on a Path
//OneOrMore	 Represents a One or More cardinality restriction on a Path
//Property	 Represents a Predicate which is part of a Path
//SequencePath	 Represents a standard forwards path
//ZeroOrMore	 Represents a Zero or More cardinality restriction on a Path
//ZeroOrOne	 Represents a Zero or One cardinality restriction on a Path
//ZeroToN	 Represents a Zero to N cardinality restriction on a Path
        }
    }
}
