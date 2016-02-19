using System;
using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Database.Base;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Assignment;
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

        private readonly Dictionary<ISourceCondition, SourceConditionSourceSpecificData> _sourceConditionSpecificData;

        private SourceConditionSourceSpecificData _sourceConditionSourceSpecificDataNullData;

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
            _sourceConditionSpecificData = new Dictionary<ISourceCondition, SourceConditionSourceSpecificData>();
            _sourceConditionSourceSpecificDataNullData = null;
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
        public string GetVariableName(ISourceCondition sourceCondition, ICalculusVariable variable)
        {
            if (sourceCondition != null)
            {
                if (!_sourceConditionSpecificData.ContainsKey(sourceCondition))
                {
                    _sourceConditionSpecificData.Add(sourceCondition, new SourceConditionSourceSpecificData(sourceCondition));
                }

                return _sourceConditionSpecificData[sourceCondition].GetVariableName(variable);
            }
            else
            {
                if (_sourceConditionSourceSpecificDataNullData == null)
                {
                    _sourceConditionSourceSpecificDataNullData = new SourceConditionSourceSpecificData(null);
                }

                return _sourceConditionSourceSpecificDataNullData.GetVariableName(variable);
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
        /// <param name="calculusModel">The calculus model.</param>
        /// <param name="assignmentCondition">The assignment condition.</param>
        public void AddAssignmentCondition(CalculusModel calculusModel, IAssignmentCondition assignmentCondition)
        {
            if (!_calculusModelSpecificDatas.ContainsKey(calculusModel))
            {
                _calculusModelSpecificDatas.Add(calculusModel, new CalculusModelSpecificData());
            }

            assignmentCondition.Accept(new AddAssignmentCondition_Visitor(), _calculusModelSpecificDatas[calculusModel]);
        }

        /// <summary>
        /// Gets the tuple from source condtion.
        /// </summary>
        /// <param name="parentModel">The parent model.</param>
        /// <param name="source">The source we are looking for.</param>
        public ISourceCondition GetSourceCondtion(CalculusModel parentModel, ICalculusSource source)
        {
            var data = _calculusModelSpecificDatas[parentModel];
            return data.GetSourceCondition(source);
        }

        private class SourceConditionSourceSpecificData
            : ISourceConditionVisitor
        {
            private readonly Dictionary<ICalculusVariable, string> _columnNames;

            private readonly bool _canGenerateNewNames;
            private int _counter;

            public SourceConditionSourceSpecificData(ISourceCondition sourceCondition)
            {
                _columnNames = new Dictionary<ICalculusVariable, string>();
                _canGenerateNewNames = false;

                _counter = 1;

                if (sourceCondition == null)
                {
                    _canGenerateNewNames = true;
                }
                else
                {
                    sourceCondition.Accept(this, null);
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

            public object Visit(TupleFromSourceCondition tupleFromSourceCondition, object data)
            {
                if (tupleFromSourceCondition.Source is SqlTable)
                {
                    foreach (var column in tupleFromSourceCondition.CalculusVariables.Cast<SqlColumn>())
                    {
                        _columnNames.Add(column, column.Name);
                    }
                }
                else
                {
                    foreach (var column in tupleFromSourceCondition.CalculusVariables)
                    {
                        _columnNames.Add(column, string.Format("c{0}", _counter++));
                    }
                }

                return null;
            }

            public object Visit(UnionedSourcesCondition unionedSourcesCondition, object data)
            {
                foreach (var column in unionedSourcesCondition.CalculusVariables)
                {
                    _columnNames.Add(column, string.Format("c{0}", _counter++));
                }

                return null;
            }
        }

        private class CalculusModelSpecificData
        {
            public CalculusModelSpecificData()
            {
                _variableConditions = new Dictionary<ICalculusVariable, ICondition>();
                _sourceConditions = new Dictionary<ICalculusSource, ISourceCondition>();
            }

            private readonly Dictionary<ICalculusVariable, ICondition> _variableConditions;

            private readonly Dictionary<ICalculusSource, ISourceCondition> _sourceConditions;

            public void AddData(TupleFromSourceCondition tupleFromSourceCondition)
            {
                foreach (var variable in tupleFromSourceCondition.CalculusVariables)
                {
                    _variableConditions.Add(variable, tupleFromSourceCondition);
                }

                _sourceConditions.Add(tupleFromSourceCondition.Source, tupleFromSourceCondition);
            }

            public ICondition GetSourceOfVariable(ICalculusVariable variable)
            {
                if (_variableConditions.ContainsKey(variable))
                {
                    return _variableConditions[variable];
                }
                else
                {
                    return null;
                }
            }

            public ISourceCondition GetSourceCondition(ICalculusSource calculusSource)
            {
                return _sourceConditions[calculusSource];
            }

            public void AddData(UnionedSourcesCondition unionedSourcesCondition)
            {
                foreach (var variable in unionedSourcesCondition.CalculusVariables)
                {
                    _variableConditions.Add(variable, unionedSourcesCondition);
                }

                foreach (var calculusSource in unionedSourcesCondition.Sources)
                {
                    _sourceConditions.Add(calculusSource, unionedSourcesCondition);
                }
            }

            public void AddData(AssignmentFromExpressionCondition assignmentFromExpressionCondition)
            {
                _variableConditions.Add(assignmentFromExpressionCondition.Variable, assignmentFromExpressionCondition);
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

        private class AddAssignmentCondition_Visitor
            : IAssignmentConditionVisitor
        {
            public object Visit(AssignmentFromExpressionCondition assignmentFromExpressionCondition, object data)
            {
                ((CalculusModelSpecificData)data).AddData(assignmentFromExpressionCondition);
                return null;
            }
        }
    }
}