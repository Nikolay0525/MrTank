using UnityEngine;
using System;

public enum DamageType
{
    Direct,
    AreaOfEffect
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Damage Configuration")]
    public DamageType damageType = DamageType.Direct;
    public float damageAmount = 25f;

    [Header("Explosive Parameters (For AoE)")]
    public float explosionRadius = 2.5f;
    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Action<bool> onResolutionCallback; // Змінна для зберігання делегата

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Збереження делегата при ініціалізації
    public void Initialize(Vector2 initialVelocity, Action<bool> callback = null)
    {
        rb.linearVelocity = initialVelocity;
        onResolutionCallback = callback;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isHit = false;

        switch (damageType)
        {
            case DamageType.Direct:
                isHit = ApplyDirectDamage(collision.gameObject);
                break;
            case DamageType.AreaOfEffect:
                isHit = ApplyAreaDamage(collision.contacts[0].point);
                break;
        }

        // Виклик делегата передає системі результат (true - влучання, false - промах)
        onResolutionCallback?.Invoke(isHit);

        Destroy(gameObject);
    }

    private bool ApplyDirectDamage(GameObject target)
    {
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
            return true; // Реєстрація успішного влучання
        }
        return false; // Колізія з ландшафтом або іншим об'єктом
    }

    private bool ApplyAreaDamage(Vector2 impactPoint)
    {
        bool hitAnyEnemy = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(impactPoint, explosionRadius, enemyLayer);

        foreach (Collider2D hitCollider in colliders)
        {
            Health health = hitCollider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                hitAnyEnemy = true;
            }
        }
        return hitAnyEnemy;
    }
}