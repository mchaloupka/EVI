using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public class ValueBinder
    {
        private ITermMap r2rmlMap;

        private Dictionary<string, ISqlColumn> columns;

        private static readonly Regex TemplateReplaceRegex = new Regex(@"(?<N>\{)([^\{\}.]+)(?<-N>\})(?(N)(?!))");

        public ValueBinder(ITermMap r2rmlMap)
        {
            this.r2rmlMap = r2rmlMap;
            this.columns = new Dictionary<string, ISqlColumn>();

            if (r2rmlMap.IsConstantValued)
            {
                // No columns needed
            }
            else if (r2rmlMap.IsColumnValued)
            {
                this.columns.Add(r2rmlMap.ColumnName, null);
            }
            else if (r2rmlMap.IsTemplateValued)
            {
                var template = r2rmlMap.Template;

                var columns = GetColumnsFromTemplate(template);

                foreach (var col in columns)
                {
                    this.columns.Add(col, null);
                }
            }
        }

        private IEnumerable<string> GetColumnsFromTemplate(string template)
        {
            var matches = TemplateReplaceRegex.Matches(template);

            foreach (var match in matches.OfType<Match>())
            {
                yield return match.Value.Substring(1, match.Value.Length - 2); // Trim brackets
            }
        }

        public IEnumerable<string> NeededColumns { get { return columns.Keys.ToArray(); } }

        public void SetColumn(string column, ISqlColumn sqlColumn)
        {
            if (this.columns.ContainsKey(column))
            {
                this.columns[column] = sqlColumn;
            }
            else
            {
                throw new Exception("Cannot set column that is not requested for evaluation");
            }
        }
    }
}
