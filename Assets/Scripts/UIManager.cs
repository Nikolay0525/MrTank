using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Mathematics;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject GaragePanel;
    public GameObject InGamePanel;
    public GameObject ScorePanel;
    public GameObject GameOverPanel;
    public GameObject GarageHelpPanel;
    public GameObject DrivingHelpPanel;
    public GameObject PausePanel;
    public GameObject SettingsGeneral;
    public GameObject SettingsAudio;
    public GameObject SettingsGraphics;

    [Header("Current Score Text")]
    public TextMeshProUGUI CurrentScoreText;

    [Header("Game Over Text")]
    public TextMeshProUGUI FinalScoreText;

    [Header("Trajectory Quality Text")]
    public TextMeshProUGUI TrajectoryQualityText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        OnClickGarage();
        OnSliderQualityChanged();
    }

    public void OnClickToBattle()
    {
        
        GaragePanel.SetActive(false);
        InGamePanel.SetActive(true);
        ScorePanel.SetActive(true);
        GameOverPanel.SetActive(false);
        GarageHelpPanel.SetActive(false);
        DrivingHelpPanel.SetActive(false);
        PausePanel.SetActive(false);

        SettingsGeneral.SetActive(false);
        SettingsAudio.SetActive(false);
        SettingsGraphics.SetActive(false);

        TankController tank = FindObjectOfType<TankController>();
        if (tank != null)
        {
            tank.StartBattleFromGarage();
        }

        if(DifficultyManager.Instance != null)
        {
            CurrentScoreText.text = DifficultyManager.Instance.GetCurrentScore();
        }
    }


    public void ShowGameOver()
    {
        GaragePanel.SetActive(false);
        InGamePanel.SetActive(false);
        ScorePanel.SetActive(false);
        GameOverPanel.SetActive(true);
        GarageHelpPanel.SetActive(false);
        DrivingHelpPanel.SetActive(false);
        PausePanel.SetActive(false);

        SettingsGeneral.SetActive(false);
        SettingsAudio.SetActive(false);
        SettingsGraphics.SetActive(false);

        if (DifficultyManager.Instance != null)
        {
            FinalScoreText.text = "Score: " + DifficultyManager.Instance.totalKills.ToString();
        }
    }

    public void OnClickRetry()
    {
        TankController.shouldAutoStart = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void OnClickPause()
    {
        GaragePanel.SetActive(false);
        InGamePanel.SetActive(false);
        ScorePanel.SetActive(true);
        GameOverPanel.SetActive(false);
        GarageHelpPanel.SetActive(false);
        DrivingHelpPanel.SetActive(false);
        PausePanel.SetActive(true);

        SettingsGeneral.SetActive(false);
        SettingsAudio.SetActive(false);
        SettingsGraphics.SetActive(false);

        Time.timeScale = 0f;
    }

    public void OnClickResume()
    {
        GaragePanel.SetActive(false);
        InGamePanel.SetActive(true);
        ScorePanel.SetActive(true);
        GameOverPanel.SetActive(false);
        GarageHelpPanel.SetActive(false);
        DrivingHelpPanel.SetActive(false);
        PausePanel.SetActive(false);

        SettingsGeneral.SetActive(false);
        SettingsAudio.SetActive(false);
        SettingsGraphics.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OnClickBackToGarageGO()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClickBackToGaragePause()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        Time.timeScale = 1f;
    }

    public void OnClickGarage()
    {
        GaragePanel.SetActive(true);
        InGamePanel.SetActive(false);
        ScorePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        GarageHelpPanel.SetActive(false);
        DrivingHelpPanel.SetActive(false);
        PausePanel.SetActive(false);

        SettingsGeneral.SetActive(false);
        SettingsAudio.SetActive(false);
        SettingsGraphics.SetActive(false);
    }

    public void OnClickGarageHelp()
    {
        GaragePanel.SetActive(false);
        InGamePanel.SetActive(false);
        ScorePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        GarageHelpPanel.SetActive(true);
        DrivingHelpPanel.SetActive(false);
        PausePanel.SetActive(false);

        SettingsGeneral.SetActive(false);
        SettingsAudio.SetActive(false);
        SettingsGraphics.SetActive(false);
    }
    public void OnClickDrivingHelp()
    {
        GaragePanel.SetActive(false);
        InGamePanel.SetActive(false);
        ScorePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        GarageHelpPanel.SetActive(false);
        DrivingHelpPanel.SetActive(true);
        PausePanel.SetActive(false);

        SettingsGeneral.SetActive(false);
        SettingsAudio.SetActive(false);
        SettingsGraphics.SetActive(false);
    }

    public void OnClickSettings()
    {
        GaragePanel.SetActive(false);
        InGamePanel.SetActive(false);
        ScorePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        GarageHelpPanel.SetActive(false);
        DrivingHelpPanel.SetActive(false);
        PausePanel.SetActive(false);

        SettingsGeneral.SetActive(true);
        SettingsAudio.SetActive(false);
        SettingsGraphics.SetActive(false);
    }
    public void OnClickAudioSettings()
    {
        GaragePanel.SetActive(false);
        InGamePanel.SetActive(false);
        ScorePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        GarageHelpPanel.SetActive(false);
        DrivingHelpPanel.SetActive(false);
        PausePanel.SetActive(false);

        SettingsGeneral.SetActive(false);
        SettingsAudio.SetActive(true);
        SettingsGraphics.SetActive(false);
    }

    public void OnClickGraphicsSettings()
    {
        GaragePanel.SetActive(false);
        InGamePanel.SetActive(false);
        ScorePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        GarageHelpPanel.SetActive(false);
        DrivingHelpPanel.SetActive(false);
        PausePanel.SetActive(false);

        SettingsGeneral.SetActive(false);
        SettingsAudio.SetActive(false);
        SettingsGraphics.SetActive(true);
    }

    public void OnSliderQualityChanged()
    {
        if(SettingsManager.Instance != null)
        {
            TrajectoryQualityText.text = SettingsManager.Instance.trajectoryQuality.ToString("F2");
            return;
        }
        TrajectoryQualityText.text = "Can't get the value";
    }
}