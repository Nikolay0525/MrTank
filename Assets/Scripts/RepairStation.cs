using UnityEngine;

public class RepairStation : MonoBehaviour
{
    public float healAmount = 100f;
    public LayerMask playerLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Щось увійшло в зону ремонту: " + other.name);
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            Health playerHealth = other.GetComponentInParent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);

                //gameObject.SetActive(false);
            }
        }
    }
}