using UnityEngine;

namespace Assets.Scripts
{
    public class RepairStation : MonoBehaviour
    {
        public float healAmount = 100f;
        public LayerMask playerLayer;
        public GameObject isActiveIconObject;
        private bool canHealAgain = true; 

        public void OnEnable()
        {
            canHealAgain = true;
            isActiveIconObject.SetActive(true);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & playerLayer) != 0 & canHealAgain == true)
            {
                Health playerHealth = other.GetComponentInParent<Health>();
                if (playerHealth != null || playerHealth.currentHealth != playerHealth.maxHealth)
                {
                    playerHealth.Heal(healAmount);
                    isActiveIconObject.SetActive(false);
                    canHealAgain = false;
                    //gameObject.SetActive(false);
                }
            }
        }
    }
}