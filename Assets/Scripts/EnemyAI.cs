using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public GameObject enemyProjectilePrefab;
    public Transform firePoint;
    public Transform gunPivot;
    public float projectileSpeed = 20f; // Висока швидкість для гарантованого влучання

    public void ExecutePerfectShot(TankController playerController)
    {
        StartCoroutine(ShotSequence(playerController));
    }

    private IEnumerator ShotSequence(TankController playerController)
    {
        // Пауза перед пострілом для візуального сприйняття гравцем
        yield return new WaitForSeconds(1f);

        Vector2 startPos = firePoint.position;
        Vector2 targetPos = playerController.transform.position;

        float x = Mathf.Abs(targetPos.x - startPos.x);
        float y = targetPos.y - startPos.y;
        float v = projectileSpeed;
        float g = Mathf.Abs(Physics2D.gravity.y);

        float discriminant = Mathf.Pow(v, 4) - g * (g * x * x + 2 * y * v * v);

        // Локальна змінна стану для синхронізації життєвого циклу снаряда
        bool isProjectileResolved = false;

        if (discriminant >= 0)
        {
            float angleRad = Mathf.Atan((v * v - Mathf.Sqrt(discriminant)) / (g * x));

            if (gunPivot != null) gunPivot.localRotation = Quaternion.Euler(0, 0, -(angleRad * Mathf.Rad2Deg));

            Vector2 velocity = new Vector2(-Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * v;

            GameObject proj = Instantiate(enemyProjectilePrefab, startPos, Quaternion.identity);
            Projectile projScript = proj.GetComponent<Projectile>();

            if (projScript != null)
            {
                // Передача анонімної функції (лямбда-виразу), яка змінить стан прапорця при колізії
                projScript.Initialize(velocity, (hitResult) => { isProjectileResolved = true; });
            }
            else
            {
                isProjectileResolved = true; // Захист від нескінченного циклу у разі відсутності компонента
            }
        }
        else
        {
            // Якщо математично постріл неможливий, дозволяємо співпрограмі продовжити виконання
            isProjectileResolved = true;
        }

        // Апаратне призупинення виконання співпрограми до моменту детонації снаряда
        yield return new WaitUntil(() => isProjectileResolved);

        // Коротка пост-пауза для відображення результату (вибуху) перед початком ходу гравця
        yield return new WaitForSeconds(0.5f);

        // Перевірка стану гравця. Якщо гравець живий (не Dead), надаємо йому новий хід
        if (playerController.currentState != TankController.TankState.Dead)
        {
            playerController.StartPlayerTurn();
        }
    }
}