using UnityEngine;
using Controllers.Data;

namespace Controllers.Objects
{
    public class MenuElement : MonoBehaviour
    {
        [field: SerializeField] public ScreenType ScreenType { get; private set; }
    }
}
