using System;
using System.Collections.Generic;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Represents a mapping of a set of triples with shared <see cref="ISubjectMapping"/> and data source.
    /// </summary>
    public interface ITriplesMapping
    {
        /// <summary>
        /// The subject mapping
        /// </summary>
        ISubjectMapping SubjectMap { get; }

        /// <summary>
        /// The set of predicate object mappings
        /// </summary>
        IEnumerable<IPredicateObjectMapping> PredicateObjectMaps { get; }

        /// <summary>
        /// The table name
        /// </summary>
        /// <remarks>
        /// Either <see cref="TableName"/> or <see cref="SqlStatement"/> should be set.
        /// </remarks>
        string TableName { get; }

        /// <summary>
        /// The sql statement to retrieve data
        /// </summary>
        /// <remarks>
        /// Either <see cref="TableName"/> or <see cref="SqlStatement"/> should be set.
        /// </remarks>
        string SqlStatement { get; }
    }

    /// <summary>
    /// Represents a predicate object mapping
    /// </summary>
    public interface IPredicateObjectMapping
    {
        /// <summary>
        /// The graph mappings
        /// </summary>
        IEnumerable<IGraphMapping> GraphMaps { get; }

        /// <summary>
        /// The predicate mappings
        /// </summary>
        IEnumerable<IPredicateMapping> PredicateMaps { get; }

        /// <summary>
        /// The object mappings
        /// </summary>
        IEnumerable<IObjectMapping> ObjectMaps { get; }

        /// <summary>
        /// The ref-object mappings
        /// </summary>
        IEnumerable<IRefObjectMapping> RefObjectMaps { get; }
    }

    /// <summary>
    /// Base abstraction for a mapping
    /// </summary>
    public interface IBaseMapping
    {
        /// <summary>
        /// A triples mapping which contains this mapping
        /// </summary>
        ITriplesMapping TriplesMap { get; }

        /// <summary>
        /// The term type for this mapping
        /// </summary>
        ITermTypeInformation TermType { get; }
    }

    /// <summary>
    /// An information about type
    /// </summary>
    public interface ITermTypeInformation
    {
        /// <summary>
        /// Gets whether the term is a blank node
        /// </summary>
        bool IsBlankNode { get; }

        /// <summary>
        /// Gets whether the term is an IRI
        /// </summary>
        bool IsIri { get; }

        /// <summary>
        /// Gets whether the term is a literal
        /// </summary>
        bool IsLiteral { get; }

        /// <summary>
        /// Gets the data-type IRI (if it is a literal)
        /// </summary>
        Uri DataTypeIri { get; }

        /// <summary>
        /// Gets the language (it it is a literal)
        /// </summary>
        string Language { get; }
    }

    /// <summary>
    /// Base interface for all direct term mappings (all except <see cref="IRefObjectMapping"/>).
    /// </summary>
    public interface ITermMapping
        : IBaseMapping
    {
        /// <summary>
        /// Determines whether the mapped value is a constant
        /// </summary>
        bool IsConstantValued { get; }

        /// <summary>
        /// Determines whether the value is retrieved from a column
        /// </summary>
        bool IsColumnValued { get; }

        /// <summary>
        /// Determines whether the mapped value is templated
        /// </summary>
        bool IsTemplateValued { get; }

        /// <summary>
        /// Gets the column name (in case that the mapping <see cref="IsColumnValued"/> is <c>true</c>).
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// Gets the template (in case that the mapping <see cref="IsTemplateValued"/> is <c>true</c>).
        /// </summary>
        string Template { get; }

        /// <summary>
        /// Gets the base IRI for value creation
        /// </summary>
        Uri BaseIri { get; }

        /// <summary>
        /// Gets the IRI (in case that the mapping <see cref="IsConstantValued"/> is <c>true</c>).
        /// </summary>
        /// <remarks>
        /// The IRI is available on this level as all mappings may contain an IRI. If it is a literal
        /// constant, it has to be a part of <see cref="IObjectMapping"/>.
        /// </remarks>
        Uri Iri { get; }
    }

    /// <summary>
    /// Represents a ref-object mapping
    /// </summary>
    public interface IRefObjectMapping
        : IBaseMapping
    {
        /// <summary>
        /// The join conditions with the target
        /// </summary>
        IEnumerable<IJoinCondition> JoinConditions { get; }

        /// <summary>
        /// The target subject map
        /// </summary>
        ISubjectMapping TargetSubjectMap { get; }
    }

    /// <summary>
    /// Represents a join condition for <see cref="IRefObjectMapping"/>.
    /// </summary>
    public interface IJoinCondition
    {
        /// <summary>
        /// The child column
        /// </summary>
        string ChildColumn { get; }

        /// <summary>
        /// The target (the one in joined source) column
        /// </summary>
        string TargetColumn { get; }
    }

    /// <summary>
    /// Represents the object mapping
    /// </summary>
    public interface IObjectMapping
        : ITermMapping
    {
        /// <summary>
        /// Gets the literal (in case that the mapping <see cref="ITermMapping.IsConstantValued"/> is <c>true</c>).
        /// </summary>
        /// <remarks>
        /// The literal is available if it is a literal constant, otherwise check <see cref="ITermMapping.Iri"/>.
        /// </remarks>
        ParsedLiteralParts Literal { get; }
    }

    /// <summary>
    /// Represents the predicate mapping
    /// </summary>
    public interface IPredicateMapping
        : ITermMapping
    {

    }

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

    /// <summary>
    /// Represents the graph mapping
    /// </summary>
    public interface IGraphMapping
        : ITermMapping
    {

    }
}
