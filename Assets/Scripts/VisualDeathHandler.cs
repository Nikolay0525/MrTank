using UnityEngine;

public class VisualDeathHandler : MonoBehaviour
{
    [Tooltip("Масив усіх спрайтів об'єкта, які потрібно затемнити")]
    public SpriteRenderer[] renderersToDarken;

    // Метод підключається через UnityEvent OnDeath у компоненті Health
    public void HandleDeathVisuals()
    {
        foreach (SpriteRenderer sr in renderersToDarken)
        {
            if (sr != null)
            {
                sr.color = Color.black;
            }
        }

        // Відключення колайдерів для запобігання подальшим колізіям
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }
}