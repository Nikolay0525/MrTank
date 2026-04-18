using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public ObjectPool projectilePool; 
    public Transform firePoint;
    public Transform gunPivot;

    [Header("Ammunition Stats")]
    public float projectileSpeed = 20f;
    public float damage = 15f; 
    public float projectileSize = 0.8f; 

    private Health myHealth;
    private int localShotsFired = 0;
    private bool isCurrentlyShooting = false; 
                                              
    private float cachedDiscriminant = -1f;
    private bool dontFindDiscriminant = false;

    private void Awake()
    {
        myHealth = GetComponent<Health>();
        if (myHealth == null) myHealth = GetComponentInParent<Health>();
    }

    public void ResetState()
    {
        localShotsFired = 0;
        isCurrentlyShooting = false;
        cachedDiscriminant = -1f;
        dontFindDiscriminant = false;

        if (myHealth != null)
        {
            myHealth.ResetHealth();
        }

        VisualDeathHandler deathHandler = GetComponent<VisualDeathHandler>();
        if (deathHandler != null)
        {
            deathHandler.ResetVisuals();
        }
    }

    public void ExecutePerfectShot(TankController playerController)
    {
        if (isCurrentlyShooting) return; 
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
        bool isIntentionalMiss = false;

        if (Random.value > hitChance)
        {
            isIntentionalMiss = true;
            float radius = DifficultyManager.Instance.GetEnemyMissRadius(localShotsFired);
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

        if (dontFindDiscriminant == false)
        {
            cachedDiscriminant = Mathf.Pow(v, 4) - g * (g * x * x + 2 * y * v * v);
            dontFindDiscriminant = cachedDiscriminant < 0 ? false : true;
        }

        bool isProjectileResolved = false;

        if (cachedDiscriminant >= 0)
        {
            float angleRad = Mathf.Atan((v * v - Mathf.Sqrt(cachedDiscriminant)) / (g * x));
            if (gunPivot != null) gunPivot.localRotation = Quaternion.Euler(0, 0, -(angleRad * Mathf.Rad2Deg));

            yield return new WaitForSeconds(0.4f); 

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
            float fallbackAngleDegrees = 45f;

            if (isIntentionalMiss)
            {
                fallbackAngleDegrees += missOffset * 3f;
            }
            else
            {
                fallbackAngleDegrees += missOffset;
            }

            fallbackAngleDegrees = Mathf.Clamp(fallbackAngleDegrees, 15f, 75f);

            float fallbackAngle = fallbackAngleDegrees * Mathf.Deg2Rad;
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

        yield return new WaitUntil(() => isProjectileResolved);

        yield return new WaitForSeconds(0.5f);

        isCurrentlyShooting = false; 

        if (playerController.currentState != TankController.TankState.Dead)
        {
            playerController.StartPlayerTurn();
        }
    }
}