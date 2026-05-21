using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        [Header("Targeting")]
        [Tooltip("Target tank transform")]
        public Transform target;
        [Tooltip("Vertical offset from target")]
        public float verticalOffset = 2f;

        [Header("Active State Parameters")]
        [Tooltip("X coordinate when out of Garage")]
        public float activeX = 6f;
        [Tooltip("Z coordinate when out of Garage")]
        public float activeZ = -10f;

        [Header("Garage State Parameters")]
        [Tooltip("Exact camera coordinates in Garage")]
        public Vector3 garagePosition = new Vector3(0f, 2f, -10f);

        [Header("Zoom Settings")]
        [Tooltip("Reference to the Camera component")]
        public Camera cam;
        [Tooltip("Camera orthographic size in Garage")]
        public float garageSize = 5f;
        [Tooltip("Camera orthographic size when out of Garage")]
        public float activeSize = 7f;

        private float sizeVelocity;

        [Header("State Tracking")]
        [Tooltip("Reference to the tank controller to read its state")]
        public TankController tankController;

        [Header("Dynamics")]
        [Tooltip("Delay time for smoothing (in seconds)")]
        public float smoothTime = 0.3f;

        private Vector3 currentVelocity;

        private void Start()
        {
            transform.position = GetDesiredPosition();

            if (cam != null)
            {
                cam.orthographicSize = GetDesiredSize();
            }
        }

        private Vector3 GetDesiredPosition()
        {
            // If in Garage, use the exact coordinates from the Inspector instead of 0,0,0
            if (tankController != null && tankController.currentState == TankController.TankState.Garage)
            {
                return garagePosition;
            }

            // If in any other state and target is assigned
            if (target != null)
            {
                float targetY = target.position.y + verticalOffset;
                return new Vector3(activeX, targetY, activeZ);
            }

            // Fallback if target is missing
            return transform.position;
        }

        private float GetDesiredSize()
        {
            if (tankController != null && tankController.currentState == TankController.TankState.Garage)
            {
                return garageSize;
            }

            return activeSize;
        }

        private void LateUpdate()
        {
            Vector3 targetPosition = GetDesiredPosition();
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

            if (cam != null)
            {
                float targetSize = GetDesiredSize();
                cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref sizeVelocity, smoothTime);
            }
        }
    }
}