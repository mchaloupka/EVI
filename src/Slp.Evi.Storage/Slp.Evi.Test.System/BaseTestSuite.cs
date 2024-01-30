using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using Xunit;

namespace Slp.Evi.Test.System
{
    public abstract class BaseTestSuite
    {
        protected void AssertBagEqual(XDocument expected, object result)
        {
            var expectedSet = ParseSparqlResultSetXmlResultFile(expected);

            Assert.IsAssignableFrom<SparqlResultSet>(result);
            var resultSet = (SparqlResultSet)result;

            Assert.Equal(expectedSet.Variables, resultSet.Variables);

            CollectionsAreEquivalent(expectedSet.Results, resultSet.Results);
        }

        protected void AssertEqual(XDocument expected, object result)
        {
            var expectedSet = ParseSparqlResultSetXmlResultFile(expected);

            Assert.IsAssignableFrom<SparqlResultSet>(result);
            var resultSet = (SparqlResultSet)result;

            Assert.Equal(expectedSet.Variables, resultSet.Variables);

            Assert.Equal(expectedSet.Results, resultSet.Results);
        }

        private void CollectionsAreEquivalent<T>(IList<T> expectedCollection, IList<T> actualCollection)
        {
            Assert.Equal(expectedCollection.Count, actualCollection.Count);

            var usedIndexes = new HashSet<int>(actualCollection.Count);

            for (var eIndex = 0; eIndex < expectedCollection.Count; eIndex++)
            {
                var expectedItem = expectedCollection[eIndex];
                bool found = false;

                for (int aIndex = 0; aIndex < actualCollection.Count; aIndex++)
                {
                    if (usedIndexes.Contains(aIndex))
                        continue;

                    var actualItem = actualCollection[aIndex];
                    if (Equals(expectedItem, actualItem))
                    {
                        usedIndexes.Add(aIndex);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Assert.Fail($"Expected item #{eIndex} cannot be found in the actual collection.");
                }
            }
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

                    if (value != null)
                    {
                        set.Add(varNane, value);
                    }
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
                else if (element.Name.LocalName == "literal")
                {
                    string value = element.Value;

                    if (element.Attribute("type") != null)
                    {
                        var type = element.Attribute("type").Value;
                        if (type.StartsWith("xsd:"))
                        {
                            type = "http://www.w3.org/2001/XMLSchema#" + type.Substring(4);
                        }

                        var uriType = new Uri(type);
                        return resultSetHandler.CreateLiteralNode(value, uriType);
                    }
                    else if (element.Attribute("lang") != null)
                    {
                        var lang = element.Attribute("lang").Value;
                        return resultSetHandler.CreateLiteralNode(value, lang);
                    }
                    else
                    {
                        return resultSetHandler.CreateLiteralNode(value);
                    }
                }
                else if (element.Name.LocalName == "unbound")
                {
                    return null;
                }
            }

            throw new NotImplementedException();
        }
    }
}
