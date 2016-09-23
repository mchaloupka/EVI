using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Slp.Evi.Storage.Sparql.PostProcess.Optimizers;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.Evi.Test.Unit.Sparql.Optimization.Optimizers
{
    [TestClass]
    public class TriplePatternOptimizerTest
        : BaseOptimizerTest
    {
        private TriplePatternOptimizer.TriplePatternOptimizerImplementation _optimizerImplementation;

        [TestInitialize]
        public void TestInitialization()
        {
            _optimizerImplementation = new TriplePatternOptimizer.TriplePatternOptimizerImplementation();
        }

        [TestMethod]
        public void MatchConstantMap_Uri_Match()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IUriValuedTermMap>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.IsTrue(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        [TestMethod]
        public void MatchConstantMap_Uri_NoMatch_DifferentSuffix()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com/Product"));

            var map = new Mock<IUriValuedTermMap>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com/ProductType"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.IsFalse(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        private static Mock<ITermType> GetUriType()
        {
            var uriType = new Mock<ITermType>();
            uriType.Setup(x => x.IsURI).Returns(true);
            uriType.Setup(x => x.IsLiteral).Returns(false);
            return uriType;
        }

        private static Mock<ITermType> GetLiteralType()
        {
            var uriType = new Mock<ITermType>();
            uriType.Setup(x => x.IsURI).Returns(false);
            uriType.Setup(x => x.IsLiteral).Returns(true);
            return uriType;
        }

        [TestMethod]
        public void MatchConstantMap_Uri_NoMatch_DifferentUri()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IUriValuedTermMap>();
            map.Setup(x => x.URI).Returns(new Uri("http://test2.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.IsFalse(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        [TestMethod]
        public void MatchConstantMap_Uri_NoMatch_NotUri()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IObjectMap>();
            map.Setup(x => x.URI).Returns((Uri)null);
            map.Setup(x => x.Literal).Returns("http://test2.com");
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetLiteralType().Object);

            Assert.IsFalse(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        [TestMethod]
        public void MatchConstantMap_Uri_Match_Object()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IObjectMap>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.IsTrue(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        [TestMethod]
        public void MatchConstantMap_Uri_NoMatch_ObjectDifferentUri()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IObjectMap>();
            map.Setup(x => x.URI).Returns(new Uri("http://test2.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.IsFalse(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        [TestMethod]
        public void MatchConstantMap_Literal_Match()
        {
            var node = new Mock<ILiteralNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Literal);
            node.Setup(x => x.Value).Returns("http://test.com");

            var map = new Mock<IObjectMap>();
            map.Setup(x => x.URI).Returns((Uri)null);
            map.Setup(x => x.Literal).Returns("http://test.com");
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetLiteralType().Object);

            Assert.IsTrue(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        [TestMethod]
        public void MatchConstantMap_Literal_NoMatch_NotLiteral()
        {
            var node = new Mock<ILiteralNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Literal);
            node.Setup(x => x.Value).Returns("http://test.com");

            var map = new Mock<IObjectMap>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.IsFalse(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        [TestMethod]
        public void MatchConstantMap_Literal_NoMatch_UriMap()
        {
            var node = new Mock<ILiteralNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Literal);
            node.Setup(x => x.Value).Returns("http://test.com");

            var map = new Mock<IUriValuedTermMap>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.IsFalse(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        [TestMethod]
        public void MatchConstantMap_Literal_NoMatch_Different()
        {
            var node = new Mock<ILiteralNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Literal);
            node.Setup(x => x.Value).Returns("http://test.com");

            var map = new Mock<IObjectMap>();
            map.Setup(x => x.URI).Returns((Uri)null);
            map.Setup(x => x.Literal).Returns("http://test2.com");
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetLiteralType().Object);

            Assert.IsFalse(_optimizerImplementation.CanMatch(node.Object, map.Object));
        }

        [TestMethod]
        [Ignore]
        public void MatchPatternMap()
        {
            Assert.IsTrue(false);
        }
    }
}
