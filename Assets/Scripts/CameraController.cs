using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Посилання на Transform об'єкта танка")]
    public Transform target;
    [Tooltip("Вертикальне зміщення камери відносно цілі")]
    public float verticalOffset = 2f;

    [Header("Dynamics")]
    [Tooltip("Час затримки для згладжування (у секундах)")]
    public float smoothTime = 0.3f;

    // Внутрішні змінні для розрахунків
    private float currentVelocity;
    private float fixedX;
    private float fixedZ;

    private void Start()
    {
        // Фіксація стартових координат X та Z
        fixedX = transform.position.x;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            // Обчислення цільової координати Y
            float targetY = target.position.y + verticalOffset;

            // Математичне згладжування руху
            float smoothedY = Mathf.SmoothDamp(transform.position.y, targetY, ref currentVelocity, smoothTime);

            // Оновлення вектора позиції
            transform.position = new Vector3(fixedX, smoothedY, fixedZ);
        }
    }
}