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
        private readonly Dictionary<ISubjectMap, ISubjectMapping> createdMappings = new Dictionary<ISubjectMap, ISubjectMapping>();
        private readonly Dictionary<ISubjectMap, List<Action<ISubjectMapping>>> storeFunctions = new Dictionary<ISubjectMap, List<Action<ISubjectMapping>>>();

        public void GetSubjectMap(ISubjectMap subjectMap, Action<ISubjectMapping> storeFunction)
        {
            if (createdMappings.TryGetValue(subjectMap, out var createdMapping))
            {
                storeFunction(createdMapping);
            }
            else
            {
                if (!storeFunctions.TryGetValue(subjectMap, out var actionList))
                {
                    actionList = new List<Action<ISubjectMapping>>();
                    storeFunctions.Add(subjectMap, actionList);
                }

                actionList.Add(storeFunction);
            }
        }

        public void RegisterSubjectMapping(ISubjectMap subjectMap, SubjectMapping createdMapping)
        {
            createdMappings.Add(subjectMap, createdMapping);

            if (storeFunctions.TryGetValue(subjectMap, out var actionList))
            {
                foreach (var action in actionList)
                {
                    action(createdMapping);
                }
            }
        }
    }
}