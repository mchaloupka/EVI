using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    public abstract class BaseSqlDb : ISqlDb
    {
        private BaseSqlQueryBuilder queryBuilder;
        private BaseSqlNameGenerator nameGenerator;

        public BaseSqlDb()
        {
            this.queryBuilder = new BaseSqlQueryBuilder();
            this.nameGenerator = new BaseSqlNameGenerator();
        }

        public abstract IQueryResultReader ExecuteQuery(string query, QueryContext context);

        public string GenerateQuery(INotSqlOriginalDbSource sqlAlgebra, QueryContext context)
        {
            this.nameGenerator.GenerateNames(sqlAlgebra, context);

            return this.queryBuilder.GenerateQuery(sqlAlgebra, context);
        }
    }
}
