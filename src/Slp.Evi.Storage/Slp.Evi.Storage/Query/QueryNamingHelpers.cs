using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;
using Slp.Evi.Storage.Relational.Query.Sources;

namespace Slp.Evi.Storage.Query
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
                _sourceConditionNames.Add(sourceCondition, $"s{_sourceConditionNameCounter++}");
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

        /// <summary>
        /// Specific data for <see cref="ISourceCondition"/>. Generates names for it.
        /// </summary>
        /// <seealso cref="ISourceConditionVisitor" />
        private class SourceConditionSourceSpecificData
            : ISourceConditionVisitor
        {
            /// <summary>
            /// The column names
            /// </summary>
            private readonly Dictionary<ICalculusVariable, string> _columnNames;

            /// <summary>
            /// Determines whether it can generate new names or not
            /// </summary>
            private readonly bool _canGenerateNewNames;

            /// <summary>
            /// The next column counter
            /// </summary>
            private int _counter;

            /// <summary>
            /// Initializes a new instance of the <see cref="SourceConditionSourceSpecificData"/> class.
            /// </summary>
            /// <param name="sourceCondition">The source condition.</param>
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

            /// <summary>
            /// Gets the name of the variable.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public string GetVariableName(ICalculusVariable variable)
            {
                if (_canGenerateNewNames && !_columnNames.ContainsKey(variable))
                {
                    _columnNames.Add(variable, $"c{_counter++}");
                }

                return _columnNames[variable];
            }

            /// <summary>
            /// Visits <see cref="TupleFromSourceCondition"/>
            /// </summary>
            /// <param name="tupleFromSourceCondition">The visited instance</param>
            /// <param name="data">The passed data</param>
            /// <returns>The returned data</returns>
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
                        _columnNames.Add(column, $"c{_counter++}");
                    }
                }

                return null;
            }

            /// <summary>
            /// Visits <see cref="UnionedSourcesCondition"/>
            /// </summary>
            /// <param name="unionedSourcesCondition">The visited instance</param>
            /// <param name="data">The passed data</param>
            /// <returns>The returned data</returns>
            public object Visit(UnionedSourcesCondition unionedSourcesCondition, object data)
            {
                foreach (var column in unionedSourcesCondition.CalculusVariables)
                {
                    _columnNames.Add(column, $"c{_counter++}");
                }

                return null;
            }

            /// <summary>
            /// Visits <see cref="LeftJoinCondition"/>
            /// </summary>
            /// <param name="leftJoinCondition">The visited instance</param>
            /// <param name="data">The passed data</param>
            /// <returns>The returned data</returns>
            public object Visit(LeftJoinCondition leftJoinCondition, object data)
            {
                foreach (var column in leftJoinCondition.CalculusVariables)
                {
                    _columnNames.Add(column, $"c{_counter++}");
                }

                return null;
            }
        }

        /// <summary>
        /// Specific data for <see cref="CalculusModel"/>
        /// </summary>
        private class CalculusModelSpecificData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CalculusModelSpecificData"/> class.
            /// </summary>
            public CalculusModelSpecificData()
            {
                _variableConditions = new Dictionary<ICalculusVariable, ICondition>();
                _sourceConditions = new Dictionary<ICalculusSource, ISourceCondition>();
            }

            /// <summary>
            /// The variable conditions
            /// </summary>
            private readonly Dictionary<ICalculusVariable, ICondition> _variableConditions;

            /// <summary>
            /// The source conditions
            /// </summary>
            private readonly Dictionary<ICalculusSource, ISourceCondition> _sourceConditions;

            /// <summary>
            /// Adds the data.
            /// </summary>
            /// <param name="tupleFromSourceCondition">The tuple from source condition.</param>
            public void AddData(TupleFromSourceCondition tupleFromSourceCondition)
            {
                foreach (var variable in tupleFromSourceCondition.CalculusVariables)
                {
                    _variableConditions.Add(variable, tupleFromSourceCondition);
                }

                _sourceConditions.Add(tupleFromSourceCondition.Source, tupleFromSourceCondition);
            }

            /// <summary>
            /// Gets the source of variable.
            /// </summary>
            /// <param name="variable">The variable.</param>
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

            /// <summary>
            /// Gets the source condition.
            /// </summary>
            /// <param name="calculusSource">The calculus source.</param>
            public ISourceCondition GetSourceCondition(ICalculusSource calculusSource)
            {
                return _sourceConditions[calculusSource];
            }

            /// <summary>
            /// Adds the data.
            /// </summary>
            /// <param name="unionedSourcesCondition">The unioned sources condition.</param>
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

            /// <summary>
            /// Adds the data.
            /// </summary>
            /// <param name="assignmentFromExpressionCondition">The assignment from expression condition.</param>
            public void AddData(AssignmentFromExpressionCondition assignmentFromExpressionCondition)
            {
                _variableConditions.Add(assignmentFromExpressionCondition.Variable, assignmentFromExpressionCondition);
            }

            /// <summary>
            /// Adds the data.
            /// </summary>
            /// <param name="leftJoinCondition">The left join condition.</param>
            public void AddData(LeftJoinCondition leftJoinCondition)
            {
                foreach (var variable in leftJoinCondition.CalculusVariables)
                {
                    _variableConditions.Add(variable, leftJoinCondition);
                }

                _sourceConditions.Add(leftJoinCondition.RightOperand, leftJoinCondition);
            }
        }

        /// <summary>
        /// Visitor to fill <see cref="CalculusModelSpecificData"/>
        /// </summary>
        /// <seealso cref="ISourceConditionVisitor" />
        private class AddSourceCondition_Visitor
            : ISourceConditionVisitor
        {
            /// <summary>
            /// Visits <see cref="TupleFromSourceCondition"/>
            /// </summary>
            /// <param name="tupleFromSourceCondition">The visited instance</param>
            /// <param name="data">The passed data</param>
            /// <returns>The returned data</returns>
            public object Visit(TupleFromSourceCondition tupleFromSourceCondition, object data)
            {
                ((CalculusModelSpecificData)data).AddData(tupleFromSourceCondition);
                return null;
            }

            /// <summary>
            /// Visits <see cref="UnionedSourcesCondition"/>
            /// </summary>
            /// <param name="unionedSourcesCondition">The visited instance</param>
            /// <param name="data">The passed data</param>
            /// <returns>The returned data</returns>
            public object Visit(UnionedSourcesCondition unionedSourcesCondition, object data)
            {
                ((CalculusModelSpecificData)data).AddData(unionedSourcesCondition);
                return null;
            }

            /// <summary>
            /// Visits <see cref="LeftJoinCondition"/>
            /// </summary>
            /// <param name="leftJoinCondition">The visited instance</param>
            /// <param name="data">The passed data</param>
            /// <returns>The returned data</returns>
            public object Visit(LeftJoinCondition leftJoinCondition, object data)
            {
                ((CalculusModelSpecificData)data).AddData(leftJoinCondition);
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