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
    public enum ShootDirection { Right, Left }

    [Header("Damage Configuration")]
    public DamageType damageType = DamageType.Direct;
    public float damageAmount = 25f;

    [Header("Explosive Parameters (For AoE)")]
    public float explosionRadius = 2.5f;
    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Action<bool> onResolutionCallback; // Змінна для зберігання делегата
    private bool isInitialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Додаємо нові аргументи: float damage та float sizeMultiplier
    public void Initialize(Vector2 initialVelocity, float damage, float sizeMultiplier, Action<bool> callback = null)
    {
        rb.linearVelocity = initialVelocity;

        damageAmount = damage;
        transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, 1f);

        onResolutionCallback = callback;
        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (isInitialized && rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Vector2 v = rb.linearVelocity;

            // Отримання абсолютного світового кута вектора швидкості
            float angleRad = Mathf.Atan2(v.y, v.x);
            float angleDeg = angleRad * Mathf.Rad2Deg;

            // Застосування константи зміщення для спрайту, зорієнтованого вгору
            angleDeg -= 90f;

            transform.rotation = Quaternion.Euler(0, 0, angleDeg);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isHit = false;
        isInitialized = false;

        switch (damageType)
        {
            case DamageType.Direct:
                isHit = ApplyDirectDamage(collision.gameObject);
                break;
            case DamageType.AreaOfEffect:
                isHit = ApplyAreaDamage(collision.contacts[0].point);
                break;
        }

        onResolutionCallback?.Invoke(isHit);

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        isInitialized = false;
        onResolutionCallback = null;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private bool ApplyDirectDamage(GameObject target)
    {
        Health health = target.GetComponentInParent<Health>();
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