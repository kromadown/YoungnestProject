using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Phone.Messages
{
    public class ReceivedPhoneMessage
    {
        public string Text { get; private set; }
        public bool IsRead { get;  set; }

        public ReceivedPhoneMessage(string text)
        {
            Text = text;
        }
    }

    public class MessageManager : MonoBehaviour
    {
        [Header("Testing")] 
        [SerializeField] private PhoneMessageSequence testSequenceOnStart;

        private readonly Dictionary<PhoneContact, List<ReceivedPhoneMessage>> _conversations = new ();
        private readonly List<PhoneContact> _contacts = new ();
        private readonly HashSet<PhoneMessageSequence> _startedSequences = new ();
    
        public event Action<PhoneContact> MessageReceived;
    
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
        }

        private void ReceiveMessage(PhoneContact sender, string text)
        {
            if (!_conversations.TryGetValue(sender, out var messages))
            {
                messages = new List<ReceivedPhoneMessage>();
                _conversations.Add(sender, messages);
            }
        
            messages.Add(new ReceivedPhoneMessage(text));
        
            _contacts.Remove(sender);
            _contacts.Insert(0, sender);
        
            MessageReceived?.Invoke(sender);
        }

        public IReadOnlyList<ReceivedPhoneMessage> GetConversation(PhoneContact contact)
        {
            if (_conversations.TryGetValue(contact, out var messages))
            {
                return messages;
            }
        
            return Array.Empty<ReceivedPhoneMessage>();
        }

        public void MarkAsRead(PhoneContact contact)
        {
            foreach (var message in GetConversation(contact))
            {
                message.IsRead = true;
            }
        }

        public int GetUnreadCount(PhoneContact contact)
        {
            var count = 0;

            foreach (var message in GetConversation(contact))
            {
                if (!message.IsRead)
                {
                    count++;
                }
            }
        
            return count;
        }
    }
}