using UnityEngine;

namespace Phone.Messages
{
    [CreateAssetMenu(fileName = "NewContact", menuName = "Phone/Contact")]
    public class PhoneContact : ScriptableObject
    {
        [SerializeField] private Sprite avatar;
    
        public string DisplayName => name;
        public Sprite Avatar => avatar;
    }
}
