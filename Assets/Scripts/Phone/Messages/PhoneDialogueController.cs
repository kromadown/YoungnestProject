using System;
using System.Collections.Generic;
using Dialogue;
using Story;
using UnityEngine;

namespace Phone.Messages
{
    public class PhoneDialogueController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MessageManager messageManager;
        
        private class PendingReply
        {
            public DialogueData Dialogue;
            public Action OnFinished;
        }

        private readonly Dictionary<PhoneContact, PendingReply> _pendingReplies = new();

        public event Action<PhoneContact> RepliesChanged;

        public IReadOnlyList<DialogueChoice> GetReplies(PhoneContact contact)
        {
            if (contact != null && _pendingReplies.TryGetValue(contact, out var pending))
            {
                return pending.Dialogue.Choices;
            }
            
            return Array.Empty<DialogueChoice>();
        }

        public void Play(DialogueData dialogue, PhoneContact contact, Action onFinished)
        {
            if (dialogue == null || contact == null || messageManager == null)
            {
                Debug.LogWarning("Cannot start phone dialogue.", this);
                return;
            }

            if (_pendingReplies.ContainsKey(contact))
            {
                Debug.LogWarning($"COntact {contact.DisplayName} already has a pending reply.", this);
                return;
            }

            foreach (var line in dialogue.Lines)
            {
                if (string.IsNullOrWhiteSpace(line.text)) continue;
                
                messageManager.ReceiveMessage(contact, line.text);
            }

            if (!dialogue.HasChoices)
            {
                onFinished?.Invoke();
                return;
            }

            if (!string.IsNullOrWhiteSpace(dialogue.ChoicePrompt.text))
            {
                messageManager.ReceiveMessage(contact, dialogue.ChoicePrompt.text);
            }
            
            _pendingReplies.Add(contact, new PendingReply
            {
                Dialogue = dialogue,
                OnFinished = onFinished
            });
            
            RepliesChanged?.Invoke(contact);
        }

        public void SelectReply(PhoneContact contact, DialogueChoice choice)
        {
            if (contact == null || choice == null || !_pendingReplies.TryGetValue(contact, out var pending))
            {
                return;
            }

            var isValidChoice = false;

            foreach (var availableChoice in pending.Dialogue.Choices)
            {
                if (availableChoice == choice)
                {
                    isValidChoice = true;
                    break;
                }
            }

            if (!isValidChoice) return;

            if (choice.nextDialogue != null && DialogueRouter.Instance == null)
            {
                Debug.LogError("DialogueRouter is missing!", this);
                return;
            }
            
            _pendingReplies.Remove(contact);
            RepliesChanged?.Invoke(contact);
            
            messageManager.SendMessage(contact, choice.text);

            if (!string.IsNullOrWhiteSpace(choice.rememberKey))
            {
                StoryChoices.Set(choice.rememberKey, choice.rememberValue);
            }

            if (choice.nextDialogue == null)
            {
                pending.OnFinished?.Invoke();
                return;
            }
            
            DialogueRouter.Instance.Play(choice.nextDialogue, pending.OnFinished, contact);
        }
    }
}
