using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Optimization;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Query
{
    public class QueryProcessor
    {
        private MappingProcessor mapping;
        private ISqlDb db;
        private SparqlAlgebraBuilder sparqlAlgebraBuilder;
        private List<ISparqlAlgebraOptimizer> sparqlOptimizers;
        private SqlAlgebraBuilder sqlAlgebraBuilder;
        private List<ISqlAlgebraOptimizer> sqlOptimizers;

        public QueryProcessor(MappingProcessor mapping, ISqlDb db)
        {
            this.mapping = mapping;
            this.db = db;
            this.sparqlAlgebraBuilder = new SparqlAlgebraBuilder();
            this.sqlAlgebraBuilder = new SqlAlgebraBuilder();

            this.sparqlOptimizers = new List<ISparqlAlgebraOptimizer>()
            {
                new Optimization.SparqlAlgebra.R2RMLOptimizer(),
                new Optimization.SparqlAlgebra.UnionOptimizer()
            };

            this.sqlOptimizers = new List<ISqlAlgebraOptimizer>() 
            {
                new Optimization.SqlAlgebra.IsNullOptimizer(),
                new Optimization.SqlAlgebra.ConcatenationInEqualConditionOptimizer(),
                new Optimization.SqlAlgebra.ConstantExprEqualityOptimizer(),
                new Optimization.SqlAlgebra.RemoveUnusedColumnsOptimization()
            };
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            SparqlQueryParser parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
            var originalQuery = parser.ParseFromString(sparqlQuery);

            INodeFactory nodeFactory = null;
            switch (originalQuery.QueryType)
            {
                case SparqlQueryType.Ask:
                    if (resultsHandler == null) 
                        throw new ArgumentNullException("resultsHandler", "Cannot handle a Ask query with a null SPARQL Results Handler");

                    nodeFactory = resultsHandler;
                    break;
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    if (rdfHandler == null) 
                        throw new ArgumentNullException("rdfHandler", "Cannot handle a Graph result with a null RDF Handler");

                    nodeFactory = rdfHandler;
                    break;
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    if (resultsHandler == null) 
                        throw new ArgumentNullException("resultsHandler", "Cannot handle SPARQL Results with a null Results Handler");

                    nodeFactory = resultsHandler;
                    break;
                default:
                    throw new Exception("Unable to process the results of an Unknown query type");
            }

            // Convert to algebra
            var context = new QueryContext(originalQuery, mapping, nodeFactory);

            // Generate SQL algebra
            var sqlAlgebra = GenerateSqlAlgebra(context);

            // Query
            var query = db.GenerateQuery(sqlAlgebra, context);

            // Execute query
            using (var result = db.ExecuteQuery(query, context))
            {
                switch (originalQuery.QueryType)
                {
                    case VDS.RDF.Query.SparqlQueryType.Ask:
                        resultsHandler.StartResults();
                        try
                        {
                            if (result.HasNextRow)
                            {
                                var row = result.Read();

                                if (!row.Columns.Any())
                                {
                                    throw new Exception("Expected a column from sql query execution");
                                }
                                else if (row.Columns.Count() > 1)
                                {
                                    throw new Exception("Expected a single column from sql query execution");
                                }

                                var boolValue = row.Columns.First().GetBooleanValue();

                                resultsHandler.HandleBooleanResult(boolValue);
                            }
                            else
                            {
                                throw new Exception("Expected a row from sql query execution");
                            }

                            resultsHandler.EndResults(true);
                        }
                        catch
                        {
                            resultsHandler.EndResults(false);
                            throw;
                        }
                        

                        break;
                    case VDS.RDF.Query.SparqlQueryType.Construct:
                    case VDS.RDF.Query.SparqlQueryType.Describe:
                    case VDS.RDF.Query.SparqlQueryType.DescribeAll:
                        if (sqlAlgebra.ValueBinders.Count() != 3)
                            throw new Exception("Expected 3 value binders in construct or describe query");

                        var sBinder = sqlAlgebra.ValueBinders.ElementAt(0);
                        var pBinder = sqlAlgebra.ValueBinders.ElementAt(1);
                        var oBinder = sqlAlgebra.ValueBinders.ElementAt(2);

                        rdfHandler.StartRdf();
                        try
                        {
                            while (result.HasNextRow)
                            {
                                var row = result.Read();

                                var sNode = sBinder.LoadNode(rdfHandler, row, context);
                                var pNode = pBinder.LoadNode(rdfHandler, row, context);
                                var oNode = oBinder.LoadNode(rdfHandler, row, context);

                                if (sNode == null || pNode == null || oNode == null)
                                    continue;

                                if (!rdfHandler.HandleTriple(new Triple(sNode, pNode, oNode))) break;
                            }
                        }
                        catch
                        {
                            rdfHandler.EndRdf(false);
                            throw;
                        }

                        break;
                    case VDS.RDF.Query.SparqlQueryType.Select:
                    case VDS.RDF.Query.SparqlQueryType.SelectAll:
                    case VDS.RDF.Query.SparqlQueryType.SelectAllDistinct:
                    case VDS.RDF.Query.SparqlQueryType.SelectAllReduced:
                    case VDS.RDF.Query.SparqlQueryType.SelectDistinct:
                    case VDS.RDF.Query.SparqlQueryType.SelectReduced:
                        resultsHandler.StartResults();

                        try
                        {
                            foreach (var binder in sqlAlgebra.ValueBinders)
                            {
                                if (!resultsHandler.HandleVariable(binder.VariableName)) ParserHelper.Stop();
                            }

                            Graph temp = new Graph();

                            //var s = new VDS.RDF.Query.Algebra.Set();
                            while (result.HasNextRow)
                            {
                                var row = result.Read();

                                var s = new VDS.RDF.Query.Algebra.Set();

                                foreach (var binder in sqlAlgebra.ValueBinders)
                                {
                                    var val = binder.LoadNode(temp, row, context);

                                    if (val != null)
                                        s.Add(binder.VariableName, val);
                                }

                                if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                            }

                            resultsHandler.EndResults(true);
                        }
                        catch
                        {
                            resultsHandler.EndResults(false);
                            throw;
                        }

                        break;
                    case VDS.RDF.Query.SparqlQueryType.Unknown:
                        throw new Exception("Unable to process the results of an Unknown query type");
                    default:
                        break;
                }
            }
        }

        private INotSqlOriginalDbSource GenerateSqlAlgebra(QueryContext context)
        {
            var algebra = sparqlAlgebraBuilder.Process(context);

            // Transform graph and from statements

            // Transform using R2RML
            algebra = mapping.ProcessAlgebra(algebra, context);

            // TODO: Make algebra valid, take filters up as possible

            // Optimize sparql algebra
            foreach (var optimizer in sparqlOptimizers)
            {
                algebra = optimizer.ProcessAlgebra(algebra, context);
            }

            // Transform to SQL algebra
            var sqlAlgebra = sqlAlgebraBuilder.Process(algebra, context);

            // Optimize sql algebra
            foreach (var optimizer in sqlOptimizers)
            {
                sqlAlgebra = optimizer.ProcessAlgebra(sqlAlgebra, context);
            }

            return sqlAlgebra;
        }
    }
}
