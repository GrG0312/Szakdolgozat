using UnityEngine;

namespace Controllers.DataObjects
{
    public class Messenger : MonoBehaviour
    {
        public static Messenger Instance { get; private set; }
        private void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Debug.LogWarning($"Multiple {nameof(Messenger)} instances found. Deleting duplicate...");
                Destroy(this.gameObject);
            }
        }
    }
}
