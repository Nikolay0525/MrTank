using UnityEngine;

public class ScrollableObject : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Глобальная координата X, при пересечении которой объект деактивируется")]
    public float despawnX = -15f;

    private void Update()
    {
        // Вычисление вектора смещения на основе глобальной скорости конечного автомата
        float currentSpeed = TankController.CurrentGlobalSpeed;

        // Применение трансформации позиции
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);

        // Проверка условия выхода за пределы области рендеринга камеры
        if (transform.position.x <= despawnX)
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        // Деактивация объекта для последующего использования в Object Pool
        gameObject.SetActive(false);
    }
}