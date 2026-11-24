using System;
using Phone;
using Phone.Messages;
using UnityEngine;

namespace Dialogue
{
    public class DialogueRouter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueManager roomDialogueManager;
        [SerializeField] private PhoneDialogueController phoneDialogueController;
        [SerializeField] private PhoneUIController phoneUIController;
        
        public static DialogueRouter Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Multiple instances of DialogueRouter!", this);
                enabled = false;
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Play(DialogueData dialogue, Action onFinished = null, PhoneContact contact = null)
        {
            if (dialogue == null)
            {
                Debug.LogWarning("No dialogue to play.", this);
                return;
            }

            switch (dialogue.Presentation)
            {
                case DialoguePresentation.Room:
                    if (roomDialogueManager == null)
                    {
                        Debug.LogError("Room DialogueManager is missing", this);
                        return;
                    }

                    if (phoneUIController != null)
                    {
                        phoneUIController.CloseForRoomDialogue();
                    }
                    
                    roomDialogueManager.Play(dialogue, onFinished, contact);
                    break;
                
                case DialoguePresentation.Phone:
                    if (phoneDialogueController == null || contact == null)
                    {
                        Debug.LogWarning("Phone dialogue is missing a controller and contact.", this);
                        return;
                    }
                    
                    phoneDialogueController.Play(dialogue, contact, onFinished);
                    break;
            }
        }
    }
}
