using System;
using UnityEngine;

namespace Phone.Objectives
{
    [Serializable]
    public class ChapterObjectiveEntry
    {
        [SerializeField] private ObjectiveData objective;
        [SerializeField] private bool activeAtStart;
        
        public ObjectiveData Objective => objective;
        public bool ActiveAtStart => activeAtStart;
    }
}
