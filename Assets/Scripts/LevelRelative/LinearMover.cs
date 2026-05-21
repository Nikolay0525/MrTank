using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class LinearMover : MonoBehaviour
    {
        private float speed;
        private float despawnX;
        private int direction;
        private float parallax;

        public void Setup(float objectSpeed, float despawnPosition, int moveDirection, float parallaxMultiplier)
        {
            speed = objectSpeed;
            despawnX = despawnPosition;
            direction = moveDirection;
            parallax = parallaxMultiplier;

            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * (direction == 1 ? -1f : 1f);
            transform.localScale = currentScale;
        }

        private void Update()
        {
            Vector3 ownMovement = Vector3.right * (speed * direction);

            Vector3 landscapeMovement = Vector3.left * (TankController.CurrentGlobalSpeed * parallax);

            transform.Translate((ownMovement + landscapeMovement) * Time.deltaTime);

            float currentX = transform.position.x;

            float leftBound = Mathf.Min(despawnX, -despawnX);
            float rightBound = Mathf.Max(despawnX, -despawnX);

            if (currentX < leftBound - 2f || currentX > rightBound + 2f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}