using UnityEngine;

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
    public LayerMask enemyLayer; // Шар, на якому знаходяться об'єкти ворогів

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 initialVelocity)
    {
        rb.linearVelocity = initialVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Маршрутизація логіки залежно від обраного типу
        switch (damageType)
        {
            case DamageType.Direct:
                ApplyDirectDamage(collision.gameObject);
                break;
            case DamageType.AreaOfEffect:
                ApplyAreaDamage(collision.contacts[0].point);
                break;
        }

        // Знищення об'єкта снаряда після обчислення пошкоджень
        Destroy(gameObject);
    }

    private void ApplyDirectDamage(GameObject target)
    {
        // Перевірка наявності компонента здоров'я на об'єкті
        // EnemyHealth health = target.GetComponent<EnemyHealth>();
        // if (health != null) health.TakeDamage(damageAmount);

        Debug.Log($"Direct hit on {target.name} for {damageAmount} damage.");
    }

    private void ApplyAreaDamage(Vector2 impactPoint)
    {
        // Сканування простору в заданому радіусі
        Collider2D[] colliders = Physics2D.OverlapCircleAll(impactPoint, explosionRadius, enemyLayer);

        foreach (Collider2D hitCollider in colliders)
        {
            // EnemyHealth health = hitCollider.GetComponent<EnemyHealth>();
            // if (health != null) health.TakeDamage(damageAmount);

            Debug.Log($"Splash damage hit {hitCollider.name} for {damageAmount} damage.");
        }
    }

    // Метод для візуалізації радіуса вибуху в редакторі (Scene View)
    private void OnDrawGizmosSelected()
    {
        if (damageType == DamageType.AreaOfEffect)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}