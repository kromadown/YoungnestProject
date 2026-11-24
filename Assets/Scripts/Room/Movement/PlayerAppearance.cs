using Player;
using UnityEngine;

namespace Room.Movement
{
    // Dresses the room's player as the character picked on the character select screen.
    // Sits next to PlayerMover on the player.
    [RequireComponent(typeof(PlayerMover))]
    public class PlayerAppearance : MonoBehaviour
    {
        [SerializeField] private PlayerCharacters characters;

        private void Awake()
        {
            var picked = characters != null ? characters.Picked : null;
            if (picked == null || picked.front == null) return;

            GetComponent<PlayerMover>().SetSprites(picked.front, picked.Back);
        }
    }
}
