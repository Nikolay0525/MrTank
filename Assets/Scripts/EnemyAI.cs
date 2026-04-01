using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Combat Configuration")]
    public GameObject enemyProjectilePrefab;
    public Transform firePoint;
    public Transform gunPivot;
    public float projectileSpeed = 15f;
    public float fireRate = 3f;

    private Transform target;
    private bool inCombat = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Перевірка перетину зони виявлення з шаром гравця
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !inCombat)
        {
            target = other.transform;
            inCombat = true;
            StartCoroutine(CombatRoutine());
        }
    }

    private IEnumerator CombatRoutine()
    {
        // Затримка перед першим пострілом для синхронізації з зупинкою кадру
        yield return new WaitForSeconds(1f);

        while (inCombat && target != null)
        {
            CalculateTrajectoryAndFire();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void CalculateTrajectoryAndFire()
    {
        if (enemyProjectilePrefab == null || firePoint == null) return;

        Vector2 startPos = firePoint.position;
        Vector2 targetPos = target.position;

        // Кінематичні змінні
        float x = Mathf.Abs(targetPos.x - startPos.x);
        float y = targetPos.y - startPos.y;
        float v = projectileSpeed;
        float g = Mathf.Abs(Physics2D.gravity.y);

        // Обчислення дискримінанта для рівняння балістичної траєкторії
        float discriminant = Mathf.Pow(v, 4) - g * (g * x * x + 2 * y * v * v);

        if (discriminant >= 0)
        {
            // Розрахунок кута настильної траєкторії (менший кут)
            float angleRad = Mathf.Atan((v * v - Mathf.Sqrt(discriminant)) / (g * x));

            // Застосування кута до графічного об'єкта гармати
            float angleDeg = angleRad * Mathf.Rad2Deg;
            if (gunPivot != null)
            {
                // Поворот враховує дзеркальне напрямлення (вліво)
                gunPivot.localRotation = Quaternion.Euler(0, 0, -angleDeg);
            }

            // Формування вектора імпульсу
            float signX = Mathf.Sign(targetPos.x - startPos.x);
            Vector2 velocity = new Vector2(Mathf.Cos(angleRad) * signX, Mathf.Sin(angleRad)) * v;

            // Інстанціювання снаряда
            GameObject proj = Instantiate(enemyProjectilePrefab, startPos, Quaternion.identity);
            Projectile projScript = proj.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.Initialize(velocity);
            }
        }
        else
        {
            Debug.LogWarning("Ціль знаходиться поза зоною досяжності при поточній швидкості снаряда.");
        }
    }
}