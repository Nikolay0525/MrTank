using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Посилання на компонент здоров'я цільового об'єкта")]
    public Health targetHealth;
    [Tooltip("Посилання на графічний компонент Slider")]
    public Slider healthSlider;

    private void Start()
    {
        InitializeSlider();
    }

    private void InitializeSlider()
    {
        if (targetHealth != null && healthSlider != null)
        {
            // Синхронізація математичних меж слайдера з параметрами здоров'я
            healthSlider.maxValue = targetHealth.maxHealth;
            healthSlider.value = targetHealth.currentHealth;
        }
    }

    // Метод викликається через систему подій UnityEvent
    public void UpdateHealthBar()
    {
        if (targetHealth != null && healthSlider != null)
        {
            healthSlider.value = targetHealth.currentHealth;
        }
    }
}