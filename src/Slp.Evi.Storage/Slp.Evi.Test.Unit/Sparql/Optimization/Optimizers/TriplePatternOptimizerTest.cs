using System;
using Moq;
using Slp.Evi.Storage.Mapping.Representation;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.PostProcess.Optimizers;
using Slp.Evi.Storage.Types;
using Slp.Evi.Storage.Utils;
using VDS.RDF;
using Xunit;

namespace Slp.Evi.Test.Unit.Sparql.Optimization.Optimizers
{
    public class TriplePatternOptimizerTest
        : BaseOptimizerTest
    {
        private readonly TriplePatternOptimizer.TriplePatternOptimizerImplementation _optimizerImplementation = new TriplePatternOptimizer.TriplePatternOptimizerImplementation();
        private readonly Mock<IQueryContext> _queryContext = new Mock<IQueryContext>();

        [Fact]
        public void MatchConstantMap_Uri_Match()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IIriValuedTermMapping>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.True(_optimizerImplementation.CanMatch(node.Object, map.Object, new IRIValueType(), _queryContext.Object));
        }

        [Fact]
        public void MatchConstantMap_Uri_NoMatch_DifferentSuffix()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com/Product"));

            var map = new Mock<IIriValuedTermMapping>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com/ProductType"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.False(_optimizerImplementation.CanMatch(node.Object, map.Object, new IRIValueType(), _queryContext.Object));
        }

        private static Mock<ITermTypeInformation> GetUriType()
        {
            var uriType = new Mock<ITermTypeInformation>();
            uriType.Setup(x => x.IsURI).Returns(true);
            uriType.Setup(x => x.IsLiteral).Returns(false);
            return uriType;
        }

        private static Mock<ITermTypeInformation> GetLiteralType()
        {
            var uriType = new Mock<ITermTypeInformation>();
            uriType.Setup(x => x.IsURI).Returns(false);
            uriType.Setup(x => x.IsLiteral).Returns(true);
            return uriType;
        }

        [Fact]
        public void MatchConstantMap_Uri_NoMatch_DifferentUri()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IIriValuedTermMapping>();
            map.Setup(x => x.URI).Returns(new Uri("http://test2.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.False(_optimizerImplementation.CanMatch(node.Object, map.Object, new IRIValueType(), _queryContext.Object));
        }

        [Fact]
        public void MatchConstantMap_Uri_NoMatch_NotUri()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IObjectMapping>();
            map.Setup(x => x.URI).Returns((Uri)null);
            map.Setup(x => x.Literal).Returns(new ParsedLiteralParts("http://test2.com", null, string.Empty));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetLiteralType().Object);

            Assert.False(_optimizerImplementation.CanMatch(node.Object, map.Object, new IRIValueType(), _queryContext.Object));
        }

        [Fact]
        public void MatchConstantMap_Uri_Match_Object()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IObjectMapping>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.True(_optimizerImplementation.CanMatch(node.Object, map.Object, new IRIValueType(), _queryContext.Object));
        }

        [Fact]
        public void MatchConstantMap_Uri_NoMatch_ObjectDifferentUri()
        {
            var node = new Mock<IUriNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Uri);
            node.Setup(x => x.Uri).Returns(new Uri("http://test.com"));

            var map = new Mock<IObjectMapping>();
            map.Setup(x => x.URI).Returns(new Uri("http://test2.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.False(_optimizerImplementation.CanMatch(node.Object, map.Object, new IRIValueType(), _queryContext.Object));
        }

        [Fact]
        public void MatchConstantMap_Literal_Match()
        {
            var node = new Mock<ILiteralNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Literal);
            node.Setup(x => x.Value).Returns("http://test.com");

            var map = new Mock<IObjectMapping>();
            map.Setup(x => x.URI).Returns((Uri)null);
            map.Setup(x => x.Literal).Returns(new ParsedLiteralParts("http://test.com", null, string.Empty));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetLiteralType().Object);

            Assert.True(_optimizerImplementation.CanMatch(node.Object, map.Object, new LiteralValueType(null, null), _queryContext.Object));
        }

        [Fact]
        public void MatchConstantMap_Literal_NoMatch_NotLiteral()
        {
            var node = new Mock<ILiteralNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Literal);
            node.Setup(x => x.Value).Returns("http://test.com");

            var map = new Mock<IObjectMapping>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.False(_optimizerImplementation.CanMatch(node.Object, map.Object, new LiteralValueType(null, null), _queryContext.Object));
        }

        [Fact]
        public void MatchConstantMap_Literal_NoMatch_UriMap()
        {
            var node = new Mock<ILiteralNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Literal);
            node.Setup(x => x.Value).Returns("http://test.com");

            var map = new Mock<IIriValuedTermMapping>();
            map.Setup(x => x.URI).Returns(new Uri("http://test.com"));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetUriType().Object);

            Assert.False(_optimizerImplementation.CanMatch(node.Object, map.Object, new LiteralValueType(null, null), _queryContext.Object));
        }

        [Fact]
        public void MatchConstantMap_Literal_NoMatch_Different()
        {
            var node = new Mock<ILiteralNode>();
            node.Setup(x => x.NodeType).Returns(NodeType.Literal);
            node.Setup(x => x.Value).Returns("http://test.com");

            var map = new Mock<IObjectMapping>();
            map.Setup(x => x.URI).Returns((Uri)null);
            map.Setup(x => x.Literal).Returns(new ParsedLiteralParts("http://test2.com", null, string.Empty));
            map.Setup(x => x.IsConstantValued).Returns(true);
            map.Setup(x => x.TermType).Returns(GetLiteralType().Object);

            Assert.False(_optimizerImplementation.CanMatch(node.Object, map.Object, new LiteralValueType(null, null), _queryContext.Object));
        }

        [Fact(Skip = "The test is not yet implemented")]
        public void MatchPatternMap()
        {
            Assert.True(false);
        }
    }
}
