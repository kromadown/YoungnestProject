using UnityEngine;

namespace Room.Interaction
{
    [CreateAssetMenu(menuName = "Room/Interactable Data")]
    public class InteractableData : ScriptableObject
    {
        public string promptText = "Click";
        public bool canRepeat = true;
    }
}