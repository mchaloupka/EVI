using System;
using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Database.Base;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Source;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using VDS.RDF.Parsing;

namespace Slp.r2rml4net.Storage.Query
{
    /// <summary>
    /// The query naming helpers.
    /// </summary>
    public class QueryNamingHelpers
    {
        private readonly QueryContext _context;

        private readonly Dictionary<CalculusModel, CalculusModelSpecificData> _calculusModelSpecificDatas;

        private readonly Dictionary<TupleFromSourceCondition, TupleFromSourceSpecificData> _tupleFromSourceSpecificDatas;

        private TupleFromSourceSpecificData _tupleFromSourceSpecificData_nullData;

        private readonly Dictionary<ISourceCondition, string> _sourceConditionNames;

        private int _sourceConditionNameCounter;
         

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryNamingHelpers"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public QueryNamingHelpers(QueryContext context)
        {
            _context = context;
            _calculusModelSpecificDatas = new Dictionary<CalculusModel, CalculusModelSpecificData>();
            _sourceConditionNameCounter = 1;
            _sourceConditionNames = new Dictionary<ISourceCondition, string>();
            _tupleFromSourceSpecificDatas = new Dictionary<TupleFromSourceCondition, TupleFromSourceSpecificData>();
            _tupleFromSourceSpecificData_nullData = null;
        }

        /// <summary>
        /// Gets the source of variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="currentModel">The current model.</param>
        public ICondition GetSourceOfVariable(ICalculusVariable variable, CalculusModel currentModel)
        {
            var specificData = _calculusModelSpecificDatas[currentModel];
            return specificData.GetSourceOfVariable(variable);
        }

        /// <summary>
        /// Gets the name of the source condition.
        /// </summary>
        /// <param name="sourceCondition">The source condition.</param>
        public string GetSourceConditionName(ISourceCondition sourceCondition)
        {
            if (!_sourceConditionNames.ContainsKey(sourceCondition))
            {
                _sourceConditionNames.Add(sourceCondition, string.Format("s{0}", _sourceConditionNameCounter++));
            }

            return _sourceConditionNames[sourceCondition];
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <param name="sourceCondition">The source condition.</param>
        /// <param name="variable">The variable.</param>
        public string GetVariableName(TupleFromSourceCondition sourceCondition, ICalculusVariable variable)
        {
            if (sourceCondition != null)
            {
                if (!_tupleFromSourceSpecificDatas.ContainsKey(sourceCondition))
                {
                    _tupleFromSourceSpecificDatas.Add(sourceCondition, new TupleFromSourceSpecificData(sourceCondition));
                }

                return _tupleFromSourceSpecificDatas[sourceCondition].GetVariableName(variable);
            }
            else
            {
                if (_tupleFromSourceSpecificData_nullData == null)
                {
                    _tupleFromSourceSpecificData_nullData = new TupleFromSourceSpecificData(null);
                }

                return _tupleFromSourceSpecificData_nullData.GetVariableName(variable);
            }
        }

        /// <summary>
        /// Adds the source condition.
        /// </summary>
        /// <param name="calculusModel">The calculus model.</param>
        /// <param name="sourceCondition">The source condition.</param>
        public void AddSourceCondition(CalculusModel calculusModel, ISourceCondition sourceCondition)
        {
            if (!_calculusModelSpecificDatas.ContainsKey(calculusModel))
            {
                _calculusModelSpecificDatas.Add(calculusModel, new CalculusModelSpecificData());
            }

            sourceCondition.Accept(new AddSourceCondition_Visitor(), _calculusModelSpecificDatas[calculusModel]);
        }

        /// <summary>
        /// Adds the assignment condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="assignmentCondition">The assignment condition.</param>
        public void AddAssignmentCondition(CalculusModel condition, IAssignmentCondition assignmentCondition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the tuple from source condtion.
        /// </summary>
        /// <param name="parentModel">The parent model.</param>
        /// <param name="model">The model we are looking for.</param>
        public TupleFromSourceCondition GetTupleFromSourceCondtion(CalculusModel parentModel, CalculusModel model)
        {
            throw new NotImplementedException();
        }

        private class TupleFromSourceSpecificData
        {
            private Dictionary<ICalculusVariable, string> _columnNames;

            private bool _canGenerateNewNames;
            private int _counter;

            public TupleFromSourceSpecificData(TupleFromSourceCondition sourceCondition)
            {
                _columnNames = new Dictionary<ICalculusVariable, string>();
                _canGenerateNewNames = false;

                _counter = 1;

                if (sourceCondition == null)
                {
                    _canGenerateNewNames = true;
                }
                else if (sourceCondition.Source is SqlTable)
                {
                    foreach (var column in sourceCondition.CalculusVariables.Cast<SqlColumn>())
                    {
                        _columnNames.Add(column, column.Name);
                    }
                }
                else
                {
                    foreach (var column in sourceCondition.CalculusVariables)
                    {
                        _columnNames.Add(column, string.Format("c{0}", _counter++));
                    }
                }
            }

            public string GetVariableName(ICalculusVariable variable)
            {
                if (_canGenerateNewNames && !_columnNames.ContainsKey(variable))
                {
                    _columnNames.Add(variable, string.Format("c{0}", _counter++));
                }

                return _columnNames[variable];
            }
        }

        private class CalculusModelSpecificData
        {
            public CalculusModelSpecificData()
            {
                _sourceConditions = new Dictionary<ICalculusVariable, ISourceCondition>();
            }

            private readonly Dictionary<ICalculusVariable, ISourceCondition> _sourceConditions;

            public void AddData(TupleFromSourceCondition tupleFromSourceCondition)
            {
                foreach (var variable in tupleFromSourceCondition.CalculusVariables)
                {
                    _sourceConditions.Add(variable, tupleFromSourceCondition);
                }
            }

            public ISourceCondition GetSourceOfVariable(ICalculusVariable variable)
            {
                return _sourceConditions[variable];
            }

            public void AddData(UnionedSourcesCondition unionedSourcesCondition)
            {
                foreach (var variable in unionedSourcesCondition.CalculusVariables)
                {
                    _sourceConditions.Add(variable, unionedSourcesCondition);
                }
            }
        }

        private class AddSourceCondition_Visitor
            : ISourceConditionVisitor
        {
            public object Visit(TupleFromSourceCondition tupleFromSourceCondition, object data)
            {
                ((CalculusModelSpecificData)data).AddData(tupleFromSourceCondition);
                return null;
            }

            public object Visit(UnionedSourcesCondition unionedSourcesCondition, object data)
            {
                ((CalculusModelSpecificData)data).AddData(unionedSourcesCondition);
                return null;
            }
        }
    }
}