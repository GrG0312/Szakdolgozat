using UnityEngine;

namespace Controllers.Assets.Controllers
{
    public class FpsLimiter : BaseController<FpsLimiter>
    {
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning($"Multiple {nameof(FpsLimiter)} instances found. Deleting duplicate...");
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }
    }
}
