using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers.Objects.Game
{
    public class GamePlayerObject : NetworkBehaviour
    {
        #region Static

        public static GamePlayerObject Instance { get; private set; }

        #endregion

        #region Serializations

        [SerializeField] private Camera attachedCamera;
        [SerializeField] private Rigidbody rb;

        #endregion

        private const float MOVE_TIME = 1f;
        private static readonly Vector3 ARENA_POSITION = new Vector3(50,95,-19);
        private static readonly Quaternion ARENA_ROTATION = new Quaternion(0, 1, 0, 0);

        private Vector3 prevPosition;
        public Camera AttachedCamera { get => attachedCamera; }

        #region Constants

        private const int MOVE_SPEED = 50;

        #endregion

        #region Unity messages
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"Multiple {nameof(GamePlayerObject)} instances found. Deleting duplicate...");
                Destroy(Instance.gameObject);
            }
            else
            {
                Instance = this;
            }

            attachedCamera.transform.LookAt(transform);

        }
        #endregion

        #region Input
        public void StartMoving(float x, float y)
        {
            rb.linearVelocity = new Vector3(x, 0, y).normalized * MOVE_SPEED;
        }
        public void StopMoving()
        {
            rb.linearVelocity = Vector3.zero;
        }
        #endregion

        #region Throwing viewpoint
        public async Task MoveToArena()
        {
            prevPosition = transform.position;
            await MoveAsync(ARENA_POSITION, ARENA_ROTATION);
        }

        public async Task MoveToPrevious()
        {
            await MoveAsync(prevPosition, Quaternion.identity);
        }

        private async Task MoveAsync(Vector3 targetPos, Quaternion targetRot)
        {
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            float elapsed = 0f;

            AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            while (elapsed < MOVE_TIME)
            {
                float eval = curve.Evaluate(elapsed / MOVE_TIME);

                transform.position = Vector3.Lerp(startPosition, targetPos, eval);
                transform.rotation = Quaternion.Slerp(startRotation, targetRot, eval);

                elapsed += Time.deltaTime;

                // This should wait for next frame
                await Task.Yield();
            }

            // just to ensure exact values
            transform.position = targetPos;
            transform.rotation = targetRot;
        }
        #endregion

        public Vector3 GetCameraPosition()
        {
            return attachedCamera.transform.position;
        }
    }
}
