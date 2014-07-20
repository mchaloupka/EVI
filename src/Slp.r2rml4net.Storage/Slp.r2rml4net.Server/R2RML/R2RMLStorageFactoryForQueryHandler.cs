using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Slp.r2rml4net.Storage;
using VDS.RDF.Configuration;

namespace Slp.r2rml4net.Server.R2RML
{
    /// <summary>
    /// Factory for the dotNetRDF config
    /// </summary>
    public class R2RMLStorageFactoryForQueryHandler : IObjectFactory
    {
        /// <summary>
        /// Returns whether this Factory is capable of creating objects of the given type
        /// </summary>
        /// <param name="t">Target Type</param>
        public bool CanLoadObject(Type t)
        {
            if (t == typeof(R2RMLStorage))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Attempts to load an Object of the given type identified by the given Node and returned as the Type that this loader generates
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Created Object</param>
        /// <returns>True if the loader succeeded in creating an Object</returns>
        /// <remarks>The Factory should not throw an error if some required configuration is missing as another factory further down the processing chain may still be able to create the object.  If the factory encounters errors and all the required configuration information is present then that error should be thrown i.e. class instantiation throws an error or a call to load an object that this object requires fails.</remarks>
        public bool TryLoadObject(VDS.RDF.IGraph g, VDS.RDF.INode objNode, Type targetType, out object obj)
        {
            obj = StorageWrapper.Storage;
            return true;
        }
    }
}