using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FloatingHealthBar : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Health component")]
        public Health targetHealth;
        [Tooltip("Slider component")]
        public Slider healthSlider;

        [Header("Select HealthBar CanvasGroup")]
        public CanvasGroup canvasGroup;

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
}