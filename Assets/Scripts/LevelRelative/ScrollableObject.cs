using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class ScrollableObject : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Global X coordinate at which the object is deactivated")]
        public float despawnX = -30f;

        public float speedMultiplier = 1f;
        public event Action<GameObject> OnDespawnReached;

        private void Update()
        {
            float currentSpeed = TankController.CurrentGlobalSpeed * speedMultiplier;

            transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);

            if (transform.position.x <= despawnX)
            {
                OnDespawnReached?.Invoke(gameObject);
            }
        }
    }
}