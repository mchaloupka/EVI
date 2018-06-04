namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Implements <see cref="IJoinCondition"/>.
    /// </summary>
    public class JoinCondition
        : IJoinCondition
    {
        /// <summary>
        /// Creates an instance of <see cref="JoinCondition"/>.
        /// </summary>
        private JoinCondition() { }

        /// <summary>
        /// Creates an instance of <see cref="JoinCondition"/> from <see cref="TCode.r2rml4net.Mapping.JoinCondition"/>.
        /// </summary>
        /// <param name="joinCondition"></param>
        /// <returns></returns>
        public static IJoinCondition Create(TCode.r2rml4net.Mapping.JoinCondition joinCondition)
        {
            var res = new JoinCondition
            {
                ChildColumn = joinCondition.ChildColumn,
                TargetColumn = joinCondition.ParentColumn
            };
            return res;
        }

        /// <inheritdoc />
        public string ChildColumn { get; private set; }

        /// <inheritdoc />
        public string TargetColumn { get; private set; }
    }
}