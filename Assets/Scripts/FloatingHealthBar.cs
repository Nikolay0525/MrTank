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

    public void OnHealthChanged()
    {
        UpdateVisibilityAndValue();
    }

    private void UpdateVisibilityAndValue()
    {
        if (targetHealth == null || healthSlider == null) return;

        healthSlider.value = targetHealth.currentHealth;

        if (targetHealth.currentHealth >= targetHealth.maxHealth || targetHealth.currentHealth <= 0)
        {
            canvasGroup.alpha = 0f; 
        }
        else
        {
            canvasGroup.alpha = 1f; 
        }
    }
}