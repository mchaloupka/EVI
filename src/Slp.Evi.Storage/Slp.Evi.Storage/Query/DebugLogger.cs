using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Slp.Evi.Storage.Query.Logging;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Builder;
using VDS.RDF.Query;

namespace Slp.Evi.Storage.Query
{
    /// <summary>
    /// Logging of individual transformations
    /// </summary>
    public class DebugLogger
    {
        readonly Dictionary<object, long> _objectIndexes = new Dictionary<object, long>();
        private long _maxIndex = 0;

        /// <summary>
        /// Logs the transformation.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="original">The original form.</param>
        /// <param name="transformed">The transformed form.</param>
        public void LogTransformation<TFrom, TTo>(ILogger logger, TFrom original, TTo transformed)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            logger.LogDebug(
                $"Transformation:\n{GetObjectIndex(original)}: {GetLoggingRepresentation(original)}\n->\n{GetObjectIndex(transformed)}: {GetLoggingRepresentation(transformed)}");
        }

        private string GetLoggingRepresentation<T>(T obj)
        {
            if (typeof(SparqlQuery).IsAssignableFrom(typeof(T)))
            {
                return GetStringRepresentation(obj as SparqlQuery);
            }
            else if (typeof(ISparqlQuery).IsAssignableFrom(typeof(T)))
            {
                return GetStringRepresentation(obj as ISparqlQuery);
            }
            else
            {
                throw new NotImplementedException($"Unsupported type for logger: {typeof(T)}");
            }
        }

        private string GetStringRepresentation(SparqlQuery sparqlQuery)
        {
            return sparqlQuery.ToString();
        }

        private string GetStringRepresentation(ISparqlQuery sparqlQuery)
        {
            var representation = new SparqlQueryRepresentation();
            var str = representation.GetRepresentation(sparqlQuery);
            return str;
        }

        private long GetObjectIndex(object obj)
        {
            if (!_objectIndexes.ContainsKey(obj))
            {
                _objectIndexes.Add(obj, _maxIndex++);
            }

            return _objectIndexes[obj];
        }
    }
}