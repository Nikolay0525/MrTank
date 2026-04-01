using UnityEngine;

public class VisualDeathHandler : MonoBehaviour
{
    // Метод підключається через UnityEvent OnDeath у компоненті Health
    public void HandleDeathVisuals()
    {
        // Збір усіх SpriteRenderer на поточному об'єкті та всіх його дочірніх об'єктах.
        // Параметр 'true' дозволяє знаходити навіть тимчасово деактивовані об'єкти.
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr != null)
            {
                sr.color = Color.black;
            }
        }

        // Відключення всіх колайдерів для усунення фізичної оболонки
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }
}