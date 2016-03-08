using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Test.Unit.Sparql.Optimization
{
    public abstract class BaseOptimizerTest
    {
        protected virtual SparqlQuery GenerateSparqlQuery()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            return parser.ParseFromString("SELECT * WHERE { }");
        }

        protected virtual QueryContext GenerateQueryContext()
        {
            var factory = new R2RMLDefaultStorageFactory();
            return factory.CreateQueryContext(GenerateSparqlQuery(), null, null, null, null);
        }

        protected void AssertPatternsEqual(IGraphPattern expected, IGraphPattern result)
        {
            Assert.IsTrue(ArePattersEqual(expected, result));
        }

        private bool ArePattersEqual(IGraphPattern left, IGraphPattern right)
        {
            SparqlCompareVisitor visitor = new SparqlCompareVisitor();
            return visitor.TransformGraphPattern(left, right);
        }
    }
}
