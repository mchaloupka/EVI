using System;
using Moq;
using Slp.Evi.Storage.Mapping.Representation;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.PostProcess.Optimizers;
using Slp.Evi.Storage.Types;
using TCode.r2rml4net.Mapping;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace Slp.Evi.Test.Unit.Sparql.Optimization.Optimizers
{
    public class UnionJoinOptimizerTest
        : BaseOptimizerTest
    {
        private readonly UnionJoinOptimizer.UnionJoinOptimizerImplementation _optimizerImplementation = new UnionJoinOptimizer.UnionJoinOptimizerImplementation();

        [Fact]
        public void JoinOfUnionToUnionOfJoins()
        {
            var p1 = CreateTemplatedRestrictedTriplePattern("v1", "p1", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p2 = CreateTemplatedRestrictedTriplePattern("v2", "p2", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p3 = CreateTemplatedRestrictedTriplePattern("v3", "p3", "v4", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");

            var up1 = new UnionPattern(new IGraphPattern[] { p1, p2 });

            var join = new JoinPattern(new IGraphPattern[] { up1, p3 });

            var result = _optimizerImplementation.TransformGraphPattern(join, new BaseSparqlOptimizer<object>.OptimizationContext()
            {
                Context = GetQueryContext().Object
            });

            var expected = new UnionPattern(new IGraphPattern[]
            {
                new JoinPattern(new IGraphPattern[] {p1, p3}),
                new JoinPattern(new IGraphPattern[] {p2, p3}),
            });

            AssertPatternsEqual(expected, result);
        }

        private static Mock<IQueryContext> GetQueryContext()
        {
            var queryContext = new Mock<IQueryContext>();
            var typeCache = new Mock<ITypeCache>();
            queryContext.SetupGet(x => x.TypeCache).Returns(typeCache.Object);
            return queryContext;
        }

        [Fact]
        public void JoinOfUnionToJoin()
        {
            var p1 = CreateTemplatedRestrictedTriplePattern("v1", "p1", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p2 = CreateTemplatedRestrictedTriplePattern("v2", "p2", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test2.com/{id}");
            var p3 = CreateTemplatedRestrictedTriplePattern("v3", "p3", "v4", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");

            var up1 = new UnionPattern(new IGraphPattern[] { p1, p2 });

            var join = new JoinPattern(new IGraphPattern[] { up1, p3 });

            var result = _optimizerImplementation.TransformGraphPattern(join, new BaseSparqlOptimizer<object>.OptimizationContext()
            {
                Context = GetQueryContext().Object
            });

            var expected = new JoinPattern(new IGraphPattern[] {p1, p3});

            AssertPatternsEqual(expected, result);
        }

        [Fact]
        public void JoinOfUnionToUnionOfJoin()
        {
            var p1 = CreateTemplatedRestrictedTriplePattern("v1", "p1", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p2 = CreateTemplatedRestrictedTriplePattern("v2", "p2", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test2.com/{id}");
            var p3 = CreateTemplatedRestrictedTriplePattern("v3", "p3", "v4", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p4 = CreateTemplatedRestrictedTriplePattern("v3", "p3", "v4", "http://test2.com/{id}", "http://test.com/{id}", "http://test.com/{id}");

            var up1 = new UnionPattern(new IGraphPattern[] { p1, p2 });
            var up2 = new UnionPattern(new IGraphPattern[] { p3, p4 });

            var join = new JoinPattern(new IGraphPattern[] { up1, up2 });

            var result = _optimizerImplementation.TransformGraphPattern(join, new BaseSparqlOptimizer<object>.OptimizationContext()
            {
                Context = GetQueryContext().Object
            });

            var expected = new UnionPattern(new IGraphPattern[]
            {
                new JoinPattern(new IGraphPattern[] {p1, p3}),
                new JoinPattern(new IGraphPattern[] {p2, p4}),
            });

            AssertPatternsEqual(expected, result);
        }

        [Fact(Skip="The test is not yet implemented")]
        public void TestMatchVariants()
        {
            throw new NotImplementedException();
        }

        private RestrictedTriplePattern CreateTemplatedRestrictedTriplePattern(string subjectVariable, string predicateVariable,
            string objectVariable, string subjectTemplate, string predicateTemplate, string objectTemplate)
        {
            var subjectMap = new Mock<ISubjectMapping>();
            subjectMap.Setup(x => x.Template).Returns(subjectTemplate);
            subjectMap.Setup(x => x.IsTemplateValued).Returns(true);
            subjectMap.Setup(x => x.IsColumnValued).Returns(false);
            subjectMap.Setup(x => x.IsColumnValued).Returns(false);
            subjectMap.Setup(x => x.TermType).Returns(CreateURITermType());

            var predicateMap = new Mock<IPredicateMapping>();
            predicateMap.Setup(x => x.Template).Returns(predicateTemplate);
            predicateMap.Setup(x => x.IsTemplateValued).Returns(true);
            predicateMap.Setup(x => x.IsColumnValued).Returns(false);
            predicateMap.Setup(x => x.IsColumnValued).Returns(false);
            predicateMap.Setup(x => x.TermType).Returns(CreateURITermType());

            var objectMap = new Mock<IObjectMapping>();
            objectMap.Setup(x => x.Template).Returns(objectTemplate);
            objectMap.Setup(x => x.IsTemplateValued).Returns(true);
            objectMap.Setup(x => x.IsColumnValued).Returns(false);
            objectMap.Setup(x => x.IsColumnValued).Returns(false);
            objectMap.Setup(x => x.TermType).Returns(CreateURITermType());

            return new RestrictedTriplePattern(new VariablePattern(subjectVariable), new VariablePattern(predicateVariable),
                new VariablePattern(objectVariable), null, subjectMap.Object, predicateMap.Object, objectMap.Object, null, null);
        }

        private ITermTypeInformation CreateURITermType()
        {
            var termType = new Mock<ITermTypeInformation>();
            termType.Setup(x => x.IsBlankNode).Returns(false);
            termType.Setup(x => x.IsLiteral).Returns(false);
            termType.Setup(x => x.IsIri).Returns(true);
            return termType.Object;
        }
    }
}
