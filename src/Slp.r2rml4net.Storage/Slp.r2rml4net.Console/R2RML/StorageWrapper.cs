using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using Slp.r2rml4net.Storage;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Sql.Vendor;
using TCode.r2rml4net;
using TCode.r2rml4net.Mapping.Fluent;

namespace Slp.r2rml4net.Console.R2RML
{
    public class StorageWrapper
    {
        private static R2RMLStorage _storage = null;

        public static R2RMLStorage Storage { get { return _storage; } }

        public static void AppStart()
        {
            StartException = null;

            try
            {
                var mappingPath = System.Configuration.ConfigurationManager.AppSettings["r2rmlConfig"];
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["r2rmlstoreconnection"].ConnectionString;

                IR2RML mapping = null;

                var path = mappingPath;

                using (var fs = new FileStream(path, FileMode.Open))
                {
                    mapping = R2RMLLoader.Load(fs);
                }

                _storage = new R2RMLStorage((new DefaultSqlDbFactory()).CreateSQLDb(connectionString), mapping, new DefaultR2RMLStorageFactory());
            }
            catch (Exception e)
            {
                StartException = e;
            }
        }

        public static void AppEnd()
        {
            if (_storage != null)
                _storage.Dispose();
        }

        public static Exception StartException { get; private set; }
    }
}