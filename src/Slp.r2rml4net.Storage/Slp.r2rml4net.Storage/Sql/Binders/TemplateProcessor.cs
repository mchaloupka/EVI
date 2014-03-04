using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public class TemplateProcessor
    {
        private static readonly Regex TemplateReplaceRegex = new Regex(@"(?<N>\{)([^\{\}.]+)(?<-N>\})(?(N)(?!))");

        public IEnumerable<string> GetColumnsFromTemplate(string template)
        {
            return ParseTemplate(template).OfType<ColumnTemplatePart>().Select(x => x.Column).Distinct();
        }

        public IEnumerable<ITemplatePart> ParseTemplate(string template)
        {
            var matches = TemplateReplaceRegex.Matches(template).OfType<Match>().OrderBy(x => x.Index);

            int curIndex = 0;

            foreach (var match in matches)
            {
                if (match.Index > curIndex)
                {
                    yield return new TextTemplatePart(template.Substring(curIndex, match.Index - curIndex));
                }

                yield return new ColumnTemplatePart(template.Substring(match.Index + 1, match.Length - 2));

                curIndex = match.Index + match.Length;
            }

            if (curIndex != template.Length)
            {
                yield return new TextTemplatePart(template.Substring(curIndex));
            }
        }

        private class ColumnTemplatePart : ITemplatePart
        {
            public ColumnTemplatePart(string column)
            {
                this.Column = column;
            }

            public string Column { get; private set; }

            public bool IsColumn
            {
                get { return true; }
            }

            public bool IsText
            {
                get { return false; }
            }

            public string Text
            {
                get { throw new Exception("Asked for text on ColumnTemplatePart"); }
            }
        }

        private class TextTemplatePart : ITemplatePart
        {
            public TextTemplatePart(string text)
            {
                this.Text = text;
            }

            public string Text { get; private set; }

            public bool IsColumn
            {
                get { return false; }
            }

            public bool IsText
            {
                get { return true; }
            }

            public string Column
            {
                get { throw new Exception("Asked for column on TextTemplatePart"); }
            }
        }
    }

    public interface ITemplatePart
    {
        bool IsColumn { get; }

        bool IsText { get; }

        string Column { get; }

        string Text { get; }
    }
}
