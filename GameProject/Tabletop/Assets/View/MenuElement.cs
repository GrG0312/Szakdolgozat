using UnityEngine;

namespace View
{
    public enum ScreenType
    {
        Main,
        Profile,
        Lobby,
        Joining,
        Decks
    }
    public class MenuElement : MonoBehaviour
    {
        [field: SerializeField]
        public ScreenType ScreenType { get; private set; }
    }
}
