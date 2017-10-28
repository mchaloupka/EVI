using System;
using System.Collections.Generic;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.DBSchema;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net.Mapping;

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
        private readonly CacheDictionary<IMapBase, IValueType> _typesDictionary;

        /// <summary>
        /// The type to index dictionary
        /// </summary>
        private readonly Dictionary<int, IValueType> _typesIndexDictionary;

        /// <summary>
        /// The type to index dictionary
        /// </summary>
        private readonly Dictionary<IValueType, int> _typeIndexesDictionary;

        /// <summary>
        /// The created full types indexes
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, int>> _createdFullTypesIndexes;

        /// <summary>
        /// The created language type only indexes
        /// </summary>
        private readonly Dictionary<string, int> _createdLangOnlyIndexes;
        /// <summary>
        /// The created datatype type only indexes
        /// </summary>
        private readonly Dictionary<string, int> _createdTypeOnlyIndexes;

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
            _typesDictionary = new CacheDictionary<IMapBase, IValueType>(ResolveType);
            _typesIndexDictionary = new Dictionary<int, IValueType>();
            _typeIndexesDictionary = new Dictionary<IValueType, int>();
            _createdFullTypesIndexes = new Dictionary<string, Dictionary<string, int>>();
            _createdLangOnlyIndexes = new Dictionary<string, int>();
            _createdTypeOnlyIndexes = new Dictionary<string, int>();

            var iriType = new IRIValueType();
            var blankNodeType = new BlankValueType();
            var noLangNoTypeLiteral = new LiteralValueType(null, null);

            _typesIndexDictionary.Add(1, iriType);
            _typeIndexesDictionary.Add(iriType, 1);

            _typesIndexDictionary.Add(2, blankNodeType);
            _typeIndexesDictionary.Add(blankNodeType, 2);

            _typesIndexDictionary.Add(3, noLangNoTypeLiteral);
            _typeIndexesDictionary.Add(noLangNoTypeLiteral, 3);

            _index = 4;
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
        public IValueType GetValueType(IMapBase termMap) => _typesDictionary.GetValueFor(termMap);

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
        private IValueType ResolveType(IMapBase map)
        {
            if (map is ITermMap termMap)
            {
                if ((termMap is ISubjectMap) || (termMap is IPredicateMap))
                {
                    if (termMap.TermType.IsBlankNode)
                    {
                        return _typesIndexDictionary[2];
                    }
                    else
                    {
                        return _typesIndexDictionary[1];
                    }
                }
                else if (termMap is IObjectMap objectMap)
                {
                    if (termMap.TermType.IsBlankNode)
                    {
                        return _typesIndexDictionary[2];
                    }
                    else if (termMap.TermType.IsURI)
                    {
                        return _typesIndexDictionary[1];
                    }
                    else if (termMap.TermType.IsLiteral)
                    {
                        return ResolveLiteralType(objectMap);
                    }
                }
            }
            else if (map is IRefObjectMap refObjectMap)
            {
                return ResolveType(refObjectMap.ParentTriplesMap.SubjectMap);
            }

            throw new NotSupportedException("Unsupported ITermMap type");
        }

        /// <summary>
        /// Resolves the type for <paramref name="objectMap"/>.
        /// </summary>
        private IValueType ResolveLiteralType(IObjectMap objectMap)
        {
            string languageTag = null;
            Uri dataType = null;

            dataType = objectMap.DataTypeURI;
            languageTag = objectMap.Language;

            if (dataType == null && languageTag == null)
            {
                if (objectMap.IsConstantValued)
                {
                    if (objectMap.Literal != null)
                    {
                        var literal = objectMap.Parsed();

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
                    var triplesMap = objectMap.GetTriplesMapConfiguration();

                    if (triplesMap.TableName != null)
                    {
                        var tableInfo = _dbSchemaProvider.GetTableInfo(triplesMap.TableName);

                        if (tableInfo == null)
                        {
                            throw new Exception($"Table {triplesMap.TableName} not found");
                        }

                        var column = tableInfo.FindColumn(columnName);

                        if (column == null)
                        {
                            throw new Exception($"Column {columnName} not found in table {triplesMap.TableName}");
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
                int foundIndex;
                if (!_createdLangOnlyIndexes.TryGetValue(languageTag, out foundIndex))
                {
                    var type = new LiteralValueType(null, languageTag);
                    _typesIndexDictionary.Add(_index, type);
                    _typeIndexesDictionary.Add(type, _index);
                    _createdLangOnlyIndexes.Add(languageTag, _index);
                    _index++;
                    return type;
                }
                else
                {
                    return _typesIndexDictionary[foundIndex];
                }
            }
            else
            {
                var dataTypeFullUri = dataType.ToCompleteUri();
                var dictionary = _createdTypeOnlyIndexes;

                if (languageTag != null)
                {
                    if (!_createdFullTypesIndexes.ContainsKey(languageTag))
                    {
                        _createdFullTypesIndexes.Add(languageTag, new Dictionary<string, int>());
                    }

                    dictionary = _createdFullTypesIndexes[languageTag];
                }

                int foundIndex;
                if (!dictionary.TryGetValue(dataTypeFullUri, out foundIndex))
                {
                    var type = new LiteralValueType(dataType, languageTag);
                    _typesIndexDictionary.Add(_index, type);
                    _typeIndexesDictionary.Add(type, _index);
                    dictionary.Add(dataTypeFullUri, _index);
                    _index++;
                    return type;
                }
                else
                {
                    return _typesIndexDictionary[foundIndex];
                }
            }
        }
    }
}
