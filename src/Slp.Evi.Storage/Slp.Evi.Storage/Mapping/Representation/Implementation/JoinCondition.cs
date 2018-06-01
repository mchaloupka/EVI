namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    public class JoinCondition
        : IJoinCondition
    {
        private JoinCondition() { }

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