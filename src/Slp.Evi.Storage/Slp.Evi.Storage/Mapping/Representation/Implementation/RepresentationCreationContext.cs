using System;
using System.Collections.Generic;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Represents a context used during representation creation.
    /// </summary>
    public class RepresentationCreationContext
    {
        private readonly Dictionary<ISubjectMap, ISubjectMapping> _createdMappings = new Dictionary<ISubjectMap, ISubjectMapping>();
        private readonly Dictionary<ISubjectMap, List<Action<ISubjectMapping>>> _storeFunctions = new Dictionary<ISubjectMap, List<Action<ISubjectMapping>>>();

        /// <summary>
        /// For <see cref="ISubjectMap"/> gets the <see cref="ISubjectMapping"/> as soon as it is available
        /// and returns it by calling <paramref name="storeFunction"/>.
        /// </summary>
        public void GetSubjectMap(ISubjectMap subjectMap, Action<ISubjectMapping> storeFunction)
        {
            if (_createdMappings.TryGetValue(subjectMap, out var createdMapping))
            {
                storeFunction(createdMapping);
            }
            else
            {
                if (!_storeFunctions.TryGetValue(subjectMap, out var actionList))
                {
                    actionList = new List<Action<ISubjectMapping>>();
                    _storeFunctions.Add(subjectMap, actionList);
                }

                actionList.Add(storeFunction);
            }
        }

        /// <summary>
        /// Registers created subject mapping.
        /// </summary>
        /// <param name="subjectMap">For which <see cref="ISubjectMap"/> was <paramref name="createdMapping"/> created.</param>
        /// <param name="createdMapping">The created <see cref="ISubjectMapping"/>.</param>
        public void RegisterSubjectMapping(ISubjectMap subjectMap, ISubjectMapping createdMapping)
        {
            _createdMappings.Add(subjectMap, createdMapping);

            if (_storeFunctions.TryGetValue(subjectMap, out var actionList))
            {
                foreach (var action in actionList)
                {
                    action(createdMapping);
                }
            }
        }
    }
}