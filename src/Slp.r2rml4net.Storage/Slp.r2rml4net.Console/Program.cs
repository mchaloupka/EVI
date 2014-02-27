using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Console.R2RML;
using VDS.RDF;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            SetDirectory();

            StorageWrapper.AppStart();

            if (StorageWrapper.StartException != null)
            {
                PrintException(StorageWrapper.StartException);
            }
            else
            {
                try
                {
                    ProcessQuery();
                }
                catch (Exception e)
                {
                    PrintException(e);
                }
            }

            StorageWrapper.AppEnd();
        }

        private static void SetDirectory()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Slp.r2rml4net.Server/App_Data")));            
        }

        private static void ProcessQuery()
        {
            var query = string.Empty;

            using (var fs = new FileStream("query.rq", FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(fs))
            {
                query = sr.ReadToEnd();
            }

            var result = StorageWrapper.Storage.Query(query);

            if (result is SparqlResultSet)
            {
                //Print out the Results
                SparqlResultSet rset = (SparqlResultSet)result;
                foreach (SparqlResult res in rset)
                {
                    System.Console.WriteLine(res.ToString());
                }
            }
            else if (result is IGraph)
            {
                //Print out the Results
                IGraph g = (IGraph)result;
                foreach (Triple t in g.Triples)
                {
                    System.Console.WriteLine(t.ToString());
                }
            }
        }

        private static void PrintException(Exception exception)
        {
            System.Console.WriteLine("{0}: {1}", exception.GetType().Name, exception.Message);
        }
    }
}
