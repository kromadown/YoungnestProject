using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Name shown in the name tag. Leave empty to hide the name tag (e.g. for narration).")]
        public string speaker;
        [TextArea(2, 5)] public string text;
    }

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] private List<DialogueLine> lines = new();

        public IReadOnlyList<DialogueLine> Lines => lines;
    }
}
