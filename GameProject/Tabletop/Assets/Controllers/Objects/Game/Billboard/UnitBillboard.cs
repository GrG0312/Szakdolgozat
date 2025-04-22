using UnityEngine;
using Model.UnityDependant;

namespace Controllers.Objects.Game.Billboard
{
    public class UnitBillboard : Billboard
    {

        public void SetupVisuals(UnitVisualData visuals)
        {
            Sprite sprite = Resources.Load<Sprite>(visuals.UnitFullSprite);
            image.sprite = sprite;
            // Lift the sprite a bit so that it doesnt glitch into the ground
            image.gameObject.transform.localPosition = new Vector3(0, image.bounds.size.y / 2.5f, 0);
        }

        #region Unity messages
        private void FixedUpdate()
        {
            // Get the camera's position
            Vector3 target = CameraObject.Instance.GetCameraPosition();
            // Set the height to the my height
            target.y = transform.position.y;
            // Look at the targeted position
            transform.LookAt(target);
            // Rotate by 180 degrees becasue of how sprite rendering works
            transform.Rotate(0, 180f, 0);
        }
        #endregion
    }
}
