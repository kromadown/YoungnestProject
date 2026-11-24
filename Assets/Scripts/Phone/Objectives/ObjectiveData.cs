using UnityEngine;

namespace Phone.Objectives
{
    [CreateAssetMenu(fileName = "NewObjective", menuName = "Phone/Objective")]
    public class ObjectiveData : ScriptableObject
    {
        [SerializeField] private string title;
        [SerializeField, TextArea(2, 5)] private string description;
        
        public string Title => title;
        public string Description => description;
    }
}
