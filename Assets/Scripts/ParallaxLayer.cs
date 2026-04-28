using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("0 = рухається як земля, 1 = стоїть на місці разом з небом")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0.5f;

    private Transform cameraTransform;
    private Vector3 startPosition;

    private void OnEnable()
    {
        // Знаходимо головну камеру
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Запам'ятовуємо точку, де ChunkManager заспавнив цей чанк
        startPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (cameraTransform != null)
        {
            // Вираховуємо, наскільки камера змістилася, і множимо на фактор паралаксу
            float distance = cameraTransform.position.x * parallaxFactor;

            // Рухаємо чанк за камерою тільки по осі X
            transform.position = new Vector3(startPosition.x + distance, transform.position.y, transform.position.z);
        }
    }
}