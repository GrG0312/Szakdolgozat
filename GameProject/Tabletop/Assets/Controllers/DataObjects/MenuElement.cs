using UnityEngine;
using Controllers.Data;

namespace Controllers.DataObjects
{
    public class MenuElement : MonoBehaviour
    {
        [field: SerializeField] public ScreenType ScreenType { get; private set; }
    }
}
