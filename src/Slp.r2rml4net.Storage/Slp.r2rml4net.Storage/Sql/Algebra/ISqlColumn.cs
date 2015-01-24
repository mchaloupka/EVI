namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    /// <summary>
    /// SQL Column
    /// </summary>
    public interface ISqlColumn
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        ISqlSource Source { get; }
    }

    /// <summary>
    /// SQL Column that is really in the database
    /// </summary>
    public interface IOriginalSqlColumn : ISqlColumn
    {
        /// <summary>
        /// Gets the name of the original.
        /// </summary>
        /// <value>The name of the original.</value>
        string OriginalName { get; }
    }
}
