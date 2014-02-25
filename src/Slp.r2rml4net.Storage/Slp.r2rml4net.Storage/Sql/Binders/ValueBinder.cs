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
        private TemplateProcessor templateProcessor;

        public ValueBinder(ITermMap r2rmlMap, TemplateProcessor templateProcessor)
            : this(null, r2rmlMap, templateProcessor)
        {

        }

        public ValueBinder(string variableName, ITermMap r2rmlMap, TemplateProcessor templateProcessor)
        {
            this.r2rmlMap = r2rmlMap;
            this.columns = new Dictionary<string, ISqlColumn>();
            this.VariableName = variableName;
            this.templateProcessor = templateProcessor;

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

                var columns = templateProcessor.GetColumnsFromTemplate(template);

                foreach (var col in columns)
                {
                    this.columns.Add(col, null);
                }
            }
        }

        public string VariableName { get; private set; }

        public ITermMap R2RMLMap { get { return r2rmlMap; } }

        public TemplateProcessor TemplateProcessor { get { return templateProcessor; } }

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

        public ISqlColumn GetColumn(string column)
        {
            if (this.columns.ContainsKey(column))
            {
                return this.columns[column];
            }
            else
            {
                throw new Exception("Cannot get column that is not requested for evaluation");
            }
        }
    }
}
