using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainMenu
{
    // One card on the Chapter Complete screen: which remembered choice it reads,
    // and the sentence to show for each answer.
    [Serializable]
    public class ChapterSummaryItem
    {
        [Tooltip("The remembered choice to read, e.g. 'insurance'.")]
        public string choiceKey;

        [Tooltip("One sentence per answer. Use <b>...</b> for the bold part, e.g. You <b>bought the home insurance</b>. " +
                 "{player} is the player's name; any other {key} shows that remembered value, e.g. {laptop_cost}.")]
        public List<ChapterSummarySentence> sentences = new();

        [Tooltip("Shown when the player never made this choice. Leave empty to hide the card.")]
        [TextArea(1, 3)] public string sentenceIfNotChosen;

        [Tooltip("Small line under the 'not chosen' sentence: what doing it would have led to. Optional.")]
        [TextArea(1, 3)] public string otherOptionIfNotChosen;
    }

    [Serializable]
    public class ChapterSummarySentence
    {
        public string value;
        [TextArea(1, 3)] public string sentence;

        [Tooltip("Small line under the sentence: what the other option would have led to. Optional.")]
        [TextArea(1, 3)] public string otherOption;
    }

    // A chapter inside a level: the scene it plays, what the preview shows,
    // and what the Chapter Complete screen says at the end.
    [CreateAssetMenu(fileName = "NewChapter", menuName = "Levels/Chapter")]
    public class ChapterData : ScriptableObject
    {
        [SerializeField] private string id;
        [Tooltip("Without a number, e.g. 'First night'. The number comes from its position in the level.")]
        [SerializeField] private string displayName;
        [SerializeField] private string sceneName;
        [SerializeField] private Sprite screenshot;
        [Tooltip("Write {player} where the player's name should go.")]
        [SerializeField, TextArea(3, 6)] private string description;
        [SerializeField] private List<string> learningGoals = new();
        [SerializeField] private bool unlockedByDefault;

        [Header("Chapter Complete screen")]
        [SerializeField] private List<ChapterSummaryItem> summary = new();

        public string Id => string.IsNullOrEmpty(id) ? name : id;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string SceneName => sceneName;
        public Sprite Screenshot => screenshot;
        public string Description => description;
        public IReadOnlyList<string> LearningGoals => learningGoals;
        public bool UnlockedByDefault => unlockedByDefault;
        public IReadOnlyList<ChapterSummaryItem> Summary => summary;
    }
}
