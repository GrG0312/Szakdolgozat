using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace View
{
    public class CameraPivot : MonoBehaviour
    {
        /// <summary>
        /// The player input component that is assigned to this pivot object.
        /// </summary>
        private PlayerInput playerInputComponent;

        /// <summary>
        /// The camera that is assigned as a child to this pivot object.
        /// </summary>
        private Camera linkedCamera;

        /// <summary>
        /// The speed with which the pivot object will move around.
        /// </summary>
        /// <remarks>(The camera is a child of the object, so its transform is relative)</remarks>
        [SerializeField, Tooltip(tooltip: "The speed with which the pivot object will move around.")]
        private float moveSpeed;

        /// <summary>
        /// The value for determining the speed of acceleration and deceleration of the pivot object.
        /// </summary>
        [SerializeField, Tooltip(tooltip: "The value for determining the speed of acceleration and deceleration of the pivot object.")]
        private float interpolationSpeed;

        /// <summary>
        /// Represents the direction in which the player wants to move the camera.
        /// </summary>
        private Vector2 inputDirection;

        private bool isMoving = false;

        #region Methods

        #region Unity Methods
        public void Start()
        {
            playerInputComponent = GetComponent<PlayerInput>();

            linkedCamera = playerInputComponent.camera;
            linkedCamera.transform.LookAt(transform.position);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Will trigger when the Input System detects a change of state from one of the Bindings
        /// </summary>
        /// <param name="value"></param>
        private void OnMoveCamera(InputValue value)
        {
            inputDirection = value.Get<Vector2>();
            if (!isMoving)
            {
                // Starts a coroutine, which is basically an enumeration accross frames
                // Starts in the first frame, finishes one iteration and yields, continuing the iteration the next frame
                // Decided to use this instead of the Update method, because when the user doesn't moves,
                //      there is no need to set the camera's position to the same exact place every frame
                // If later this ends up not performing as well, it could be possible to implement a global Update Manager class
                //      and implement (un)subscibtion there
                StartCoroutine(CameraMovement());
            }
        }
        #endregion
        /// <summary>
        /// Controls the Player's camera's movement, and sets the <see cref="isMoving"/> variable's value before and after moving.
        /// </summary>
        /// <returns><c>null</c>, because there is no use for returning any value here</returns>
        private IEnumerator CameraMovement()
        {
            isMoving = true;
            while (inputDirection != Vector2.zero)
            {
                Vector3 interpolated = new Vector3();
                interpolated.x = Mathf.Lerp(interpolated.x, inputDirection.x, interpolationSpeed * Time.deltaTime);
                interpolated.z = Mathf.Lerp(interpolated.z, inputDirection.y, interpolationSpeed * Time.deltaTime);

                transform.Translate(interpolated * moveSpeed * Time.deltaTime);
                yield return null;
            }
            isMoving = false;
        }

        #endregion
    }
}
