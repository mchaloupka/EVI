namespace Slp.r2rml4net.Storage.DBSchema
{
    /// <summary>
    /// Provider for functions related to database schema.
    /// </summary>
    public class DbSchemaProvider
    {
        /// <summary>
        /// The database
        /// </summary>
        private Sql.ISqlDb db;


        public DbSchemaProvider(Sql.ISqlDb db)
        {
            this.db = db;
        }

    }
}
