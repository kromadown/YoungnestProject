using System.Collections.Generic;
using Dialogue;
using UnityEngine;
using UnityEngine.UIElements;

namespace Phone
{
    // Makes the phone act disabled while a dialogue is playing: its buttons don't highlight on hover
    // and can't be pressed, but it looks the same and still updates (new messages still arrive and show).
    // The phone body itself keeps catching the pointer, so clicks on it don't fall through to the room.
    [RequireComponent(typeof(UIDocument))]
    public class PhoneDialogueLock : MonoBehaviour
    {
        [Tooltip("The phone body that keeps catching clicks while its contents are locked.")]
        [SerializeField] private string phoneFrameName = "PhoneFrame";

        private VisualElement _root;
        private VisualElement _frame;
        private bool _locked;

        // Everything we made ignore the pointer, so it can be given back when the dialogue ends.
        private readonly List<VisualElement> _muted = new();

        private static bool DialoguePlaying => DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            if (_root == null) return;

            _frame = _root.Q<VisualElement>(phoneFrameName) ?? _root;

            // Keyboard/gamepad "press" on a focused phone button.
            _root.RegisterCallback<NavigationSubmitEvent>(BlockSubmit, TrickleDown.TrickleDown);
        }

        private void OnDisable()
        {
            Unlock();
            if (_root != null) _root.UnregisterCallback<NavigationSubmitEvent>(BlockSubmit, TrickleDown.TrickleDown);
        }

        private void Update()
        {
            if (_root == null) return;

            if (DialoguePlaying)
            {
                _locked = true;
                // Runs every frame so messages that arrive during the dialogue are locked too.
                MuteInside(_frame);
            }
            else if (_locked)
            {
                Unlock();
            }
        }

        private void MuteInside(VisualElement parent)
        {
            foreach (var child in parent.Children())
            {
                if (child.pickingMode == PickingMode.Position)
                {
                    child.pickingMode = PickingMode.Ignore;
                    _muted.Add(child);
                }
                MuteInside(child);
            }
        }

        private void Unlock()
        {
            _locked = false;
            foreach (var element in _muted)
            {
                if (element != null) element.pickingMode = PickingMode.Position;
            }
            _muted.Clear();
        }

        private void BlockSubmit(NavigationSubmitEvent evt)
        {
            if (DialoguePlaying) evt.StopImmediatePropagation();
        }
    }
}
