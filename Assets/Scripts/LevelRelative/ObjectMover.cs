using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class ObjectMover : MonoBehaviour
    {
        public Action<GameObject> onDespawnCallback;

        private float despawnX;
        private bool shouldDespawnInRightSide;
        private float parallax;
        private float speed;
        private int direction;
        private bool isSetup = false;

        public void Setup(float despawnPosition, bool DespawnInRightSide, float parallaxMultiplier = 1f, float objectSpeed = 0f, int moveDirection = -1)
        {
            despawnX = despawnPosition;
            shouldDespawnInRightSide = DespawnInRightSide;
            parallax = parallaxMultiplier;
            speed = objectSpeed;
            direction = moveDirection;

            if (speed > 0f)
            {
                Vector3 currentScale = transform.localScale;
                currentScale.x = Mathf.Abs(currentScale.x) * (direction == 1 ? -1f : 1f);
                transform.localScale = currentScale;
            }

            isSetup = true;
        }

        private void Update()
        {
            if (!isSetup) return;

            Vector3 ownMovement = Vector3.right * (speed * direction);

            Vector3 landscapeMovement = Vector3.left * (TankController.Instance.CurrentGlobalSpeed * parallax);

            transform.Translate((ownMovement + landscapeMovement) * Time.deltaTime);

            float currentX = transform.position.x;
            float leftBound = Mathf.Min(despawnX, -despawnX);
            float rightBound = Mathf.Max(despawnX, -despawnX);

            if (currentX < leftBound || shouldDespawnInRightSide && currentX > rightBound)
            {
                isSetup = false;
                if (onDespawnCallback != null)
                {
                    onDespawnCallback.Invoke(gameObject);
                }
                else
                {
                    PoolManager.Instance.ReturnObject(gameObject);
                }
            }
        }
    }
}
