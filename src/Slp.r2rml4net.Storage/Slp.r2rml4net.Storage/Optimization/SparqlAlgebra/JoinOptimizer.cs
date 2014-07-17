using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;
using TCode.r2rml4net.Mapping;
using Slp.r2rml4net.Storage.Utils;
using VDS.RDF.Query.Patterns;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Mapping.Utils;
using TCode.r2rml4net;

namespace Slp.r2rml4net.Storage.Optimization.SparqlAlgebra
{
    /// <summary>
    /// Join optimizer.
    /// </summary>
    public class JoinOptimizer : ISparqlAlgebraOptimizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoinOptimizer"/> class.
        /// </summary>
        public JoinOptimizer()
        {
            this.canMatchCache = new Dictionary<ITermMap, Dictionary<ITermMap, bool>>();
        }

        /// <summary>
        /// The template processor
        /// </summary>
        private TemplateProcessor templateProcessor = new TemplateProcessor();

        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context)
        {
            if (algebra is JoinOp)
            {
                return ProcessJoin((JoinOp)algebra, context).FinalizeAfterTransform();
            }
            else
            {
                var innerQueries = algebra.GetInnerQueries().ToList();

                foreach (var query in innerQueries)
                {
                    var processed = ProcessAlgebra(query, context);

                    if (processed != query)
                    {
                        algebra.ReplaceInnerQuery(query, processed);
                    }
                }

                return algebra.FinalizeAfterTransform();
            }
        }

        /// <summary>
        /// Processes the join.
        /// </summary>
        /// <param name="joinOp">The join operator.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        private ISparqlQuery ProcessJoin(JoinOp joinOp, QueryContext context)
        {
            var bgps = joinOp.GetInnerQueries().OfType<BgpOp>();

            Dictionary<string, List<ITermMap>> variables = new Dictionary<string, List<ITermMap>>();

            foreach (var bgp in bgps)
            {
                GetBgpInfo(bgp, variables, context);
            }

            foreach (var bgp in bgps)
            {
                if (!ProcessBgp(bgp, variables, context))
                    return new NoSolutionOp();
            }

            return joinOp;
        }

        /// <summary>
        /// Gets the BGP information.
        /// </summary>
        /// <param name="bgp">The BGP.</param>
        /// <param name="variables">The variables mappings.</param>
        /// <param name="context">The query context.</param>
        /// <exception cref="System.Exception">There must be an object map or ref object map</exception>
        public void GetBgpInfo(BgpOp bgp, Dictionary<string, List<ITermMap>> variables, QueryContext context)
        {
            if (bgp.SubjectPattern is VariablePattern)
                GetPatternInfo(((VariablePattern)bgp.SubjectPattern).VariableName, bgp.R2RMLSubjectMap, variables, context);
            else if (bgp.SubjectPattern is BlankNodePattern)
                GetPatternInfo(((BlankNodePattern)bgp.SubjectPattern).ID, bgp.R2RMLSubjectMap, variables, context);

            if (bgp.PredicatePattern is VariablePattern)
                GetPatternInfo(((VariablePattern)bgp.PredicatePattern).VariableName, bgp.R2RMLPredicateMap, variables, context);
            else if (bgp.PredicatePattern is BlankNodePattern)
                GetPatternInfo(((BlankNodePattern)bgp.PredicatePattern).ID, bgp.R2RMLPredicateMap, variables, context);

            if (bgp.R2RMLObjectMap != null)
            {
                if (bgp.ObjectPattern is VariablePattern)
                    GetPatternInfo(((VariablePattern)bgp.ObjectPattern).VariableName, bgp.R2RMLObjectMap, variables, context);
                else if (bgp.ObjectPattern is BlankNodePattern)
                    GetPatternInfo(((BlankNodePattern)bgp.ObjectPattern).ID, bgp.R2RMLObjectMap, variables, context);
            }
            else if (bgp.R2RMLRefObjectMap != null)
            {
                var parentMap = bgp.R2RMLRefObjectMap.GetParentTriplesMap(context.Mapping.Mapping);

                if (bgp.ObjectPattern is VariablePattern)
                    GetPatternInfo(((VariablePattern)bgp.ObjectPattern).VariableName, parentMap.SubjectMap, variables, context);
                else if (bgp.ObjectPattern is BlankNodePattern)
                    GetPatternInfo(((BlankNodePattern)bgp.ObjectPattern).ID, parentMap.SubjectMap, variables, context);
            }
            else
                throw new Exception("There must be an object map or ref object map");
        }

        /// <summary>
        /// Gets the pattern information.
        /// </summary>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="termMap">The term map.</param>
        /// <param name="variables">The variables mappings.</param>
        /// <param name="context">The query context.</param>
        private void GetPatternInfo(string varName, ITermMap termMap, Dictionary<string, List<ITermMap>> variables, QueryContext context)
        {
            if (!variables.ContainsKey(varName))
            {
                variables.Add(varName, new List<ITermMap>());
            }

            variables[varName].Add(termMap);
        }

        /// <summary>
        /// Processes the BGP.
        /// </summary>
        /// <param name="bgp">The BGP.</param>
        /// <param name="variables">The variables mappings.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the variables can match also other mappings, <c>false</c> otherwise.</returns>
        public bool ProcessBgp(BgpOp bgp, Dictionary<string, List<ITermMap>> variables, QueryContext context)
        {
            bool ok = true;

            if (bgp.SubjectPattern is VariablePattern)
                ok = ok && ProcessPatternInfo(((VariablePattern)bgp.SubjectPattern).VariableName, bgp.R2RMLSubjectMap, variables, context);
            else if (bgp.SubjectPattern is BlankNodePattern)
                ok = ok && ProcessPatternInfo(((BlankNodePattern)bgp.SubjectPattern).ID, bgp.R2RMLSubjectMap, variables, context);

            if (bgp.PredicatePattern is VariablePattern)
                ok = ok && ProcessPatternInfo(((VariablePattern)bgp.PredicatePattern).VariableName, bgp.R2RMLPredicateMap, variables, context);
            else if (bgp.PredicatePattern is BlankNodePattern)
                ok = ok && ProcessPatternInfo(((BlankNodePattern)bgp.PredicatePattern).ID, bgp.R2RMLPredicateMap, variables, context);

            if (bgp.R2RMLObjectMap != null)
            {
                if (bgp.ObjectPattern is VariablePattern)
                    ok = ok && ProcessPatternInfo(((VariablePattern)bgp.ObjectPattern).VariableName, bgp.R2RMLObjectMap, variables, context);
                else if (bgp.ObjectPattern is BlankNodePattern)
                    ok = ok && ProcessPatternInfo(((BlankNodePattern)bgp.ObjectPattern).ID, bgp.R2RMLObjectMap, variables, context);
            }
            else if (bgp.R2RMLRefObjectMap != null)
            {
                var parentMap = bgp.R2RMLRefObjectMap.GetParentTriplesMap(context.Mapping.Mapping);

                if (bgp.ObjectPattern is VariablePattern)
                    ok = ok && ProcessPatternInfo(((VariablePattern)bgp.ObjectPattern).VariableName, parentMap.SubjectMap, variables, context);
                else if (bgp.ObjectPattern is BlankNodePattern)
                    ok = ok && ProcessPatternInfo(((BlankNodePattern)bgp.ObjectPattern).ID, parentMap.SubjectMap, variables, context);
            }

            return ok;
        }

        /// <summary>
        /// Processes the pattern information.
        /// </summary>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="termMap">The term map.</param>
        /// <param name="variables">The variables mappings.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the pattern can match also other mappings, <c>false</c> otherwise.</returns>
        private bool ProcessPatternInfo(string varName, ITermMap termMap, Dictionary<string, List<ITermMap>> variables, QueryContext context)
        {
            if(variables.ContainsKey(varName))
            {
                var termMaps = variables[varName];

                foreach (var item in termMaps)
                {
                    if (!CanMatch(termMap, item, context))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The cache for can match
        /// </summary>
        private Dictionary<ITermMap, Dictionary<ITermMap, bool>> canMatchCache;

        /// <summary>
        /// Gets the cached can match.
        /// </summary>
        /// <param name="first">The first mapping.</param>
        /// <param name="second">The second mapping.</param>
        /// <returns><c>true</c> if they can match, <c>false</c> otherwise.</returns>
        private bool? GetCachedCanMatch(ITermMap first, ITermMap second)
        {
            if(canMatchCache.ContainsKey(first))
            {
                var c = canMatchCache[first];

                if (c.ContainsKey(second))
                    return c[second];
            }
            
            if(canMatchCache.ContainsKey(second))
            {
                var c = canMatchCache[second];

                if (c.ContainsKey(first))
                {
                    var val = c[first];
                    SetCanMatchCache(first, second, val);
                    return val;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the can match cache.
        /// </summary>
        /// <param name="first">The first term.</param>
        /// <param name="second">The second term.</param>
        /// <param name="value">The value to set.</param>
        private void SetCanMatchCache(ITermMap first, ITermMap second, bool value)
        {
            if (!canMatchCache.ContainsKey(first))
                canMatchCache.Add(first, new Dictionary<ITermMap, bool>());

            if (!canMatchCache[first].ContainsKey(second))
                canMatchCache[first].Add(second, value);
            else
                canMatchCache[first][second] = value;
        }

        /// <summary>
        /// Determines whether the first mapping can match the second mapping.
        /// </summary>
        /// <param name="first">The first mapping.</param>
        /// <param name="second">The second mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
        private bool CanMatch(ITermMap first, ITermMap second, QueryContext context)
        {
            var res = GetCachedCanMatch(first, second);

            if (!res.HasValue)
            {
                var result = CanMatchFunction<bool>(first,
                    constantUriFunc: x => CanMatch(x, second, context),
                    constantLiteralFunc: x => CanMatch(x, second, context),
                    columnFunc: x => CanColumnMatch(x, second, context),
                    templateFunc: x => CanTemplateMatch(x, second, context));

                SetCanMatchCache(first, second, result);
                return result;
            }
            else
            {
                return res.Value;
            }
        }

        /// <summary>
        /// Determines whether the first template mapping can match the second mapping.
        /// </summary>
        /// <param name="first">The first template mapping.</param>
        /// <param name="second">The second mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
        private bool CanTemplateMatch(ITermMap first, ITermMap second, QueryContext context)
        {
            return CanMatchFunction<bool>(second,
                constantUriFunc: x => CanMatchTemplate(x, first, context),
                constantLiteralFunc: x => CanMatchTemplate(x, first, context),
                columnFunc: x => CanTemplateMatchColumn(first, x, context),
                templateFunc: x => CanTemplatesMatch(first, x, context));
        }

        /// <summary>
        /// Determines whether the first column mapping can match the second mapping.
        /// </summary>
        /// <param name="first">The first column mapping.</param>
        /// <param name="second">The second mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
        private bool CanColumnMatch(ITermMap first, ITermMap second, QueryContext context)
        {
            return CanMatchFunction<bool>(second,
                constantUriFunc: x => CanMatchColumn(x, first, context),
                constantLiteralFunc: x => CanMatchColumn(x, first, context),
                columnFunc: x => CanColumnsMatch(first, x, context),
                templateFunc: x => CanTemplateMatchColumn(x, first, context));
        }

        /// <summary>
        /// Determines whether the first literal mapping can match the second mapping.
        /// </summary>
        /// <param name="literal">The literal mapping.</param>
        /// <param name="second">The second mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
        private bool CanMatch(string literal, ITermMap second, QueryContext context)
        {
            return CanMatchFunction<bool>(second,
                constantUriFunc: x => CanMatch(literal, x, context),
                constantLiteralFunc: x => CanMatch(literal, x, context),
                columnFunc: x => CanMatchColumn(literal, x, context),
                templateFunc: x => CanMatchTemplate(literal, x, context)
                );
        }

        /// <summary>
        /// Determines whether the first URI mapping can match the second mapping.
        /// </summary>
        /// <param name="uri">The first URI mapping.</param>
        /// <param name="second">The second mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
        private bool CanMatch(Uri uri, ITermMap second, QueryContext context)
        {
            return CanMatchFunction<bool>(second,
                constantUriFunc: x => CanMatch(uri, x, context),
                constantLiteralFunc: x => CanMatch(x, uri, context),
                columnFunc: x => CanMatchColumn(uri, x, context),
                templateFunc: x => CanMatchTemplate(uri, x, context));
        }

        /// <summary>
        /// Determines whether the URIs can match.
        /// </summary>
        /// <param name="firstUri">The first URI.</param>
        /// <param name="secondUri">The second URI.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the URIs can match; otherwise, <c>false</c>.</returns>
        private bool CanMatch(Uri firstUri, Uri secondUri, QueryContext context)
        {
            return firstUri.UriEquals(secondUri);
        }

        /// <summary>
        /// Determines whether the literals can match.
        /// </summary>
        /// <param name="firstLiteral">The first literal.</param>
        /// <param name="secondLiteral">The second literal.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the literals can match; otherwise, <c>false</c>.</returns>
        private bool CanMatch(string firstLiteral, string secondLiteral, QueryContext context)
        {
            return firstLiteral == secondLiteral;
        }

        /// <summary>
        /// Determines whether the literal can match uri.
        /// </summary>
        /// <param name="literal">The first literal.</param>
        /// <param name="uri">The second URI.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>false</c>.</returns>
        private bool CanMatch(string literal, Uri uri, QueryContext context)
        {
            return false;
        }

        /// <summary>
        /// Determines whether the template mappings can match.
        /// </summary>
        /// <param name="firstTemplateTermMap">The first template mapping.</param>
        /// <param name="secondTemplateTermMap">The second template mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the template mappings can match; otherwise, <c>false</c>.</returns>
        private bool CanTemplatesMatch(ITermMap firstTemplateTermMap, ITermMap secondTemplateTermMap, QueryContext context)
        {
            if ((firstTemplateTermMap.TermType.IsLiteral && secondTemplateTermMap.TermType.IsURI) || (firstTemplateTermMap.TermType.IsURI && secondTemplateTermMap.TermType.IsLiteral))
                return false;

            return TemplatesMatchCheck(firstTemplateTermMap.Template, secondTemplateTermMap.Template, firstTemplateTermMap.TermType.IsURI);
        }

        /// <summary>
        /// Determines whether the template mapping can match the column mapping.
        /// </summary>
        /// <param name="firstTemplateTermMap">The first template mapping.</param>
        /// <param name="secondColumnTermMap">The second column mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
        private bool CanTemplateMatchColumn(ITermMap firstTemplateTermMap, ITermMap secondColumnTermMap, QueryContext context)
        {
            if ((firstTemplateTermMap.TermType.IsLiteral && secondColumnTermMap.TermType.IsURI) || (firstTemplateTermMap.TermType.IsURI && secondColumnTermMap.TermType.IsLiteral))
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether the literal can match the template mapping.
        /// </summary>
        /// <param name="literal">The literal.</param>
        /// <param name="templateTermMap">The template mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
        private bool CanMatchTemplate(string literal, ITermMap templateTermMap, QueryContext context)
        {
            if (templateTermMap.TermType.IsURI)
                return false;

            return TemplateMatchCheck(templateTermMap.Template, literal, false);
        }

        /// <summary>
        /// Determines whether the URI can match the template mapping.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="templateTermMap">The template mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
        private bool CanMatchTemplate(Uri uri, ITermMap templateTermMap, QueryContext context)
        {
            if (templateTermMap.TermType.IsLiteral)
                return false;

            return TemplateMatchCheck(templateTermMap.Template, uri.ToString(), true);
        }

        /// <summary>
        /// Determines whether the column mappings can match.
        /// </summary>
        /// <param name="firstColumnTermMap">The first column mapping.</param>
        /// <param name="secondColumnTermMap">The second column mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
        private bool CanColumnsMatch(ITermMap firstColumnTermMap, ITermMap secondColumnTermMap, QueryContext context)
        {
            bool ok = true;

            if ((firstColumnTermMap.TermType.IsLiteral && secondColumnTermMap.TermType.IsURI) || (firstColumnTermMap.TermType.IsURI && secondColumnTermMap.TermType.IsLiteral))
                ok = false;

            // NOTE: Maybe type checking

            return ok;
        }

        /// <summary>
        /// Determines whether the literal can match the column mapping.
        /// </summary>
        /// <param name="literal">The literal.</param>
        /// <param name="columnTermMap">The column mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
        private bool CanMatchColumn(string literal, ITermMap columnTermMap, QueryContext context)
        {
            if (columnTermMap.TermType.IsLiteral)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determines whether the URI can match the column mapping.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="columnTermMap">The column mapping.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
        private bool CanMatchColumn(Uri uri, ITermMap columnTermMap, QueryContext context)
        {
            if (columnTermMap.TermType.IsURI)
                return true;
            else
                return false;
        }

        /// <summary>
        /// The can match function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="decider">The mapping chosen as decider which function will be used.</param>
        /// <param name="constantUriFunc">The constant URI function.</param>
        /// <param name="constantLiteralFunc">The constant literal function.</param>
        /// <param name="columnFunc">The column function.</param>
        /// <param name="templateFunc">The template function.</param>
        /// <exception cref="System.Exception">
        /// Object map must be an IRI or Literal
        /// or
        /// Constant value term must be uri valued or an object map
        /// or
        /// Term must be constant, column or template valued
        /// </exception>
        private T CanMatchFunction<T>(ITermMap decider, Func<Uri, T> constantUriFunc, Func<string, T> constantLiteralFunc, Func<ITermMap, T> columnFunc, Func<ITermMap, T> templateFunc)
        {
            if (decider.IsConstantValued)
            {
                if (decider is IUriValuedTermMap)
                {
                    var uri = ((IUriValuedTermMap)decider).URI;
                    return constantUriFunc(uri);
                }
                else if (decider is IObjectMap)
                {
                    var oMap = (IObjectMap)decider;

                    if (oMap.URI != null)
                    {
                        return constantUriFunc(oMap.URI);
                    }
                    else if (oMap.Literal != null)
                    {
                        return constantLiteralFunc(oMap.Literal);
                    }
                    else
                        throw new Exception("Object map must be an IRI or Literal");
                }
                else
                    throw new Exception("Constant value term must be uri valued or an object map");
            }
            else if (decider.IsColumnValued)
            {
                return columnFunc(decider);
            }
            else if (decider.IsTemplateValued)
            {
                return templateFunc(decider);
            }
            else
                throw new Exception("Term must be constant, column or template valued");
        }

        /// <summary>
        /// Can the template match the value.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="value">The value.</param>
        /// <param name="isIri">if set to <c>true</c> the values are IRI.</param>
        /// <returns><c>true</c> if the template can match the value, <c>false</c> otherwise.</returns>
        private bool TemplateMatchCheck(string template, string value, bool isIri)
        {
            var templateParts = this.templateProcessor.ParseTemplate(template).ToArray();
            var secondParts = new ITemplatePart[] { new TemplateProcessor.TextTemplatePart(value) };

            return TemplateMatchCheck(string.Empty, string.Empty, 0, templateParts.Length, templateParts, string.Empty, string.Empty, 0, secondParts.Length, secondParts, isIri);
        }

        /// <summary>
        /// Can the templates match.
        /// </summary>
        /// <param name="firstTemplate">The first template.</param>
        /// <param name="secondTemplate">The second template.</param>
        /// <param name="isIri">if set to <c>true</c> the values are IRI.</param>
        /// <returns><c>true</c> if the templates can match, <c>false</c> otherwise.</returns>
        private bool TemplatesMatchCheck(string firstTemplate, string secondTemplate, bool isIri)
        {
            var firstTemplateParts = this.templateProcessor.ParseTemplate(firstTemplate).ToArray();
            var secondTemplateParts = this.templateProcessor.ParseTemplate(secondTemplate).ToArray();

            return TemplateMatchCheck(string.Empty, string.Empty, 0, firstTemplateParts.Length, firstTemplateParts, string.Empty, string.Empty, 0, secondTemplateParts.Length, secondTemplateParts, isIri);
        }

        /// <summary>
        /// Can the templates match.
        /// </summary>
        /// <param name="firstPrefix">The first prefix.</param>
        /// <param name="firstSuffix">The first suffix.</param>
        /// <param name="firstIndex">The first index.</param>
        /// <param name="firstEndIndex">The first index of the end.</param>
        /// <param name="firstParts">The first parts.</param>
        /// <param name="secondPrefix">The second prefix.</param>
        /// <param name="secondSuffix">The second suffix.</param>
        /// <param name="secondIndex">The index of the second.</param>
        /// <param name="secondEndIndex">the end index of the second.</param>
        /// <param name="secondParts">The second parts.</param>
        /// <param name="isIri">if set to <c>true</c> the values are IRI.</param>
        /// <returns><c>true</c> if the templates can match, <c>false</c> otherwise.</returns>
        private bool TemplateMatchCheck(string firstPrefix, string firstSuffix, int firstIndex, int firstEndIndex, ITemplatePart[] firstParts,
            string secondPrefix, string secondSuffix, int secondIndex, int secondEndIndex, ITemplatePart[] secondParts, bool isIri)
        {
            while (firstIndex != firstEndIndex || secondIndex != secondEndIndex)
            {
                ExtractStartEndFromTemplates(ref firstPrefix, ref firstSuffix, ref firstIndex, ref firstEndIndex, firstParts);
                ExtractStartEndFromTemplates(ref secondPrefix, ref secondSuffix, ref secondIndex, ref secondEndIndex, secondParts);

                if (!PrefixMatch(ref firstPrefix, ref secondPrefix))
                    return false;

                if (!SuffixMatch(ref firstSuffix, ref secondSuffix))
                    return false;

                if (!isIri)
                    return true;

                CanColumnMatch(ref firstPrefix, ref firstSuffix, ref firstIndex, ref firstEndIndex, firstParts, ref secondPrefix, ref secondSuffix, ref secondIndex, ref secondEndIndex, secondParts);
            }

            return firstPrefix == secondPrefix && firstSuffix == secondSuffix;
        }

        /// <summary>
        /// Can the templates match, when one of them starts with column
        /// </summary>
        /// <param name="firstPrefix">The first prefix.</param>
        /// <param name="firstSuffix">The first suffix.</param>
        /// <param name="firstIndex">The first index.</param>
        /// <param name="firstEndIndex">The first index of the end.</param>
        /// <param name="firstParts">The first parts.</param>
        /// <param name="secondPrefix">The second prefix.</param>
        /// <param name="secondSuffix">The second suffix.</param>
        /// <param name="secondIndex">The index of the second.</param>
        /// <param name="secondEndIndex">the end index of the second.</param>
        /// <param name="secondParts">The second parts.</param>
        /// <remarks>It does not return a value, it only skips the parts that can match</remarks>
        private void CanColumnMatch(ref string firstPrefix, ref string firstSuffix, ref int firstIndex, ref int firstEndIndex, ITemplatePart[] firstParts, ref string secondPrefix, ref string secondSuffix, ref int secondIndex, ref int secondEndIndex, ITemplatePart[] secondParts)
        {
            if(firstIndex < firstEndIndex && firstParts[firstIndex].IsColumn)
            {
                firstIndex++;
            }
            else if (secondIndex < secondEndIndex && secondParts[secondIndex].IsColumn)
            {
                secondIndex++;
            }
            else
            {
                return; // No colums found
            }

            SkipToFirstNotIUnreserverdCharacter(ref firstPrefix, ref firstSuffix, ref firstIndex, ref firstEndIndex, firstParts);
            SkipToFirstNotIUnreserverdCharacter(ref secondPrefix, ref secondSuffix, ref secondIndex, ref secondEndIndex, secondParts);
            return;
        }

        /// <summary>
        /// Can suffixes match
        /// </summary>
        /// <param name="firstSuffix">The first suffix.</param>
        /// <param name="secondSuffix">The second suffix.</param>
        /// <returns><c>true</c> if they can match, <c>false</c> otherwise.</returns>
        private bool SuffixMatch(ref string firstSuffix, ref string secondSuffix)
        {
            int index = 0;

            while (index < firstSuffix.Length && index < secondSuffix.Length)
            {
                if (firstSuffix[firstSuffix.Length - index - 1] != secondSuffix[secondSuffix.Length - index - 1])
                    return false;

                index++;
            }

            if (index == firstSuffix.Length)
                firstSuffix = string.Empty;
            else
                firstSuffix = firstSuffix.Substring(0, firstSuffix.Length - index);

            if (index == secondSuffix.Length)
                secondSuffix = string.Empty;
            else
                secondSuffix = secondSuffix.Substring(0, secondSuffix.Length - index);

            return true;
        }

        /// <summary>
        /// Can prefixes match.
        /// </summary>
        /// <param name="firstPrefix">The first prefix.</param>
        /// <param name="secondPrefix">The second prefix.</param>
        /// <returns><c>true</c> if they can match, <c>false</c> otherwise.</returns>
        private bool PrefixMatch(ref string firstPrefix, ref string secondPrefix)
        {
            int index = 0;

            while (index < firstPrefix.Length && index < secondPrefix.Length)
            {
                if (firstPrefix[index] != secondPrefix[index])
                    return false;

                index++;
            }

            if (index == firstPrefix.Length)
                firstPrefix = string.Empty;
            else
                firstPrefix = firstPrefix.Substring(index);

            if (index == secondPrefix.Length)
                secondPrefix = string.Empty;
            else
                secondPrefix = secondPrefix.Substring(index);

            return true;
        }

        /// <summary>
        /// Extracts the start and end from template.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="suffix">The suffix.</param>
        /// <param name="index">The index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="parts">The parts.</param>
        private static void ExtractStartEndFromTemplates(ref string prefix, ref string suffix, ref int index, ref int endIndex, ITemplatePart[] parts)
        {
            while (index < endIndex && parts[index].IsText)
            {
                prefix += parts[index++].Text;
            }

            while (endIndex > index && parts[endIndex - 1].IsText)
            {
                suffix = parts[endIndex-- - 1].Text + suffix;
            }
        }

        /// <summary>
        /// Skips to first not iunreserverd character.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="suffix">The suffix.</param>
        /// <param name="index">The index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="parts">The parts.</param>
        private void SkipToFirstNotIUnreserverdCharacter(ref string prefix, ref string suffix, ref int index, ref int endIndex, ITemplatePart[] parts)
        {
            int prefixSkip;
            for (prefixSkip = 0; prefixSkip < prefix.Length; prefixSkip++)
            {
                if(!MappingHelper.IsIUnreserved(prefix[prefixSkip]))
                {
                    prefix = prefix.Substring(prefixSkip);
                    return;
                }
            }

            prefix = string.Empty;

            while (index < endIndex)
            {
                var current = parts[index];

                if (current.IsText)
                {
                    ExtractStartEndFromTemplates(ref prefix, ref suffix, ref index, ref endIndex, parts);
                    SkipToFirstNotIUnreserverdCharacter(ref prefix, ref suffix, ref index, ref endIndex, parts);
                    return;
                }
                else
                {
                    index++;
                }
            }

            if(!string.IsNullOrEmpty(suffix))
            {
                prefix = suffix;
                suffix = string.Empty;
                SkipToFirstNotIUnreserverdCharacter(ref prefix, ref suffix, ref index, ref endIndex, parts);
                return;
            }
        }
    }
}

