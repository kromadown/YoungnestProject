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

        [Tooltip("Optional. When this line appears, every Dialogue Cue in the scene with this name runs, " +
                 "e.g. 'laptop_off' to swap the laptop sprite.")]
        public string cue;
    }

    [Serializable]
    public class DialogueChoice
    {
        [Tooltip("What the option says.")]
        [TextArea(1, 3)] public string text;

        [Tooltip("Dialogue that plays when this option is picked. Leave empty to end the conversation.")]
        public DialogueData nextDialogue;

        [Tooltip("Optional. Saves this choice so later scenes can react, e.g. key 'insurance', value 'skipped'.")]
        public string rememberKey;
        public string rememberValue;
    }

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] private List<DialogueLine> lines = new();

        [Header("Choices (optional)")]
        [Tooltip("Shown above the options after the last line, e.g. 'About the home insurance...'.")]
        [SerializeField] private DialogueLine choicePrompt = new();
        [SerializeField] private List<DialogueChoice> choices = new();

        public IReadOnlyList<DialogueLine> Lines => lines;
        public DialogueLine ChoicePrompt => choicePrompt;
        public IReadOnlyList<DialogueChoice> Choices => choices;
        public bool HasChoices => choices.Count > 0;
    }
}
