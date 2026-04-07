using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimingSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Посилання на точку обертання гармати")]
    public Transform gunPivot;

    [Header("Ballistics")]
    public float projectileSpeed = 12f;
    public float minAngle = 0f;
    public float maxAngle = 90f;
    public float aimSpeed = 3f;

    [Header("Ammunition Stats")]
    public float damage = 15f;          // Слабший урон для звичайного ворога
    public float projectileSize = 0.8f; // Трохи менший снаряд

    [Header("Trajectory Rendering")]
    public int linePoints = 120;
    public float timeStep = 0.05f;

    [Header("Spawning")]
    public Transform firePoint;

    private LineRenderer lineRenderer;
    private float currentAngle;
    private bool isAiming = false;
    private Quaternion defaultGunRotation;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints;
        lineRenderer.enabled = false;

        if (gunPivot != null)
        {
            // Фіксація початкового положення гармати
            defaultGunRotation = gunPivot.localRotation;
        }
    }

    private void Update()
    {
        if (isAiming)
        {
            OscillateAngle();
            DrawTrajectory();
            RotateGun();
        }
    }

    public void StartAiming()
    {
        isAiming = true;
        lineRenderer.enabled = true;
    }

    // Метод для примусового скасування прицілювання (наприклад, при вичерпанні таймера)
    public void CancelAiming()
    {
        isAiming = false;
        lineRenderer.enabled = false;

        if (gunPivot != null)
        {
            gunPivot.localRotation = defaultGunRotation;
        }
    }

    public GameObject ExecuteShot(Action<bool> onResolutionCallback = null)
    {
        isAiming = false;
        lineRenderer.enabled = false;

        // Отримуємо актуальний світовий кут у момент пострілу
        float worldAngleRad = gunPivot.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 shootVector = new Vector2(Mathf.Cos(worldAngleRad), Mathf.Sin(worldAngleRad)) * projectileSpeed;

        GameObject projectileInstance = ProjectilePoolManager.Instance.GetProjectile();

        if (projectileInstance != null && firePoint != null)
        {
            // БЕРЕМО З ПУЛУ ЗАМІСТЬ Instantiate
            projectileInstance.transform.position = firePoint.position;
            projectileInstance.transform.rotation = firePoint.rotation;
            projectileInstance.SetActive(true);

            Projectile projScript = projectileInstance.GetComponent<Projectile>();

            if (projScript != null)
            {
                projScript.Initialize(shootVector, damage, projectileSize, onResolutionCallback);
            }
        }

        if (gunPivot != null)
        {
            gunPivot.localRotation = defaultGunRotation;
        }

        return projectileInstance;
    }

    private void OscillateAngle()
    {
        float t = (Mathf.Sin(Time.time * aimSpeed) + 1f) / 2f;
        currentAngle = Mathf.Lerp(minAngle, maxAngle, t);
    }

    private void RotateGun()
    {
        if (gunPivot != null)
        {
            gunPivot.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }

    private void DrawTrajectory()
    {
        // Отримуємо світовий кут дула в градусах, а потім переводимо в радіани
        float worldAngleRad = gunPivot.eulerAngles.z * Mathf.Deg2Rad;

        Vector2 startPos = firePoint.position; // Використовуємо firePoint для точності
        Vector2 gravity = Physics2D.gravity;

        for (int i = 0; i < linePoints; i++)
        {
            float t = i * timeStep;

            // Використовуємо світовий кут (worldAngleRad)
            Vector2 point = startPos + new Vector2(
                projectileSpeed * Mathf.Cos(worldAngleRad) * t,
                projectileSpeed * Mathf.Sin(worldAngleRad) * t + 0.5f * gravity.y * t * t
            );

            lineRenderer.SetPosition(i, point);
        }
    }
}