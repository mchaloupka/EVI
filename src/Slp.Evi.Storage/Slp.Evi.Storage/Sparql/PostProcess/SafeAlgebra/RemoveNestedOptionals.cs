using System;
using System.Collections.Generic;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Utils;
using VDS.RDF.Nodes;

namespace Slp.Evi.Storage.Sparql.PostProcess.SafeAlgebra
{
    /// <summary>
    /// Class providing the ability to remove nested optionals. It is called after 
    /// <see cref="AscendFilterPattern"/> and <see cref="AscendExtendPattern"/>  so
    /// we have already removed lots of scenarios.
    /// </summary>
    public class RemoveNestedOptionals
        : BaseSparqlTransformer<SafeAlgebraParameter>, ISparqlPostProcess
    {
        /// <summary>
        /// Optimizes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        public ISparqlQuery Process(ISparqlQuery query, QueryContext context)
        {
            return TransformSparqlQuery(query, new SafeAlgebraParameter(context, false));
        }

        /// <summary>
        /// Process the <see cref="GraphPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(GraphPattern toTransform, SafeAlgebraParameter data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Process the <see cref="LeftJoinPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(LeftJoinPattern toTransform, SafeAlgebraParameter data)
        {
            if (data.IsNestedInLeftJoin)
            {
                return toTransform;
            }
            else
            {
                var newLeft = TransformGraphPattern(toTransform.LeftOperand, data.Create(false));
                var newRight = TransformGraphPattern(toTransform.RightOperand, data.Create(true));

                if (newRight is LeftJoinPattern)
                {
                    var rightLeftJoin = (LeftJoinPattern) newRight;

                    var newVariable = data.Context.CreateSparqlVariable();
                    
                    var leftRight = new ExtendPattern(rightLeftJoin.LeftOperand, newVariable, new NodeExpression(new LongNode(null, 1)));
                    var leftLeft = newLeft;

                    var left = new LeftJoinPattern(leftLeft, leftRight, toTransform.Condition);
                    var right = rightLeftJoin.RightOperand;

                    var condition = new ConjunctionExpression(new ISparqlCondition[]
                    {
                        rightLeftJoin.Condition,
                        new IsBoundExpression(newVariable)
                    });

                    var result = new LeftJoinPattern(left, right, condition);
                    return TransformGraphPattern(result, data);
                }
                else if (newLeft != toTransform.LeftOperand || newRight != toTransform.RightOperand)
                {
                    return new LeftJoinPattern(newLeft, newRight, toTransform.Condition);
                }
                else
                {
                    return toTransform;
                }
            }
        }

        /// <summary>
        /// Process the <see cref="MinusPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(MinusPattern toTransform, SafeAlgebraParameter data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Process the <see cref="UnionPattern"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(UnionPattern toTransform, SafeAlgebraParameter data)
        {
            return base.Transform(toTransform, data.Create(false));
        }
    }
}
