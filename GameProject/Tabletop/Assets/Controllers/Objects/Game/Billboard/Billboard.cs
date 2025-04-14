using Model.Units;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.Objects.Game.Billboard
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer image;
        [SerializeField] private bool shouldLift = true;

        private void Start()
        {
            if (shouldLift)
            {
                // Lift the sprite a bit so that it doesnt glitch into the ground
                image.gameObject.transform.localPosition = new Vector3(0, image.bounds.size.y / 2.5f, 0);
            }
        }
        private void FixedUpdate()
        {
            // Get the camera's position
            Vector3 target = GamePlayerObject.Instance.GetCameraPosition();
            // Set the height to the my height
            target.y = transform.position.y;
            // Look at the targeted position
            transform.LookAt(target);
            // Rotate by 180 degrees becasue of how sprite rendering works
            transform.Rotate(0, 180f, 0);
        }
    }
}
