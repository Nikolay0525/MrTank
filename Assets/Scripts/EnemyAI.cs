using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public ObjectPool projectilePool; // Наш спільний пул
    public Transform firePoint;
    public Transform gunPivot;

    [Header("Ammunition Stats")]
    public float projectileSpeed = 20f;
    public float damage = 15f;          // Слабший урон для звичайного ворога
    public float projectileSize = 0.8f; // Трохи менший снаряд

    private Health myHealth;
    private int localShotsFired = 0;
    private bool isCurrentlyShooting = false; // Запобіжник

    private void Awake()
    {
        myHealth = GetComponent<Health>();
        if (myHealth == null) myHealth = GetComponentInParent<Health>();
    }

    public void ExecutePerfectShot(TankController playerController)
    {
        if (isCurrentlyShooting) return; // Якщо вже стріляє, ігноруємо новий виклик
        StartCoroutine(ShotSequence(playerController));
    }

    private IEnumerator ShotSequence(TankController playerController)
    {
        isCurrentlyShooting = true;
        yield return new WaitForSeconds(1f);

        if (myHealth != null && myHealth.currentHealth <= 0)
        {
            isCurrentlyShooting = false;
            yield break;
        }

        Vector2 startPos = firePoint.position;
        Vector2 targetPos = playerController.transform.position;

        float hitChance = DifficultyManager.Instance.GetEnemyHitChance(localShotsFired);
        float missOffset = 0f;

        if (Random.value > hitChance)
        {
            float radius = DifficultyManager.Instance.GetEnemyMissRadius();
            float randomMiss = Random.Range(1.5f, radius);
            missOffset = Random.value > 0.5f ? randomMiss : -randomMiss;
        }
        else
        {
            missOffset = Random.Range(-0.2f, 0.2f);
        }

        targetPos += new Vector2(missOffset, 0f);
        localShotsFired++;

        float x = Mathf.Abs(targetPos.x - startPos.x);
        float y = targetPos.y - startPos.y;
        float v = projectileSpeed;
        float g = Mathf.Abs(Physics2D.gravity.y);

        float discriminant = Mathf.Pow(v, 4) - g * (g * x * x + 2 * y * v * v);
        bool isProjectileResolved = false;

        if (discriminant >= 0)
        {
            // --- ЛОГІКА ПОСТРІЛУ (як ми робили раніше) ---
            float angleRad = Mathf.Atan((v * v - Mathf.Sqrt(discriminant)) / (g * x));
            if (gunPivot != null) gunPivot.localRotation = Quaternion.Euler(0, 0, -(angleRad * Mathf.Rad2Deg));

            yield return new WaitForSeconds(0.4f); // Пауза для прицілювання

            Vector2 velocity = new Vector2(-Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * v;
            GameObject proj = ProjectilePoolManager.Instance.GetProjectile();
            proj.transform.position = firePoint.position;
            proj.SetActive(true);

            Projectile projScript = proj.GetComponent<Projectile>();
            if (projScript != null)
                projScript.Initialize(velocity, damage, projectileSize, (hitResult) => { isProjectileResolved = true; });
            else isProjectileResolved = true;
        }
        else
        {
            // --- ЯКЩО МАТЕМАТИКА НЕ ДУМАЄ ---
            Debug.LogWarning("Бот не може дострілити до цілі! Дискримінант < 0");

            // Щоб гра не зависла, бот вистрілить просто "в той бік" під кутом 45 градусів
            float fallbackAngle = 45f * Mathf.Deg2Rad;
            if (gunPivot != null) gunPivot.localRotation = Quaternion.Euler(0, 0, -45f);

            yield return new WaitForSeconds(0.4f);

            Vector2 velocity = new Vector2(-Mathf.Cos(fallbackAngle), Mathf.Sin(fallbackAngle)) * v;
            GameObject proj = ProjectilePoolManager.Instance.GetProjectile();
            proj.transform.position = firePoint.position;
            proj.SetActive(true);

            Projectile projScript = proj.GetComponent<Projectile>();
            if (projScript != null)
                projScript.Initialize(velocity, damage, projectileSize, (hitResult) => { isProjectileResolved = true; });
            else isProjectileResolved = true;
        }

        // Чекаємо, поки снаряд кудись влучить
        yield return new WaitUntil(() => isProjectileResolved);

        yield return new WaitForSeconds(0.5f);

        isCurrentlyShooting = false; // Звільняємо бота

        if (playerController.currentState != TankController.TankState.Dead)
        {
            playerController.StartPlayerTurn();
        }
    }
}