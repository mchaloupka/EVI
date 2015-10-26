using System;
using System.Security.Cryptography;
using Microsoft.SqlServer.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Patterns;
using Slp.r2rml4net.Storage.Sparql.Optimization.Optimizers;
using TCode.r2rml4net.Mapping;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Test.Unit.Sparql.Optimization.Optimizers
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
        [Ignore]
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

        private RestrictedTriplePattern CreateTemplatedRestrictedTriplePattern(string subjectVariable, string predicateVariable,
            string objectVariable, string subjectTemplate, string predicateTemplate, string objectTemplate)
        {
            var subjectMap = new Mock<ISubjectMap>();
            subjectMap.Setup(x => x.Template).Returns(subjectTemplate);
            subjectMap.Setup(x => x.IsTemplateValued).Returns(true);
            subjectMap.Setup(x => x.IsColumnValued).Returns(false);
            subjectMap.Setup(x => x.IsColumnValued).Returns(false);

            var predicateMap = new Mock<IPredicateMap>();
            predicateMap.Setup(x => x.Template).Returns(predicateTemplate);
            predicateMap.Setup(x => x.IsTemplateValued).Returns(true);
            predicateMap.Setup(x => x.IsColumnValued).Returns(false);
            predicateMap.Setup(x => x.IsColumnValued).Returns(false);

            var objectMap = new Mock<IObjectMap>();
            objectMap.Setup(x => x.Template).Returns(objectTemplate);
            objectMap.Setup(x => x.IsTemplateValued).Returns(true);
            objectMap.Setup(x => x.IsColumnValued).Returns(false);
            objectMap.Setup(x => x.IsColumnValued).Returns(false);

            return new RestrictedTriplePattern(new VariablePattern(subjectVariable), new VariablePattern(predicateVariable),
                new VariablePattern(objectVariable), null, subjectMap.Object, predicateMap.Object, objectMap.Object, null, null);
        }
    }
}
