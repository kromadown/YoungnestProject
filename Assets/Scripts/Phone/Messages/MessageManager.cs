using System;
using System.Collections;
using System.Collections.Generic;
using Dialogue;
using UnityEngine;

namespace Phone.Messages
{
    public class PhoneMessage
    {
        public string Text { get; private set; }
        public bool IsFromPlayer { get; private set; }
        public bool IsRead { get;  set; }

        public PhoneMessage(string text, bool isFromPlayer)
        {
            Text = text;
            IsFromPlayer = isFromPlayer;
            IsRead = isFromPlayer;
        }
    }

    public class MessageManager : MonoBehaviour
    {
        [Header("Testing")] 
        [SerializeField] private PhoneMessageSequence testSequenceOnStart;

        private readonly Dictionary<PhoneContact, List<PhoneMessage>> _conversations = new ();
        private readonly List<PhoneContact> _contacts = new ();
        private readonly HashSet<PhoneMessageSequence> _startedSequences = new ();
    
        public event Action<PhoneContact> MessageReceived;
        public event Action<PhoneContact> ConversationChanged;
    
        public IReadOnlyList<PhoneContact> Contacts => _contacts;

        private void Start()
        {
            if (testSequenceOnStart != null)
            {
                DeliverSequence(testSequenceOnStart);
            }
        }

        public void DeliverSequence(PhoneMessageSequence sequence)
        {
            if (sequence == null || sequence.Sender == null)
            {
                Debug.LogWarning("Invalid message sequence", this);
                return;
            }

            if (!sequence.CanRepeat && !_startedSequences.Add(sequence)) return;
        
            StartCoroutine(DeliverMessages(sequence));
        }

        private IEnumerator DeliverMessages(PhoneMessageSequence sequence)
        {
            foreach (var message in sequence.Messages)
            {
                if (message.delayBeforeMessage > 0)
                {
                    yield return new WaitForSeconds(message.delayBeforeMessage);
                }
            
                ReceiveMessage(sequence.Sender, message.text);
            }

            if (sequence.FollowUpDialogue != null)
            {
                var router = DialogueRouter.Instance;
                if (router == null)
                {
                    Debug.LogWarning("DialogueRouter is missing!", this);
                    yield break;
                }
                
                router.Play(sequence.FollowUpDialogue, contact: sequence.Sender);
            }
        }

        public void ReceiveMessage(PhoneContact sender, string text)
        {
            if (sender == null || string.IsNullOrWhiteSpace(text))
            {
                Debug.LogWarning("Received message is invalid.", this);
                return;
            }
            
            AddMessage(sender, text, false);
            MessageReceived?.Invoke(sender);
        }

        public void SendMessage(PhoneContact recipient, string text)
        {
            if (recipient == null || string.IsNullOrWhiteSpace(text))
            {
                Debug.LogWarning("Message is invalid.", this);
                return;
            }
            
            AddMessage(recipient, text, true);
        }
        
        private void AddMessage(PhoneContact contact, string text, bool isFromPlayer)
        {
            if (!_conversations.TryGetValue(contact, out var messages))
            {
                messages = new List<PhoneMessage>();
                _conversations.Add(contact, messages);
            }
            
            messages.Add(new PhoneMessage(text, isFromPlayer));
            
            _contacts.Remove(contact);
            _contacts.Insert(0, contact);
            
            ConversationChanged?.Invoke(contact);
        }
        
        public IReadOnlyList<PhoneMessage> GetConversation(PhoneContact contact)
        {
            if (_conversations.TryGetValue(contact, out var messages))
            {
                return messages;
            }
        
            return Array.Empty<PhoneMessage>();
        }

        public void MarkAsRead(PhoneContact contact)
        {
            foreach (var message in GetConversation(contact))
            {
                if (message.IsFromPlayer) continue;
                
                message.IsRead = true;
            }
        }

        public int GetUnreadCount(PhoneContact contact)
        {
            var count = 0;

            foreach (var message in GetConversation(contact))
            {
                if (!message.IsFromPlayer && !message.IsRead)
                {
                    count++;
                }
            }
        
            return count;
        }
    }
}