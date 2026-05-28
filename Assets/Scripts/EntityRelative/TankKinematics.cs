using UnityEngine;

namespace Assets.Scripts
{
    public class TankKinematics : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the object that handles vertical movement")]
        public Transform heightBody;

        [Tooltip("Reference to the object that handles rotation")]
        public Transform rotationBody;

        [Header("Scan Parameters")]
        public float raycastHeightOffset = 10f;
        public float raycastDistance = 30f;
        public LayerMask groundLayer;

        [Header("Interpolation Parameters")]
        public float positionLerpSpeed = 15f;
        public float rotationLerpSpeed = 10f;
        public float tankHeightOffset = 0.5f;

        private void Update()
        {
            Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y + raycastHeightOffset);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, raycastDistance, groundLayer);

            if (hit.collider != null)
            {
                ApplyKinematics(hit);
            }
        }

        private void ApplyKinematics(RaycastHit2D hit)
        {
            if (heightBody != null)
            {
                float targetY = hit.point.y + tankHeightOffset;
                Vector3 targetPosition = new Vector3(heightBody.position.x, targetY, heightBody.position.z);
                heightBody.position = Vector3.Lerp(heightBody.position, targetPosition, Time.deltaTime * positionLerpSpeed);
            }

            if (rotationBody != null)
            {
                float targetAngle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

                rotationBody.rotation = Quaternion.Lerp(rotationBody.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
            }
        }
    }
}