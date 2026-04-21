using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("UI References")]
    public TMP_Dropdown fpsDropdown; 
    public Slider trajectorySlider;

    [Header("Current Settings")]
    public int currentFPS = 60;
    public float trajectoryQuality = 0.1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadSettings();
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (fpsDropdown != null)
        {
            if (currentFPS == 60) fpsDropdown.value = 0;
            else if (currentFPS == 120) fpsDropdown.value = 1;
            else if (currentFPS == 240) fpsDropdown.value = 2;
            else fpsDropdown.value = 1;

            fpsDropdown.onValueChanged.AddListener(SetFPSFromDropdown);
        }

        if (trajectorySlider != null)
        {
            trajectorySlider.value = trajectoryQuality;
            trajectorySlider.onValueChanged.AddListener(SetTrajectoryQuality);
        }

        ApplyGlobalSettings();
    }
    public void SetFPSFromDropdown(int index)
    {
        switch (index)
        {
            case 0: currentFPS = 60; break;
            case 1: currentFPS = 120; break;
            case 2: currentFPS = 240; break;
        }

        Application.targetFrameRate = currentFPS; 
        PlayerPrefs.SetInt("Settings_FPS", currentFPS); 
    }

    public void SetTrajectoryQuality(float value)
    {
        float min = trajectorySlider.minValue;
        float max = trajectorySlider.maxValue;

        float deadZone = (max - min) * 0.05f;

        if (value >= max - deadZone)
        {
            value = max; 
        }
        else if (value <= min + deadZone)
        {
            value = min; 
        }

        trajectoryQuality = value;

        trajectorySlider.SetValueWithoutNotify(value);

        PlayerPrefs.SetFloat("Settings_Trajectory", trajectoryQuality);
    }

    private void LoadSettings()
    {
        currentFPS = PlayerPrefs.GetInt("Settings_FPS", 60);
        trajectoryQuality = PlayerPrefs.GetFloat("Settings_Trajectory", 0.1f);
    }

    private void ApplyGlobalSettings()
    {
        Application.targetFrameRate = currentFPS;
    }
}