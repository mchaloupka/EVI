namespace Slp.Evi.Storage.MySql.Database
{
    public class MySqlQuery
    {
        public MySqlQuery(string queryString)
        {
            QueryString = queryString;
        }

        public string QueryString { get; }
    }
}
