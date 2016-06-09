using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.PostProcess.Optimizers;
using TCode.r2rml4net.Mapping;
using VDS.RDF.Query.Patterns;

namespace Slp.Evi.Test.Unit.Sparql.Optimization.Optimizers
{
    [TestClass]
    public class UnionJoinOptimizerTest
        : BaseOptimizerTest
    {
        private UnionJoinOptimizer.UnionJoinOptimizerImplementation _optimizerImplementation;

        [TestInitialize]
        public void TestInitialization()
        {
            _optimizerImplementation = new UnionJoinOptimizer.UnionJoinOptimizerImplementation();
        }

        [TestMethod]
        public void JoinOfUnionToUnionOfJoins()
        {
            var p1 = CreateTemplatedRestrictedTriplePattern("v1", "p1", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p2 = CreateTemplatedRestrictedTriplePattern("v2", "p2", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p3 = CreateTemplatedRestrictedTriplePattern("v3", "p3", "v4", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            
            var up1 = new UnionPattern(new IGraphPattern[] {p1, p2});

            var join = new JoinPattern(new IGraphPattern[] {up1, p3});

            var result = _optimizerImplementation.TransformGraphPattern(join, new BaseSparqlOptimizer<object>.OptimizationContext());

            var expected = new UnionPattern(new IGraphPattern[]
            {
                new JoinPattern(new IGraphPattern[] {p1, p3}),
                new JoinPattern(new IGraphPattern[] {p2, p3}),  
            });

            AssertPatternsEqual(expected, result);
        }

        [TestMethod]
        public void JoinOfUnionToJoin()
        {
            var p1 = CreateTemplatedRestrictedTriplePattern("v1", "p1", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p2 = CreateTemplatedRestrictedTriplePattern("v2", "p2", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test2.com/{id}");
            var p3 = CreateTemplatedRestrictedTriplePattern("v3", "p3", "v4", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");

            var up1 = new UnionPattern(new IGraphPattern[] { p1, p2 });

            var join = new JoinPattern(new IGraphPattern[] { up1, p3 });

            var result = _optimizerImplementation.TransformGraphPattern(join, new BaseSparqlOptimizer<object>.OptimizationContext());

            var expected = new JoinPattern(new IGraphPattern[] {p1, p3});

            AssertPatternsEqual(expected, result);
        }

        [TestMethod]
        public void JoinOfUnionToUnionOfJoin()
        {
            var p1 = CreateTemplatedRestrictedTriplePattern("v1", "p1", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p2 = CreateTemplatedRestrictedTriplePattern("v2", "p2", "v3", "http://test.com/{id}", "http://test.com/{id}", "http://test2.com/{id}");
            var p3 = CreateTemplatedRestrictedTriplePattern("v3", "p3", "v4", "http://test.com/{id}", "http://test.com/{id}", "http://test.com/{id}");
            var p4 = CreateTemplatedRestrictedTriplePattern("v3", "p3", "v4", "http://test2.com/{id}", "http://test.com/{id}", "http://test.com/{id}");

            var up1 = new UnionPattern(new IGraphPattern[] { p1, p2 });
            var up2 = new UnionPattern(new IGraphPattern[] { p3, p4 });

            var join = new JoinPattern(new IGraphPattern[] { up1, up2 });

            var result = _optimizerImplementation.TransformGraphPattern(join, new BaseSparqlOptimizer<object>.OptimizationContext());

            var expected = new UnionPattern(new IGraphPattern[]
            {
                new JoinPattern(new IGraphPattern[] {p1, p3}),
                new JoinPattern(new IGraphPattern[] {p2, p4}),
            });

            AssertPatternsEqual(expected, result);
        }

        [TestMethod]
        [Ignore]
        public void TestMatchVariants()
        {
            throw new NotImplementedException();
        }

        private RestrictedTriplePattern CreateTemplatedRestrictedTriplePattern(string subjectVariable, string predicateVariable,
            string objectVariable, string subjectTemplate, string predicateTemplate, string objectTemplate)
        {
            var subjectMap = new Mock<ISubjectMap>();
            subjectMap.Setup(x => x.Template).Returns(subjectTemplate);
            subjectMap.Setup(x => x.IsTemplateValued).Returns(true);
            subjectMap.Setup(x => x.IsColumnValued).Returns(false);
            subjectMap.Setup(x => x.IsColumnValued).Returns(false);
            subjectMap.Setup(x => x.TermType).Returns(CreateURITermType());

            var predicateMap = new Mock<IPredicateMap>();
            predicateMap.Setup(x => x.Template).Returns(predicateTemplate);
            predicateMap.Setup(x => x.IsTemplateValued).Returns(true);
            predicateMap.Setup(x => x.IsColumnValued).Returns(false);
            predicateMap.Setup(x => x.IsColumnValued).Returns(false);
            predicateMap.Setup(x => x.TermType).Returns(CreateURITermType());

            var objectMap = new Mock<IObjectMap>();
            objectMap.Setup(x => x.Template).Returns(objectTemplate);
            objectMap.Setup(x => x.IsTemplateValued).Returns(true);
            objectMap.Setup(x => x.IsColumnValued).Returns(false);
            objectMap.Setup(x => x.IsColumnValued).Returns(false);
            objectMap.Setup(x => x.TermType).Returns(CreateURITermType());

            return new RestrictedTriplePattern(new VariablePattern(subjectVariable), new VariablePattern(predicateVariable),
                new VariablePattern(objectVariable), null, subjectMap.Object, predicateMap.Object, objectMap.Object, null, null);
        }

        private ITermType CreateURITermType()
        {
            var termType = new Mock<ITermType>();
            termType.Setup(x => x.IsBlankNode).Returns(false);
            termType.Setup(x => x.IsLiteral).Returns(false);
            termType.Setup(x => x.IsURI).Returns(true);
            return termType.Object;
        }
    }
}
