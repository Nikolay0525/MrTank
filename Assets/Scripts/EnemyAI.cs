using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public GameObject enemyProjectilePrefab;
    public Transform firePoint;
    public Transform gunPivot;
    public float projectileSpeed = 20f;

    private Health myHealth;

    private void Awake()
    {
        // Ініціалізація компонента контролю здоров'я
        myHealth = GetComponent<Health>();
        if (myHealth == null)
        {
            myHealth = GetComponentInParent<Health>();
        }
    }

    public void ExecutePerfectShot(TankController playerController)
    {
        StartCoroutine(ShotSequence(playerController));
    }

    private IEnumerator ShotSequence(TankController playerController)
    {
        yield return new WaitForSeconds(1f);

        // Валідація життєздатності об'єкта після затримки.
        // Переривання співпрограми, якщо танк класифіковано як знищений.
        if (myHealth != null && myHealth.currentHealth <= 0)
        {
            yield break;
        }

        Vector2 startPos = firePoint.position;
        Vector2 targetPos = playerController.transform.position;

        float x = Mathf.Abs(targetPos.x - startPos.x);
        float y = targetPos.y - startPos.y;
        float v = projectileSpeed;
        float g = Mathf.Abs(Physics2D.gravity.y);

        float discriminant = Mathf.Pow(v, 4) - g * (g * x * x + 2 * y * v * v);

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
                projScript.Initialize(velocity, Projectile.ShootDirection.Left, (hitResult) => { isProjectileResolved = true; });
            }
            else isProjectileResolved = true;
        }
        else isProjectileResolved = true;

        yield return new WaitUntil(() => isProjectileResolved);

        yield return new WaitForSeconds(0.5f);

        if (playerController.currentState != TankController.TankState.Dead)
        {
            playerController.StartPlayerTurn();
        }
    }
}