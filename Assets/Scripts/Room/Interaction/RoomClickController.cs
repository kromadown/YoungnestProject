using UnityEngine;
using UnityEngine.InputSystem;
using Room.Movement;

namespace Room.Interaction
{
    public class RoomClickController : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] LayerMask groundMask;
        [SerializeField] LayerMask interactableMask;
        [SerializeField] PlayerMover player;
        [SerializeField] GameObject clickMarkerPrefab;

        Interactable current;

        void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            Vector2 screenPos = pointer.position.ReadValue();
            Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

            Collider2D itemHit = Physics2D.OverlapPoint(worldPos, interactableMask);
            Interactable item = itemHit != null
                ? itemHit.GetComponentInParent<Interactable>()
                : null;
            SetCurrent(item);

            if (!pointer.press.wasPressedThisFrame) return;

            Instantiate(clickMarkerPrefab,
                new Vector3(worldPos.x, worldPos.y, 0f), Quaternion.identity);

            if (item != null)
            {
                Debug.Log("點到物品：" + item.name);
                return;
            }

            if (Physics2D.OverlapPoint(worldPos, groundMask) != null)
                player.MoveTo(worldPos);
        }

        void SetCurrent(Interactable item)
        {
            if (current == item) return;
            if (current != null) current.SetHighlight(false);
            current = item;
            if (current != null) current.SetHighlight(true);
        }
    }
}