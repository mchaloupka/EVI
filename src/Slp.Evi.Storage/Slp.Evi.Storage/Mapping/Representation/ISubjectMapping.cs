using System;
using System.Collections.Generic;

namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Represents the subject mapping
    /// </summary>
    public interface ISubjectMapping
        : ITermMapping
    {
        /// <summary>
        /// The graph mappings
        /// </summary>
        IEnumerable<IGraphMapping> GraphMaps { get; }

        /// <summary>
        /// The classes for this subject mapping
        /// </summary>
        IEnumerable<Uri> Classes { get; }
    }
}