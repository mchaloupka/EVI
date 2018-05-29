using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation
{
    public interface ITriplesMapping
    {
        ISubjectMapping SubjectMap { get; }
        IEnumerable<IPredicateObjectMapping> PredicateObjectMaps { get; }
        string TableName { get; }
        string SqlStatement { get; }
    }

    public interface IPredicateObjectMapping
    {
        IEnumerable<IGraphMapping> GraphMaps { get; }
        IEnumerable<IPredicateMapping> PredicateMaps { get; }
        IEnumerable<IObjectMapping> ObjectMaps { get; }
        IEnumerable<IRefObjectMapping> RefObjectMaps { get; }
    }

    public interface IBaseMapping
    {
        ITriplesMapping TriplesMap { get; }
        ITermTypeInformation TermType { get; }
    }

    public interface ITermTypeInformation
    {
        bool IsBlankNode { get; }
        bool IsURI { get; }
        bool IsLiteral { get; }
        Uri DataTypeURI { get; }
        string Language { get; }
    }

    public interface ITermMapping
        : IBaseMapping
    {
        bool IsConstantValued { get; }
        bool IsColumnValued { get; }
        bool IsTemplateValued { get; }
        string ColumnName { get; }
        string Template { get; }
        Uri BaseUri { get; }
        Uri URI { get; }
    }

    public interface IIriValuedTermMapping
        : ITermMapping
    {

    }

    public interface IRefObjectMapping
        : IBaseMapping
    {
        IEnumerable<IJoinCondition> JoinConditions { get; }
        ISubjectMapping SubjectMap { get; }
    }

    public interface IJoinCondition
    {
        string ChildColumn { get; }
        string ParentColumn { get; }
    }

    public interface IObjectMapping
        : ITermMapping
    {
        ParsedLiteralParts Literal { get; }
    }

    public interface IPredicateMapping
        : IIriValuedTermMapping
    {

    }

    public interface ISubjectMapping
        : IIriValuedTermMapping
    {
        IEnumerable<IGraphMapping> GraphMaps { get; }
        IEnumerable<Uri> Classes { get; }
    }

    public interface IGraphMapping
        : IIriValuedTermMapping
    {

    }
}
