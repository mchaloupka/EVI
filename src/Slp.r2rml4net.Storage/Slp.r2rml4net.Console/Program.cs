using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Console.R2RML;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace Slp.r2rml4net.Console
{
    class Program
    {
        static void Main(string[] args)
        {
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

        private static void ProcessQuery()
        {
            var query = string.Empty;

            using (var fsr = new FileStream("query.rq", FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(fsr))
            {
                query = sr.ReadToEnd();
            }

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var result = StorageWrapper.Storage.Query(query);

            watch.Stop();

            System.Console.WriteLine("Query processed in {0}ms", watch.ElapsedMilliseconds);

            using (var fsw = new FileStream("output.txt", FileMode.Create, FileAccess.Write))
            using (var sw = new StreamWriter(fsw))
            {
                if (result is SparqlResultSet)
                {

                    //Print out the Results
                    SparqlResultSet rset = (SparqlResultSet)result;
                    foreach (SparqlResult res in rset)
                    {
                        sw.WriteLine(res.ToString());
                    }
                }
                else if (result is IGraph)
                {
                    CompressingTurtleWriter writer = new CompressingTurtleWriter();
                    writer.Save((IGraph)result, sw);
                }
            }
        }

        private static void PrintException(Exception exception)
        {
            System.Console.WriteLine("{0}: {1}", exception.GetType().Name, exception.Message);
        }
    }
}
