using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.DBSchema;
using Slp.Evi.Storage.Mapping.Representation;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Types
{
    /// <summary>
    /// The type cache.
    /// </summary>
    public class TypeCache : ITypeCache
    {
        /// <summary>
        /// The database schema provider
        /// </summary>
        private readonly IDbSchemaProvider _dbSchemaProvider;

        /// <summary>
        /// The database
        /// </summary>
        private readonly ISqlDatabase _database;

        /// <summary>
        /// The types dictionary
        /// </summary>
        private readonly CacheDictionary<IBaseMapping, IValueType> _typesDictionary;

        /// <summary>
        /// The type to index dictionary
        /// </summary>
        private readonly ConcurrentDictionary<int, IValueType> _typesIndexDictionary;

        /// <summary>
        /// The type to index dictionary
        /// </summary>
        private readonly ConcurrentDictionary<IValueType, int> _typeIndexesDictionary;

        /// <summary>
        /// The created full types indexes
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _createdFullTypesIndexes;

        /// <summary>
        /// The created language type only indexes
        /// </summary>
        private readonly ConcurrentDictionary<string, int> _createdLangOnlyIndexes;
        /// <summary>
        /// The created datatype type only indexes
        /// </summary>
        private readonly ConcurrentDictionary<string, int> _createdTypeOnlyIndexes;

        /// <summary>
        /// The next index
        /// </summary>
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeCache"/> class.
        /// </summary>
        public TypeCache(IDbSchemaProvider dbSchemaProvider, ISqlDatabase database)
        {
            _dbSchemaProvider = dbSchemaProvider;
            _database = database;
            _typesDictionary = new CacheDictionary<IBaseMapping, IValueType>(ResolveType);
            _createdFullTypesIndexes = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();
            _createdLangOnlyIndexes = new ConcurrentDictionary<string, int>();
            _createdTypeOnlyIndexes = new ConcurrentDictionary<string, int>();

            var iriType = new IRIValueType();
            var blankNodeType = new BlankValueType();
            var noLangNoTypeLiteral = new LiteralValueType(null, null);

            _typesIndexDictionary = new ConcurrentDictionary<int, IValueType>(new[]
            {
                new KeyValuePair<int, IValueType>(1, iriType),
                new KeyValuePair<int, IValueType>(2, blankNodeType),
                new KeyValuePair<int, IValueType>(3, noLangNoTypeLiteral)
            });

            _typeIndexesDictionary = new ConcurrentDictionary<IValueType, int>(new[]
            {
                new KeyValuePair<IValueType, int>(iriType, 1),
                new KeyValuePair<IValueType, int>(blankNodeType, 2),
                new KeyValuePair<IValueType, int>(noLangNoTypeLiteral, 3)
            });

            _index = 3;
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        public int GetIndex(IValueType valueType) => _typeIndexesDictionary[valueType];

        /// <summary>
        /// Gets the <see cref="IValueType"/> with <paramref name="index"/>.
        /// </summary>
        public IValueType GetValueType(int index) => _typesIndexDictionary[index];

        /// <summary>
        /// Gets the <see cref="IValueType"/> for <paramref name="termMap"/>
        /// </summary>
        public IValueType GetValueType(IBaseMapping termMap) => _typesDictionary.GetValueFor(termMap);

        /// <inheritdoc />
        public IValueType IRIValueType => _typesIndexDictionary[1];

        /// <inheritdoc />
        public IValueType SimpleLiteralValueType => _typesIndexDictionary[3];

        /// <inheritdoc />
        public IValueType GetValueTypeForLanguage(string language) => GetValueType(language, null);

        /// <inheritdoc />
        public IValueType GetValueTypeForDataType(Uri dataTypeUri) => GetValueType(null, dataTypeUri);

        /// <summary>
        /// Resolves the type for <paramref name="map"/>.
        /// </summary>
        private IValueType ResolveType(IBaseMapping map)
        {
            if (map is ITermMapping termMap)
            {
                if (termMap.TermType.IsBlankNode)
                {
                    return _typesIndexDictionary[2];
                }
                else if (termMap.TermType.IsIri)
                {
                    return _typesIndexDictionary[1];
                }
                else if (termMap.TermType.IsLiteral)
                {
                    return ResolveLiteralType((IObjectMapping)termMap);
                }
            }
            else if (map is IRefObjectMapping refObjectMap)
            {
                return ResolveType(refObjectMap.TargetSubjectMap);
            }

            throw new NotSupportedException("Unsupported ITermMap type");
        }

        /// <summary>
        /// Resolves the type for <paramref name="objectMap"/>.
        /// </summary>
        private IValueType ResolveLiteralType(IObjectMapping objectMap)
        {
            string languageTag = null;
            Uri dataType = null;

            dataType = objectMap.DataTypeIri;
            languageTag = objectMap.Language;

            if (dataType == null && languageTag == null)
            {
                if (objectMap.IsConstantValued)
                {
                    if (objectMap.Literal != null)
                    {
                        var literal = objectMap.Literal;
                        languageTag = literal.LanguageTag;
                        dataType = literal.Type;
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported literal object map constant value");
                    }
                }
                else if (objectMap.IsColumnValued)
                {
                    var columnName = objectMap.ColumnName;
                    var triplesMap = objectMap.TriplesMap;
                    var tableName = triplesMap.TableName;

                    if (tableName != null)
                    {
                        var tableInfo = _dbSchemaProvider.GetTableInfo(tableName);

                        if (tableInfo == null)
                        {
                            throw new Exception($"Table {tableName} not found");
                        }

                        var column = tableInfo.FindColumn(columnName);

                        if (column == null)
                        {
                            throw new Exception($"Column {columnName} not found in table {tableName}");
                        }

                        dataType = _database.GetNaturalRdfType(column.DbDataType);
                    }
                    else
                    {
                        throw new NotImplementedException("Only table is supported as source in triples map");
                    }
                }
            }

            return GetValueType(languageTag, dataType);
        }

        private IValueType GetValueType(string languageTag, Uri dataType)
        {
            if (dataType == null && languageTag == null)
            {
                return _typesIndexDictionary[3];
            }
            else if (dataType == null)
            {
                var foundIndex = _createdLangOnlyIndexes.GetOrAdd(languageTag, key =>
                {
                    var newIndex = Interlocked.Increment(ref _index);
                    var type = new LiteralValueType(null, languageTag);
                    _typesIndexDictionary.TryAdd(newIndex, type);
                    _typeIndexesDictionary.TryAdd(type, newIndex);
                    return newIndex;
                });

                return _typesIndexDictionary[foundIndex];
            }
            else
            {
                var dataTypeFullUri = dataType.AbsoluteUri;
                var dictionary = _createdTypeOnlyIndexes;

                if (languageTag != null)
                {
                    dictionary = _createdFullTypesIndexes.GetOrAdd(languageTag, key => new ConcurrentDictionary<string, int>());
                }

                var foundIndex = dictionary.GetOrAdd(dataTypeFullUri, key =>
                {
                    var newIndex = Interlocked.Increment(ref _index);
                    var type = new LiteralValueType(dataType, languageTag);
                    _typesIndexDictionary.TryAdd(newIndex, type);
                    _typeIndexesDictionary.TryAdd(type, newIndex);
                    return newIndex;
                });

                return _typesIndexDictionary[foundIndex];
            }
        }
    }
}
