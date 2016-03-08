using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Assignment
{
    /// <summary>
    /// Visitor for assignment conditions
    /// </summary>
    public interface IAssignmentConditionVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="AssignmentFromExpressionCondition"/>
        /// </summary>
        /// <param name="assignmentFromExpressionCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(AssignmentFromExpressionCondition assignmentFromExpressionCondition, object data);
    }
}
