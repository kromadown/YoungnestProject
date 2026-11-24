using System;
using System.Collections.Generic;
using Dialogue;
using UnityEngine;

namespace Phone.Messages
{
    [Serializable]
    public class PhoneMessageStep
    {
        [Min(0)] public float delayBeforeMessage;
        [TextArea(2, 5)] public string text;
    }

    [CreateAssetMenu(fileName = "NewMessageSequence", menuName = "Phone/Message Sequence")]
    public class PhoneMessageSequence : ScriptableObject
    {
        [SerializeField] private PhoneContact sender;
        [SerializeField] private bool canRepeat;
        [SerializeField] private List<PhoneMessageStep> messages = new();
    
        [Header("Dialogue (Optional)")]
        [SerializeField] private DialogueData followUpDialogue;
        
        public PhoneContact Sender => sender;
        public bool CanRepeat => canRepeat;
        public IReadOnlyList<PhoneMessageStep> Messages => messages;
        public DialogueData FollowUpDialogue => followUpDialogue;
    }
}