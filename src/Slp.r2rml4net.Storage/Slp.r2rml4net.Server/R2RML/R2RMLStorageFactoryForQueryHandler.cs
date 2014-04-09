using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Slp.r2rml4net.Storage;
using VDS.RDF.Configuration;

namespace Slp.r2rml4net.Server.R2RML
{
    public class R2RMLStorageFactoryForQueryHandler : IObjectFactory
    {
        public bool CanLoadObject(Type t)
        {
            if (t == typeof(R2RMLStorage))
                return true;
            else
                return false;
        }

        public bool TryLoadObject(VDS.RDF.IGraph g, VDS.RDF.INode objNode, Type targetType, out object obj)
        {
            obj = StorageWrapper.Storage;
            return true;
        }
    }
}