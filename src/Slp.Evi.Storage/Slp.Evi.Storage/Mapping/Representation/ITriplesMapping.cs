using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation
{
    public interface ITriplesMapping
    {
        ISubjectMapping SubjectMap { get; }
        IEnumerable<IPredicateObjectMapping> PredicateObjectMaps { get; }
        Uri BaseUri { get; }
    }

    public interface IPredicateObjectMapping
    {
        IEnumerable<IGraphMapping> GraphMaps { get; }
        IEnumerable<IPredicateMapping> PredicateMaps { get; }
        IEnumerable<IObjectMapping> ObjectMaps { get; }
        IEnumerable<IRefObjectMapping> RefObjectMaps { get; }
    }

    public interface IRefObjectMapping
    {
    }

    public interface IObjectMapping
    {
    }

    public interface IPredicateMapping
    {
    }

    public interface ISubjectMapping
    {
        IEnumerable<IGraphMapping> GraphMaps { get; }
        IEnumerable<Uri> Classes { get; }
    }

    public interface IGraphMapping
    {
    }
}
