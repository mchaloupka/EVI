using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Optimization;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using VDS.RDF;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Storage.Query
{
    public class QueryContext
    {
        private List<string> usedSqlSourceNames;
        private Dictionary<string, INode> blankNodesSubjects;
        private Dictionary<string, INode> blankNodesObjects;
        private ISqlAlgebraOptimizerOnTheFly[] sqlOptimizers;

        public QueryContext(SparqlQuery query, MappingProcessor mapping, ISqlDb db, INodeFactory nodeFactory, ISqlAlgebraOptimizerOnTheFly[] sqlOptimizers)
        {
            this.OriginalQuery = query;
            this.NodeFactory = nodeFactory;
            this.Db = db;
            this.Mapping = mapping;
            this.usedSqlSourceNames = new List<string>();
            this.blankNodesSubjects = new Dictionary<string, INode>();
            this.blankNodesObjects = new Dictionary<string, INode>();
            this.sqlOptimizers = sqlOptimizers;
        }

        public SparqlQuery OriginalQuery { get; private set; }

        public MappingProcessor Mapping { get; private set; }

        public INodeFactory NodeFactory { get; private set; }

        public ISqlDb Db { get; private set; }

        public bool IsAlreadyUsedSqlSourceName(string name)
        {
            return usedSqlSourceNames.Contains(name);
        }

        public void RegisterUsedSqlSourceName(string name)
        {
            if (usedSqlSourceNames.Contains(name))
            {
                throw new ArgumentException("This sql source name already used", "name");
            }
            else
            {
                usedSqlSourceNames.Add(name);
            }
        }

        public INode GetBlankNodeSubjectForValue(INodeFactory factory, object value)
        {
            var sVal = value.ToString();

            if (!blankNodesSubjects.ContainsKey(sVal))
            {
                blankNodesSubjects.Add(sVal, factory.CreateBlankNode());
            }

            return blankNodesSubjects[sVal];
        }

        public INode GetBlankNodeObjectForValue(INodeFactory factory, object value)
        {
            var sVal = value.ToString();

            if (!blankNodesObjects.ContainsKey(sVal))
            {
                blankNodesObjects.Add(sVal, factory.CreateBlankNode());
            }

            return blankNodesObjects[sVal];
        }

        public INotSqlOriginalDbSource OptimizeOnTheFly(INotSqlOriginalDbSource algebra)
        {
            var currentAlgebra = algebra;

            foreach (var optimizer in sqlOptimizers)
            {
                currentAlgebra = optimizer.ProcessAlgebraOnTheFly(currentAlgebra, this);
            }

            return currentAlgebra;
        }
    }
}
