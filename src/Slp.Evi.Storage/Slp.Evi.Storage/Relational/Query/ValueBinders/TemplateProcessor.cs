using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Slp.Evi.Storage.Relational.Query.ValueBinders
{
    /// <summary>
    /// Template processor
    /// </summary>
    public class TemplateProcessor
    {
        /// <summary>
        /// The template replace regex
        /// </summary>
        private static readonly Regex TemplateReplaceRegex = new Regex(@"(?<N>\{)([^\{\}.]+)(?<-N>\})(?(N)(?!))");

        /// <summary>
        /// Gets the columns from template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>Columns used in template.</returns>
        public IEnumerable<string> GetColumnsFromTemplate(string template)
        {
            return ParseTemplate(template).OfType<ColumnTemplatePart>().Select(x => x.Column).Distinct();
        }

        /// <summary>
        /// Parses the template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>The template parts.</returns>
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

        /// <summary>
        /// Template column part
        /// </summary>
        public class ColumnTemplatePart : ITemplatePart, IEquatable<ColumnTemplatePart>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ColumnTemplatePart"/> class.
            /// </summary>
            /// <param name="column">The column.</param>
            public ColumnTemplatePart(string column)
            {
                Column = column;
            }

            /// <summary>
            /// Gets the column.
            /// </summary>
            /// <value>The column.</value>
            public string Column { get; }

            /// <summary>
            /// Gets a value indicating whether this instance is column.
            /// </summary>
            /// <value><c>true</c> if this instance is column; otherwise, <c>false</c>.</value>
            public bool IsColumn => true;

            /// <summary>
            /// Gets a value indicating whether this instance is text.
            /// </summary>
            /// <value><c>true</c> if this instance is text; otherwise, <c>false</c>.</value>
            public bool IsText => false;

            /// <summary>
            /// Gets the text.
            /// </summary>
            /// <value>The text.</value>
            /// <exception cref="System.Exception">Asked for text on ColumnTemplatePart</exception>
            public string Text
            {
                get { throw new Exception("Asked for text on ColumnTemplatePart"); }
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(ColumnTemplatePart)) return false;
                return Equals((ColumnTemplatePart) obj);
            }

            /// <inheritdoc />
            public bool Equals(ColumnTemplatePart other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Column, other.Column);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return (Column != null ? Column.GetHashCode() : 0);
            }

            public static bool operator ==(ColumnTemplatePart left, ColumnTemplatePart right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(ColumnTemplatePart left, ColumnTemplatePart right)
            {
                return !Equals(left, right);
            }
        }

        /// <summary>
        /// Template text part
        /// </summary>
        public class TextTemplatePart : ITemplatePart, IEquatable<TextTemplatePart>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TextTemplatePart"/> class.
            /// </summary>
            /// <param name="text">The text.</param>
            public TextTemplatePart(string text)
            {
                Text = text;
            }

            /// <summary>
            /// Gets the text.
            /// </summary>
            /// <value>The text.</value>
            public string Text { get; }

            /// <summary>
            /// Gets a value indicating whether this instance is column.
            /// </summary>
            /// <value><c>true</c> if this instance is column; otherwise, <c>false</c>.</value>
            public bool IsColumn => false;

            /// <summary>
            /// Gets a value indicating whether this instance is text.
            /// </summary>
            /// <value><c>true</c> if this instance is text; otherwise, <c>false</c>.</value>
            public bool IsText => true;

            /// <summary>
            /// Gets the column.
            /// </summary>
            /// <value>The column.</value>
            /// <exception cref="System.Exception">Asked for column on TextTemplatePart</exception>
            public string Column
            {
                get { throw new Exception("Asked for column on TextTemplatePart"); }
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(TextTemplatePart)) return false;
                return Equals((TextTemplatePart) obj);
            }

            /// <inheritdoc />
            public bool Equals(TextTemplatePart other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Text, other.Text);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return (Text != null ? Text.GetHashCode() : 0);
            }

            public static bool operator ==(TextTemplatePart left, TextTemplatePart right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(TextTemplatePart left, TextTemplatePart right)
            {
                return !Equals(left, right);
            }
        }
    }

    /// <summary>
    /// Template part
    /// </summary>
    public interface ITemplatePart
    {
        /// <summary>
        /// Gets a value indicating whether this instance is column.
        /// </summary>
        /// <value><c>true</c> if this instance is column; otherwise, <c>false</c>.</value>
        bool IsColumn { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is text.
        /// </summary>
        /// <value><c>true</c> if this instance is text; otherwise, <c>false</c>.</value>
        bool IsText { get; }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <value>The column.</value>
        string Column { get; }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        string Text { get; }
    }
}
