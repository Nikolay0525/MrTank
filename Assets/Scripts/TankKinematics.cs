using UnityEngine;

public class TankKinematics : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Посилання на візуальний об'єкт корпусу")]
    public Transform bodyVisual;

    [Header("Параметри сканування")]
    public float raycastHeightOffset = 10f;
    public float raycastDistance = 30f;
    public LayerMask groundLayer;

    [Header("Параметри інтерполяції")]
    public float positionLerpSpeed = 15f;
    public float rotationLerpSpeed = 10f;
    public float tankHeightOffset = 0.5f;

    private void Update()
    {
        // Промінь випускаємо від кореня (Tank_logic)
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y + raycastHeightOffset);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, raycastDistance, groundLayer);

        if (hit.collider != null)
        {
            ApplyKinematics(hit);
        }
    }

    private void ApplyKinematics(RaycastHit2D hit)
    {
        // 1. Позиціонування кореня (Tank_logic)
        float targetY = hit.point.y + tankHeightOffset;
        Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);

        // 2. Обертання ТІЛЬКИ корпусу (bodyVisual)
        if (bodyVisual != null)
        {
            float targetAngle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            bodyVisual.rotation = Quaternion.Lerp(bodyVisual.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        }
    }
}