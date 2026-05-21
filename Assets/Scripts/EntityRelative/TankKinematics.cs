using UnityEngine;

namespace Assets.Scripts
{
    public class TankKinematics : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the visual body object (if still needed for offsets)")]
        public Transform bodyVisual;

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
            float targetY = hit.point.y + tankHeightOffset;
            Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);

            if (bodyVisual != null)
            {
                float targetAngle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

                bodyVisual.rotation = Quaternion.Lerp(bodyVisual.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
            }
        }
    }
}