using System;
using System.Linq;
using System.Xml.Linq;
using TCode.r2rml4net;
using TCode.r2rml4net.Mapping.Fluent;

namespace Slp.Evi.Test.System.Sparql
{
    public abstract class SparqlTestHelpers<TStorage, TDatabase>
    {
        public TStorage InitializeDataset(string dataset, Func<TDatabase> createSqlDb)
        {
            var sqlDb = createSqlDb();
            var datasetFile = SparqlTestData.GetPath($@"Data\{dataset}\_dataset.xml");

            var doc = XDocument.Load(datasetFile);
            var sqlCommands = doc.Root
                .Elements()
                .Where(x => x.Name == "sql")
                .Single()
                .Elements();

            foreach (var command in sqlCommands)
            {
                if (command.Name == "table")
                    CreateTable(sqlDb, command);
                else if (command.Name == "query")
                    ExecuteQuery(sqlDb, command);
                else
                    throw new Exception(string.Format("Unknown sql command {1} when creating dataset {0}", dataset, command.Name));
            }

            var mappingString = doc.Root.Elements().Where(x => x.Name == "mapping").Single().Value;
            var mapping = R2RMLLoader.Load(mappingString, new MappingOptions());

            return CreateStorage(mapping, createSqlDb());
        }

        protected abstract void CreateTable(TDatabase sqlDb, XElement table);

        protected abstract void ExecuteQuery(TDatabase sqlDb, XElement query);

        protected abstract TStorage CreateStorage(IR2RML mapping, TDatabase database);
    }
}