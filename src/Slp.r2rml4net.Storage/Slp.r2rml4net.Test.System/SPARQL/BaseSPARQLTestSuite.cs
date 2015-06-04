using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace Slp.r2rml4net.Test.System.SPARQL
{
    public abstract class BaseSPARQLTestSuite
    {
        protected void AssertBagEqual(XDocument expected, object result)
        {
            var expectedSet = ParseSparqlResultSetXmlResultFile(expected);

            Assert.IsTrue(result is SparqlResultSet);

            var resultSet = (SparqlResultSet)result;

            CollectionAssert.AreEqual(expectedSet.Variables.ToArray(), resultSet.Variables.ToArray());

            CollectionAssert.AreEquivalent(expectedSet.Results, resultSet.Results);
        }

        private SparqlResultSet ParseSparqlResultSetXmlResultFile(XDocument expected)
        {
            XNamespace ns = "http://www.w3.org/2005/sparql-results#";

            var variableNodes = expected
                .Descendants()
                .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "head")
                .Descendants()
                .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "variable")
                .Select(x => x.Attribute("name"))
                .Where(x => x != null)
                .Select(x => x.Value);

            var resultSet = new SparqlResultSet();
            var handler = new ResultSetHandler(resultSet);

            handler.StartResults();

            foreach (var variable in variableNodes)
            {
                handler.HandleVariable(variable);
            }

            var resultNodes = expected
                .Descendants()
                .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "results")
                .Descendants()
                .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "result");

            foreach (var resultNode in resultNodes)
            {
                var bindings = resultNode
                    .Descendants()
                    .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "binding")
                    .Where(x => x.Attribute("name") != null);


                var set = new Set();

                foreach (var binding in bindings)
                {
                    var varNane = binding.Attribute("name").Value;
                    var value = GetSparqlValue(binding, handler);

                    set.Add(varNane, value);
                }

                handler.HandleResult(new SparqlResult(set));
            }

            handler.EndResults(true);

            return resultSet;
        }

        private INode GetSparqlValue(XElement binding, ISparqlResultsHandler resultSetHandler)
        {
            if (binding.Nodes().OfType<XElement>().Count() == 1)
            {
                var element = binding.Nodes().OfType<XElement>().First();

                if (element.Name.LocalName == "uri")
                {
                    string uri = element.Value;
                    return resultSetHandler.CreateUriNode(new Uri(uri));
                }
            }

            throw new NotImplementedException();
        }
    }
}
