using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Query.Conditions
{
    /// <summary>
    /// Visitor for conditions
    /// </summary>
    public interface IConditionVisitor 
        : IFilterConditionVisitor, IAssignmentConditionVisitor, ISourceConditionVisitor
    {
        
    }

    /// <summary>
    /// Visitor for assignment conditions
    /// </summary>
    public interface IAssignmentConditionVisitor
        : IVisitor
    {
        
    }

    /// <summary>
    /// Visitor for source conditions
    /// </summary>
    public interface ISourceConditionVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="TupleFromSourceCondition"/>
        /// </summary>
        /// <param name="tupleFromSourceCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(TupleFromSourceCondition tupleFromSourceCondition, object data);
    }

    /// <summary>
    /// Visitor for filter conditions
    /// </summary>
    public interface IFilterConditionVisitor : IVisitor
    {
        /// <summary>
        /// Visits <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="alwaysFalseCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(AlwaysFalseCondition alwaysFalseCondition, object data);

        /// <summary>
        /// Visits <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="alwaysTrueCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(AlwaysTrueCondition alwaysTrueCondition, object data);

        /// <summary>
        /// Visits <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="conjunctionCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(ConjunctionCondition conjunctionCondition, object data);

        /// <summary>
        /// Visits <see cref="DisjunctionCondition"/>
        /// </summary>
        /// <param name="disjunctionCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(DisjunctionCondition disjunctionCondition, object data);

        /// <summary>
        /// Visits <see cref="EqualExpressionCondition"/>
        /// </summary>
        /// <param name="equalExpressionCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(EqualExpressionCondition equalExpressionCondition, object data);

        /// <summary>
        /// Visits <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="equalVariablesCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(EqualVariablesCondition equalVariablesCondition, object data);

        /// <summary>
        /// Visits <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="isNullCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(IsNullCondition isNullCondition, object data);

        /// <summary>
        /// Visits <see cref="NegationCondition"/>
        /// </summary>
        /// <param name="negationCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(NegationCondition negationCondition, object data);
    }
}
