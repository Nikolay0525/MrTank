using UnityEngine;
using UnityEngine.UI; 

public class CombatTimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image timerImage;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0); // Offset above the tank

    private Transform targetTransform;
    private float maxTime;

    public void ShowTimer(Transform target, float timeToAim)
    {
        targetTransform = target;
        maxTime = timeToAim;

        // Ensure the image is fully filled at the start
        if (timerImage != null) timerImage.fillAmount = 1f;

        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void UpdateTimer(float currentTime)
    {
        if (timerImage != null && maxTime > 0)
        {
            // Calculate fill amount from 0 to 1
            timerImage.fillAmount = currentTime / maxTime;
        }
        UpdatePosition();
    }

    public void HideTimer()
    {
        targetTransform = null;
        gameObject.SetActive(false);
    }

    private void UpdatePosition()
    {
        if (targetTransform != null)
        {
            // Update position to follow the target with the specified offset
            transform.position = targetTransform.position + offset;
        }
    }
}