using UnityEngine;
using UnityEngine.Events;

namespace Dialogue
{
    // Changes something in the scene at a specific dialogue line.
    // Give it the same Cue Name as a line's Cue field; when that line appears, On Cue runs
    // (e.g. swap a sprite, show an object, play an animation).
    public class DialogueCue : MonoBehaviour
    {
        [SerializeField] private string cueName;
        [SerializeField] private UnityEvent onCue;

        private void OnEnable()
        {
            DialogueManager.CueFired += OnCueFired;
        }

        private void OnDisable()
        {
            DialogueManager.CueFired -= OnCueFired;
        }

        private void OnCueFired(string cue)
        {
            if (!string.IsNullOrWhiteSpace(cueName) && cue == cueName.Trim()) onCue.Invoke();
        }
    }
}
