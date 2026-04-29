using UnityEngine;
using UnityEngine.UI; 

public class CombatTimerUI : MonoBehaviour
{
    [Header("UI References")]
    public Image timerFillImage; 
    public GameObject canvasObject; 

    private float maxTime;
    private float currentTime;
    private bool isCounting = false;

    private void Start()
    {
        // Ховаємо таймер на старті гри
        HideTimer();
    }

    private void Update()
    {
        if (isCounting)
        {
            currentTime -= Time.deltaTime;

            timerFillImage.fillAmount = currentTime / maxTime;

            if (currentTime <= 0)
            {
                HideTimer();
            }
        }
    }

    public void StartTimer(float duration)
    {
        maxTime = duration;
        currentTime = duration;
        timerFillImage.fillAmount = 1f;
        canvasObject.SetActive(true);
        isCounting = true;
    }

    public void HideTimer()
    {
        isCounting = false;
        if (canvasObject != null)
        {
            canvasObject.SetActive(false);
        }
    }
}