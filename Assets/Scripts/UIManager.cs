using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject HomePanel;
        public GameObject InGamePanel;
        public GameObject GaragePanel;
        public GameObject ScorePanel;
        public GameObject GameOverPanel;
        public GameObject PausePanel;
        public GameObject StatsPanel;

        [Header("Settings Panels")]
        public GameObject SettingsGeneral;
        public GameObject SettingsAudio;
        public GameObject SettingsGraphics;

        [Header("Current Score Text")]
        public TextMeshProUGUI CurrentScoreText;

        [Header("Game Over Text")]
        public TextMeshProUGUI FinalScoreText;

        [Header("Trajectory Quality Text")]
        public TextMeshProUGUI TrajectoryQualityText;

        [Header("Coins Text")]
        public TextMeshProUGUI CoinsText;

        private GameObject[] allPanels;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            allPanels = new GameObject[]
            {
                HomePanel, GaragePanel, InGamePanel, ScorePanel, GameOverPanel, PausePanel,
                StatsPanel, SettingsGeneral, SettingsAudio, SettingsGraphics
            };
        }

        private void Start()
        {
            OnSliderQualityChanged();

            TankController tank = FindObjectOfType<TankController>();

            if (tank != null && tank.currentState == TankController.TankState.Driving)
            {
                ShowPanels(InGamePanel, ScorePanel);
                UpdateScoreText();
            }
            else
            {
                OnClickGarage();
            }
        }

        private void ShowPanels(params GameObject[] activePanels)
        {
            foreach (var panel in allPanels)
            {
                if (panel == null) continue;

                bool shouldBeActive = System.Array.Exists(activePanels, p => p == panel);
                panel.SetActive(shouldBeActive);
            }
        }

        private void UpdateScoreText()
        {
            if (DifficultyManager.Instance != null)
            {
                CurrentScoreText.text = DifficultyManager.Instance.GetCurrentScore();
            }
        }

        public void OnClickToBattle()
        {
            ShowPanels(InGamePanel, ScorePanel);

            TankController tank = FindObjectOfType<TankController>();
            if (tank != null)
            {
                tank.StartBattleFromGarage();
            }

            UpdateScoreText();
        }

        public void ShowGameOver()
        {
            ShowPanels(GameOverPanel);

            if (DifficultyManager.Instance != null)
            {
                FinalScoreText.text = "Score: " + DifficultyManager.Instance.totalKills.ToString();

                StatsManager.Instance.UpdateBestScore(DifficultyManager.Instance.totalKills);
            }
        }

        public void OnClickRetry()
        {
            ShowPanels(InGamePanel, ScorePanel);

            TankController.shouldAutoStart = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OnClickPause()
        {
            ShowPanels(PausePanel, ScorePanel);
            Time.timeScale = 0f;
        }

        public void OnClickResume()
        {
            ShowPanels(InGamePanel, ScorePanel);
            Time.timeScale = 1f;
        }

        public void OnClickBackToGarageGO()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OnClickBackToGaragePause()
        {
            StatsManager.Instance.UpdateBestScore(DifficultyManager.Instance.totalKills);
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OnClickGarage()
        {
            CoinsText.text = StatsManager.Instance.GetCurrentCoins().ToString();
            StatsManager.Instance.UpdateBestScore(DifficultyManager.Instance.totalKills);
            ShowPanels(GaragePanel, HomePanel);
        }

        public void OnClickStats()
        {
            ShowPanels(StatsPanel);
        }

        public void OnClickSettings()
        {
            ShowPanels(SettingsGeneral);
        }

        public void OnClickAudioSettings()
        {
            ShowPanels(SettingsAudio);
        }

        public void OnClickGraphicsSettings()
        {
            ShowPanels(SettingsGraphics);
        }

        public void OnSliderQualityChanged()
        {
            if (SettingsManager.Instance != null)
            {
                TrajectoryQualityText.text = SettingsManager.Instance.trajectoryQuality.ToString("F2");
            }
            else
            {
                TrajectoryQualityText.text = "Can't get the value";
            }
        }
    }
}