using UnityEngine;

namespace Phone.Messages
{
    [CreateAssetMenu(fileName = "NewContact", menuName = "Phone/Contact")]
    public class PhoneContact : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private Sprite avatar;
    
        public string DisplayName => displayName;
        public Sprite Avatar => avatar;
    }
}
