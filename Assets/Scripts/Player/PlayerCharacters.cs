using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    // The characters the player can pick on the character select screen, in order.
    // The main menu shows them; the room's player (PlayerAppearance) uses the picked one's sprites.
    [CreateAssetMenu(fileName = "PlayerCharacters", menuName = "Player/Player Characters")]
    public class PlayerCharacters : ScriptableObject
    {
        [Serializable]
        public class Character
        {
            [Tooltip("Facing the camera. Also shown on the character select screen.")]
            public Sprite front;
            [Tooltip("Walking away from the camera. Leave empty to use the front sprite.")]
            public Sprite back;

            public Sprite Back => back != null ? back : front;
        }

        [SerializeField] private List<Character> characters = new();

        public IReadOnlyList<Character> All => characters;

        // The character the player picked, or the first one if the saved choice no longer exists.
        public Character Picked
        {
            get
            {
                if (characters.Count == 0) return null;
                var index = PlayerProfile.CharacterIndex;
                return characters[index >= 0 && index < characters.Count ? index : 0];
            }
        }
    }
}
