using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Current Settings")]
    public int currentFPS = 60;
    public bool isTrajectoryOptimized = false; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            LoadSettings();                
            ApplySettings();               
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void LoadSettings()
    {
        currentFPS = PlayerPrefs.GetInt("TargetFPS", 60);

        isTrajectoryOptimized = PlayerPrefs.GetInt("OptimizedTrajectory", 0) == 1;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("TargetFPS", currentFPS);
        PlayerPrefs.SetInt("OptimizedTrajectory", isTrajectoryOptimized ? 1 : 0);
        PlayerPrefs.Save(); 
    }


    private void ApplySettings()
    {
        Application.targetFrameRate = currentFPS;
    }

    public void SetFPS(int fps)
    {
        currentFPS = fps;
        ApplySettings();
        SaveSettings();
    }

    public void SetTrajectoryOptimization(bool isOptimized)
    {
        isTrajectoryOptimized = isOptimized;
        SaveSettings();
    }
}