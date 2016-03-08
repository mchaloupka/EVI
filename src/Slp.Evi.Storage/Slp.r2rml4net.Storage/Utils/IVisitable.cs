namespace Slp.r2rml4net.Storage.Utils
{
    /// <summary>
    /// Visitable object for visitor pattern
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IVisitable<in T> where T : IVisitor
    {
        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        object Accept(T visitor, object data);
    }
}
