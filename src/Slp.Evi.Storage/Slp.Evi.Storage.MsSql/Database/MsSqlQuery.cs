namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlQuery
    {
        public MsSqlQuery(string queryString)
        {
            QueryString = queryString;
        }

        public string QueryString { get; }
    }
}
