using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        // Логіка знищення або перезавантаження
        //gameObject.SetActive(false);
    }
}