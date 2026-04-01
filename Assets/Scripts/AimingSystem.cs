using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimingSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Посилання на точку обертання гармати")]
    public Transform gunPivot;

    [Header("Ballistics")]
    public float projectileSpeed = 15f;
    public float minAngle = 15f;
    public float maxAngle = 75f;
    public float aimSpeed = 4f;

    [Header("Trajectory Rendering")]
    public int linePoints = 30;
    public float timeStep = 0.05f;

    [Header("Spawning")]
    public GameObject projectilePrefab;
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

    // Змінено тип повернення з void на GameObject для відстеження життєвого циклу снаряда
    public GameObject ExecuteShot(Action<bool> onResolutionCallback = null)
    {
        isAiming = false;
        lineRenderer.enabled = false;

        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector2 shootVector = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * projectileSpeed;

        GameObject projectileInstance = null;

        if (projectilePrefab != null && firePoint != null)
        {
            projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile projScript = projectileInstance.GetComponent<Projectile>();

            if (projScript != null)
            {
                // Передача вектора швидкості та функції зворотного виклику
                projScript.Initialize(shootVector, onResolutionCallback);
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
        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector2 startPos = (gunPivot != null) ? (Vector2)gunPivot.position : (Vector2)transform.position;
        Vector2 gravity = Physics2D.gravity;

        for (int i = 0; i < linePoints; i++)
        {
            float t = i * timeStep;

            // Рівняння балістичної кривої
            Vector2 point = startPos + new Vector2(
                projectileSpeed * Mathf.Cos(angleRad) * t,
                projectileSpeed * Mathf.Sin(angleRad) * t + 0.5f * gravity.y * t * t
            );

            lineRenderer.SetPosition(i, point);
        }
    }
}