using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Triples map
    /// </summary>
    /// <seealso cref="ITriplesMapping" />
    public class TriplesMapping
        : ITriplesMapping
    {
        private TriplesMapping()
        { }

        /// <inheritdoc />
        public IEnumerable<IPredicateObjectMap> PredicateObjectMaps { get; private set; }

        /// <inheritdoc />
        public ISubjectMap SubjectMap { get; private set; }

        /// <inheritdoc />
        public string TableName { get; private set; }

        /// <inheritdoc />
        public string SqlQuery { get; private set; }

        /// <inheritdoc />
        public Uri[] SqlVersions { get; private set; }

        public static ITriplesMapping Create(ITriplesMap triplesMap, RepresentationCreationContext creationContext)
        {
            var res = new TriplesMapping();
            res.PredicateObjectMaps = triplesMap.PredicateObjectMaps
                .Select(x => PredicateObjectMapping.Create(x, res, creationContext)).ToArray();
            res.SubjectMap = SubjectMapping.Create(triplesMap.SubjectMap, res, creationContext);
            res.TableName = triplesMap.TableName;
            res.SqlQuery = triplesMap.SqlQuery;
            res.SqlVersions = triplesMap.SqlVersions;
            return res;
        }
    }

    public class SubjectMapping
        : ISubjectMap
    {
        private SubjectMapping() { }

        public static ISubjectMap Create(ISubjectMap subjectMap, TriplesMapping context,
            RepresentationCreationContext creationContext)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public INode Node { get; private set; }

        /// <inheritdoc />
        public Uri BaseUri { get; private set; }

        /// <inheritdoc />
        public ITriplesMap TriplesMap { get; private set; }

        /// <inheritdoc />
        public string Template { get; private set; }

        /// <inheritdoc />
        public Uri TermTypeURI { get; private set; }

        /// <inheritdoc />
        public string ColumnName { get; private set; }

        /// <inheritdoc />
        public string InverseExpression { get; private set; }

        /// <inheritdoc />
        public bool IsConstantValued { get; private set; }

        /// <inheritdoc />
        public bool IsColumnValued { get; private set; }

        /// <inheritdoc />
        public bool IsTemplateValued { get; private set; }

        /// <inheritdoc />
        public ITermType TermType { get; private set; }

        /// <inheritdoc />
        public Uri URI { get; private set; }

        /// <inheritdoc />
        public IEnumerable<IGraphMap> GraphMaps { get; private set; }

        /// <inheritdoc />
        public Uri[] Classes { get; private set; }
    }

    public class GraphMapping
        : IGraphMap
    {
        private GraphMapping() { }

        public static IGraphMap Create(IGraphMap graphMap, TriplesMapping context, TriplesMapping triplesMapping)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public INode Node { get; }

        /// <inheritdoc />
        public Uri BaseUri { get; }

        /// <inheritdoc />
        public ITriplesMap TriplesMap { get; }

        /// <inheritdoc />
        public string Template { get; }

        /// <inheritdoc />
        public Uri TermTypeURI { get; }

        /// <inheritdoc />
        public string ColumnName { get; }

        /// <inheritdoc />
        public string InverseExpression { get; }

        /// <inheritdoc />
        public bool IsConstantValued { get; }

        /// <inheritdoc />
        public bool IsColumnValued { get; }

        /// <inheritdoc />
        public bool IsTemplateValued { get; }

        /// <inheritdoc />
        public ITermType TermType { get; }

        /// <inheritdoc />
        public Uri URI { get; }
    }

    public class PredicateObjectMapping
    {
        public static IPredicateObjectMap Create(IPredicateObjectMap predicateObjectMap, TriplesMapping context,
            RepresentationCreationContext creationContext)
        {
            throw new NotImplementedException();
        }
    }

    public class TermTypeRepresentation
    {
        public static ITermType Create(ITermType subjectMapTermType, TriplesMapping context)
        {
            throw new NotImplementedException();
        }
    }
}

