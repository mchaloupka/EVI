using Slp.Evi.Storage.Sparql.Algebra;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;

namespace Slp.Evi.Test.Unit.Sparql.Optimization
{
    public abstract class BaseOptimizerTest
    {
        protected virtual SparqlQuery GenerateSparqlQuery()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            return parser.ParseFromString("SELECT * WHERE { }");
        }

        protected void AssertPatternsEqual(IGraphPattern expected, IGraphPattern result)
        {
            Assert.True(ArePattersEqual(expected, result));
        }

        private bool ArePattersEqual(IGraphPattern left, IGraphPattern right)
        {
            SparqlCompareVisitor visitor = new SparqlCompareVisitor();
            return visitor.TransformGraphPattern(left, right);
        }
    }
}
