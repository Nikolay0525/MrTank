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

    private LineRenderer lineRenderer;
    private float currentAngle;
    private bool isAiming = false;
    private Quaternion defaultGunRotation;

    [Header("Spawning")]
    public GameObject projectilePrefab;
    public Transform firePoint; 

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = linePoints;
        lineRenderer.enabled = false;

        if (gunPivot != null)
        {
            // Запам'ятовуємо початкове положення гармати (прямо)
            defaultGunRotation = gunPivot.localRotation;
        }
    }

    private void Update()
    {
        if (isAiming)
        {
            OscillateAngle();
            DrawTrajectory();
            RotateGun(); // Новий метод
        }
    }

    public void StartAiming()
    {
        isAiming = true;
        lineRenderer.enabled = true;
    }

    public void ExecuteShot()
    {
        isAiming = false;
        lineRenderer.enabled = false;

        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector2 shootVector = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * projectileSpeed;

        // Інстанціювання снаряда та передача вектора швидкості
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile projScript = projectile.GetComponent<Projectile>();

            if (projScript != null)
            {
                projScript.Initialize(shootVector);
            }
        }

        // Повернення гармати у вихідне положення
        if (gunPivot != null) gunPivot.localRotation = defaultGunRotation;
    }

    private void OscillateAngle()
    {
        float t = (Mathf.Sin(Time.time * aimSpeed) + 1f) / 2f;
        currentAngle = Mathf.Lerp(minAngle, maxAngle, t);
    }

    // Новий метод для фізичного повороту гармати
    private void RotateGun()
    {
        if (gunPivot != null)
        {
            // Обертаємо локально відносно корпусу
            gunPivot.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }

    private void DrawTrajectory()
    {
        float angleRad = currentAngle * Mathf.Deg2Rad;
        // Траєкторія починається від гармати
        Vector2 startPos = (gunPivot != null) ? (Vector2)gunPivot.position : (Vector2)transform.position;
        Vector2 gravity = Physics2D.gravity;

        for (int i = 0; i < linePoints; i++)
        {
            float t = i * timeStep;
            Vector2 point = startPos + new Vector2(
                projectileSpeed * Mathf.Cos(angleRad) * t,
                projectileSpeed * Mathf.Sin(angleRad) * t + 0.5f * gravity.y * t * t
            );
            lineRenderer.SetPosition(i, point);
        }
    }
}