using System;
using System.Linq;
using Slp.Evi.Storage.Common.Optimization.PatternMatching;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Utils.CodeGeneration;
using Slp.Evi.Storage.Types;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net.Extensions;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Query.Patterns;
using PatternItem = Slp.Evi.Storage.Common.Optimization.PatternMatching.PatternItem;

namespace Slp.Evi.Storage.Sparql.PostProcess.Optimizers
{
    /// <summary>
    /// The triple pattern optimization
    /// </summary>
    public class TriplePatternOptimizer
        : BaseSparqlOptimizer<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriplePatternOptimizer"/> class.
        /// </summary>
        public TriplePatternOptimizer()
            : base(new TriplePatternOptimizerImplementation())
        { }

        /// <summary>
        /// The implementation class for <see cref="TriplePatternOptimizer"/>
        /// </summary>
        public class TriplePatternOptimizerImplementation
            : BaseSparqlOptimizerImplementation<object>
        {
            /// <summary>
            /// The template processor
            /// </summary>
            private readonly TemplateProcessor _templateProcessor;

            /// <summary>
            /// The pattern comparer
            /// </summary>
            private readonly PatternComparer _patternComparer;

            /// <summary>
            /// Initializes a new instance of the <see cref="TriplePatternOptimizerImplementation"/> class.
            /// </summary>
            public TriplePatternOptimizerImplementation()
            {
                _templateProcessor = new TemplateProcessor();
                _patternComparer = new PatternComparer();
            }

            /// <summary>
            /// Process the <see cref="RestrictedTriplePattern"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IGraphPattern Transform(RestrictedTriplePattern toTransform, OptimizationContext data)
            {
                if (!CanObjectMatch(toTransform, data)
                    || !CanPredicateMatch(toTransform, data)
                    || !CanSubjectMatch(toTransform, data))
                {
                    return new NotMatchingPattern();
                }
                return toTransform;
            }

            /// <summary>
            /// Can the subject match the pattern?.
            /// </summary>
            /// <param name="toTransform">To transform.</param>
            /// <param name="data">The data.</param>
            private bool CanSubjectMatch(RestrictedTriplePattern toTransform, OptimizationContext data)
            {
                var pattern = toTransform.SubjectPattern;

                if (pattern is NodeMatchPattern nodeMatchPattern)
                {
                    return CanMatch(nodeMatchPattern.Node, toTransform.SubjectMap, data.Context.TypeCache.GetValueType(toTransform.SubjectMap));
                }
                return true;
            }

            /// <summary>
            /// Can the predicate match the pattern?.
            /// </summary>
            /// <param name="toTransform">To transform.</param>
            /// <param name="data">The data.</param>
            private bool CanPredicateMatch(RestrictedTriplePattern toTransform, OptimizationContext data)
            {
                var pattern = toTransform.PredicatePattern;

                if (pattern is NodeMatchPattern nodeMatchPattern)
                {
                    return CanMatch(nodeMatchPattern.Node, toTransform.PredicateMap, data.Context.TypeCache.GetValueType(toTransform.PredicateMap));
                }
                return true;
            }

            /// <summary>
            /// Can the object match the pattern?.
            /// </summary>
            /// <param name="toTransform">To transform.</param>
            /// <param name="data">The data.</param>
            private bool CanObjectMatch(RestrictedTriplePattern toTransform, OptimizationContext data)
            {
                var pattern = toTransform.ObjectPattern;
                ITermMap r2RmlDef;

                if (toTransform.ObjectMap != null)
                {
                    r2RmlDef = toTransform.ObjectMap;
                }
                else if (toTransform.RefObjectMap != null)
                {
                    var parentTriples = toTransform.RefObjectMap.ParentTriplesMap;
                    r2RmlDef = parentTriples.SubjectMap;
                }
                else
                {
                    throw new Exception("R2RMLObjectMap or R2RMLRefObjectMap must be assigned");
                }

                if (pattern is NodeMatchPattern nodeMatchPattern)
                {
                    return CanMatch(nodeMatchPattern.Node, r2RmlDef, data.Context.TypeCache.GetValueType(r2RmlDef));
                }

                return true;
            }

            /// <summary>
            /// Determines whether the node can match the mapping.
            /// </summary>
            /// <param name="node">The match pattern node.</param>
            /// <param name="termMap">The mapping.</param>
            /// <param name="type">The type of <paramref name="termMap"/></param>
            public bool CanMatch(INode node, ITermMap termMap, IValueType type)
            {
                if (type.Category == TypeCategories.BlankNode)
                {
                    if (node.NodeType != NodeType.Blank)
                    {
                        return false;
                    }
                }
                else if (type.Category == TypeCategories.IRI)
                {
                    if (node.NodeType != NodeType.Uri)
                    {
                        return false;
                    }
                }
                else
                {
                    if (node.NodeType != NodeType.Literal)
                    {
                        return false;
                    }

                    var literalType = (LiteralValueType) type;
                    var literalNode = (ILiteralNode) node;

                    var nodeType = literalNode.DataType;
                    var nodeLang = string.IsNullOrEmpty(literalNode.Language) ? null : literalNode.Language;

                    if (!((literalType.LanguageTag == nodeLang) &&
                        literalType.LiteralType.IsCompleteUriEqualTo(nodeType)))
                    {
                        return false;
                    }
                }

                return CanMatchValue(node, termMap);
            }

            /// <summary>
            /// Determines whether the node can match the mapping according to the value.
            /// </summary>
            /// <param name="node">The match pattern node.</param>
            /// <param name="termMap">The mapping.</param>
            private bool CanMatchValue(INode node, ITermMap termMap)
            {
                if (node.NodeType == NodeType.Uri)
                {
                    if (!termMap.TermType.IsURI)
                    {
                        return false;
                    }
                    var uri = node.GetUri();
                    var pattern = new Pattern(true, new[] { new PatternItem(uri.AbsoluteUri) });
                    return CanMatch(pattern, termMap);
                }
                if (node.NodeType == NodeType.Literal)
                {
                    if (!termMap.TermType.IsLiteral)
                    {
                        return false;
                    }

                    var literalNode = (ILiteralNode) node;
                    var pattern = new Pattern(false, new[] { new PatternItem(literalNode.Value) });
                    return CanMatch(pattern, termMap);
                }
                else
                {
                    return true;
                }
            }

            /// <summary>
            /// Determines whether the pattern can match the mapping.
            /// </summary>
            /// <param name="pattern">The pattern.</param>
            /// <param name="termMap">The term map.</param>
            private bool CanMatch(Pattern pattern, ITermMap termMap)
            {
                bool isIriEscaped = !termMap.TermType.IsLiteral;

                if (termMap.IsColumnValued)
                {
                    var termPattern = new Pattern(isIriEscaped, new[] {new PatternItem()});
                    return CanMatch(pattern, termPattern);
                }
                if (termMap.IsConstantValued)
                {
                    if (termMap is IUriValuedTermMap)
                    {
                        var uriValued = (IUriValuedTermMap)termMap;
                        var termPattern = new Pattern(true, new[] {new PatternItem(uriValued.URI.AbsoluteUri)});
                        return CanMatch(pattern, termPattern);
                    }
                    else if (termMap is IObjectMap objectMap)
                    {
                        if (objectMap.URI != null)
                        {
                            var termPattern = new Pattern(true, new[] {new PatternItem(objectMap.URI.AbsoluteUri)});
                            return CanMatch(pattern, termPattern);
                        }
                        else if (objectMap.Literal != null)
                        {
                            var termPattern = new Pattern(false, new[] {new PatternItem(objectMap.Literal)});
                            return CanMatch(pattern, termPattern);
                        }
                        else
                        {
                            throw new InvalidOperationException("IObjectMap must have URI or Literal assigned.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("ITermMap has to be IUriValuedTermMap or an IObjectMap");
                    }
                }
                else if (termMap.IsTemplateValued)
                {
                    var templateParts = _templateProcessor.ParseTemplate(termMap.Template);
                    var patternItems = templateParts.Select(PatternItem.FromTemplatePart);
                    var termPattern = new Pattern(isIriEscaped, patternItems);
                    return CanMatch(pattern, termPattern);
                }
                else
                {
                    throw new InvalidOperationException("ITermMap must be column, constant or template valued");
                }
            }

            private bool CanMatch(Pattern leftPattern, Pattern rightPattern)
            {
                var result = _patternComparer.Compare(leftPattern, rightPattern);
                return !result.NeverMatch;
            }
        }
    }
}
