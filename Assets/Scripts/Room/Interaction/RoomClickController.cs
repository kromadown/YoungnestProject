using UnityEngine;
using UnityEngine.InputSystem;
using Room.Movement;

namespace Room.Interaction
{
    public class RoomClickController : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] LayerMask groundMask;
        [SerializeField] PlayerMover player;
        [SerializeField] GameObject clickMarkerPrefab;

        void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            if (!pointer.press.wasPressedThisFrame) return;

            Vector2 screenPos = pointer.position.ReadValue();
            Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

            Collider2D hit = Physics2D.OverlapPoint(worldPos, groundMask);
            if (hit != null)
            {
                Instantiate(clickMarkerPrefab, worldPos, Quaternion.identity);
                player.MoveTo(worldPos);
            }
        }
    }
}