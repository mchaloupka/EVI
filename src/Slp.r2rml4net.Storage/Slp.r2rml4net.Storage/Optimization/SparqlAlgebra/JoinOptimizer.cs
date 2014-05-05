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
    public class JoinOptimizer : ISparqlAlgebraOptimizer
    {
        public JoinOptimizer()
        {
            this.canMatchCache = new Dictionary<ITermMap, Dictionary<ITermMap, bool>>();
        }

        private TemplateProcessor templateProcessor = new TemplateProcessor();

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

        private void GetPatternInfo(string varName, ITermMap termMap, Dictionary<string, List<ITermMap>> variables, QueryContext context)
        {
            if (!variables.ContainsKey(varName))
            {
                variables.Add(varName, new List<ITermMap>());
            }

            variables[varName].Add(termMap);
        }

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

        private bool ProcessPatternInfo(string varName, ITermMap termMap, Dictionary<string, List<ITermMap>> variables, QueryContext context)
        {
            var termMaps = variables[varName];

            foreach (var item in termMaps)
            {
                if (!CanMatch(termMap, item, context))
                    return false;
            }

            return true;
        }

        private Dictionary<ITermMap, Dictionary<ITermMap, bool>> canMatchCache;

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
                    return c[first];
            }

            return null;
        }

        private void SetCanMatchCache(ITermMap first, ITermMap second, bool value)
        {
            if (!canMatchCache.ContainsKey(first))
                canMatchCache.Add(first, new Dictionary<ITermMap, bool>());

            if (!canMatchCache[first].ContainsKey(second))
                canMatchCache[first].Add(second, value);
            else
                canMatchCache[first][second] = value;
        }

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

        private bool CanTemplateMatch(ITermMap first, ITermMap second, QueryContext context)
        {
            return CanMatchFunction<bool>(second,
                constantUriFunc: x => CanMatchTemplate(x, first, context),
                constantLiteralFunc: x => CanMatchTemplate(x, first, context),
                columnFunc: x => CanTemplateMatchColumn(first, x, context),
                templateFunc: x => CanTemplatesMatch(first, x, context));
        }

        private bool CanColumnMatch(ITermMap first, ITermMap second, QueryContext context)
        {
            return CanMatchFunction<bool>(second,
                constantUriFunc: x => CanMatchColumn(x, first, context),
                constantLiteralFunc: x => CanMatchColumn(x, first, context),
                columnFunc: x => CanColumnsMatch(first, x, context),
                templateFunc: x => CanTemplateMatchColumn(x, first, context));
        }

        private bool CanMatch(string literal, ITermMap second, QueryContext context)
        {
            return CanMatchFunction<bool>(second,
                constantUriFunc: x => CanMatch(literal, x, context),
                constantLiteralFunc: x => CanMatch(literal, x, context),
                columnFunc: x => CanMatchColumn(literal, x, context),
                templateFunc: x => CanMatchTemplate(literal, x, context)
                );
        }

        private bool CanMatch(Uri uri, ITermMap second, QueryContext context)
        {
            return CanMatchFunction<bool>(second,
                constantUriFunc: x => CanMatch(uri, x, context),
                constantLiteralFunc: x => CanMatch(x, uri, context),
                columnFunc: x => CanMatchColumn(uri, x, context),
                templateFunc: x => CanMatchTemplate(uri, x, context));
        }

        private bool CanMatch(Uri firstUri, Uri secondUri, QueryContext context)
        {
            return firstUri.UriEquals(secondUri);
        }

        private bool CanMatch(string firstLiteral, string secondLiteral, QueryContext context)
        {
            return firstLiteral == secondLiteral;
        }

        private bool CanMatch(string literal, Uri uri, QueryContext context)
        {
            return false;
        }

        private bool CanTemplatesMatch(ITermMap firstTemplateTermMap, ITermMap secondTemplateTermMap, QueryContext context)
        {
            if ((firstTemplateTermMap.TermType.IsLiteral && secondTemplateTermMap.TermType.IsURI) || (firstTemplateTermMap.TermType.IsURI && secondTemplateTermMap.TermType.IsLiteral))
                return false;

            return TemplatesMatchCheck(firstTemplateTermMap.Template, secondTemplateTermMap.Template, firstTemplateTermMap.TermType.IsURI);
        }

        private bool CanTemplateMatchColumn(ITermMap firstTemplateTermMap, ITermMap secondColumnTermMap, QueryContext context)
        {
            if ((firstTemplateTermMap.TermType.IsLiteral && secondColumnTermMap.TermType.IsURI) || (firstTemplateTermMap.TermType.IsURI && secondColumnTermMap.TermType.IsLiteral))
                return false;

            return true;
        }

        private bool CanMatchTemplate(string literal, ITermMap templateTermMap, QueryContext context)
        {
            if (templateTermMap.TermType.IsURI)
                return false;

            return TemplateMatchCheck(templateTermMap.Template, literal, false);
        }

        private bool CanMatchTemplate(Uri uri, ITermMap templateTermMap, QueryContext context)
        {
            if (templateTermMap.TermType.IsLiteral)
                return false;

            return TemplateMatchCheck(templateTermMap.Template, uri.ToString(), true);
        }

        private bool CanColumnsMatch(ITermMap firstColumnTermMap, ITermMap secondColumnTermMap, QueryContext context)
        {
            bool ok = true;

            if ((firstColumnTermMap.TermType.IsLiteral && secondColumnTermMap.TermType.IsURI) || (firstColumnTermMap.TermType.IsURI && secondColumnTermMap.TermType.IsLiteral))
                ok = false;

            // NOTE: Maybe type checking

            return ok;
        }

        private bool CanMatchColumn(string literal, ITermMap columnTermMap, QueryContext context)
        {
            if (columnTermMap.TermType.IsLiteral)
                return true;
            else
                return false;
        }

        private bool CanMatchColumn(Uri uri, ITermMap columnTermMap, QueryContext context)
        {
            if (columnTermMap.TermType.IsURI)
                return true;
            else
                return false;
        }

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

        private bool TemplateMatchCheck(string template, string value, bool isIri)
        {
            var templateParts = this.templateProcessor.ParseTemplate(template).ToArray();
            var secondParts = new ITemplatePart[] { new TemplateProcessor.TextTemplatePart(value) };

            return TemplateMatchCheck(string.Empty, string.Empty, 0, templateParts.Length, templateParts, string.Empty, string.Empty, 0, secondParts.Length, secondParts, isIri);
        }

        private bool TemplatesMatchCheck(string firstTemplate, string secondTemplate, bool isIri)
        {
            var firstTemplateParts = this.templateProcessor.ParseTemplate(firstTemplate).ToArray();
            var secondTemplateParts = this.templateProcessor.ParseTemplate(secondTemplate).ToArray();

            return TemplateMatchCheck(string.Empty, string.Empty, 0, firstTemplateParts.Length, firstTemplateParts, string.Empty, string.Empty, 0, secondTemplateParts.Length, secondTemplateParts, isIri);
        }

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

