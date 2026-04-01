using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FloatingHealthBar : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Посилання на компонент Health об'єкта")]
    public Health targetHealth;
    [Tooltip("Посилання на графічний компонент Slider")]
    public Slider healthSlider;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (targetHealth != null && healthSlider != null)
        {
            healthSlider.maxValue = targetHealth.maxHealth;
            UpdateVisibilityAndValue();
        }
    }

    // Метод викликається через UnityEvent з компонента Health
    public void OnHealthChanged()
    {
        UpdateVisibilityAndValue();
    }

    private void UpdateVisibilityAndValue()
    {
        if (targetHealth == null || healthSlider == null) return;

        // Оновлення математичного значення
        healthSlider.value = targetHealth.currentHealth;

        // Логіка видимості: приховування при повному здоров'ї або знищенні
        if (targetHealth.currentHealth >= targetHealth.maxHealth || targetHealth.currentHealth <= 0)
        {
            canvasGroup.alpha = 0f; // Повна прозорість
        }
        else
        {
            canvasGroup.alpha = 1f; // Повна видимість
        }
    }
}