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
        public GameObject TankSelectionPanel;
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

        [Header("Combat Timer")]
        public CombatTimerUI CombatTimer;

        private GameObject[] allPanels;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            allPanels = new GameObject[]
            {
                HomePanel, GaragePanel, TankSelectionPanel, InGamePanel, ScorePanel, GameOverPanel, PausePanel,
                StatsPanel, SettingsGeneral, SettingsAudio, SettingsGraphics
            };
        }

        private void Start()
        {
            OnSliderQualityChanged();

            TankController tank = FindAnyObjectByType<TankController>();

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

            TankController tank = FindAnyObjectByType<TankController>();
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
                FinalScoreText.text = "Score: " + DifficultyManager.Instance.TotalKills.ToString();

                StatsManager.Instance.UpdateBestScore(DifficultyManager.Instance.TotalKills);
            }
        }

        public void OnClickRetry()
        {
            Time.timeScale = 1f;
            LevelManager.Instance.RestartSession(true);

            ShowPanels(InGamePanel, ScorePanel);
            UpdateScoreText();
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
            LevelManager.Instance.RestartSession();
            OnClickGarage();
        }

        public void OnClickBackToGaragePause()
        {
            StatsManager.Instance.UpdateBestScore(DifficultyManager.Instance.TotalKills);
            Time.timeScale = 1f;
            LevelManager.Instance.RestartSession();
            OnClickGarage();
        }

        public void OnClickGarage()
        {
            CoinsText.text = StatsManager.Instance.GetCurrentCoins().ToString();
            StatsManager.Instance.UpdateBestScore(DifficultyManager.Instance.TotalKills);
            ShowPanels(GaragePanel, HomePanel);
        }

        public void OnClickTankSelection()
        {
            ShowPanels(TankSelectionPanel, HomePanel);
        }

        public void OnClickStats()
        {
            ShowPanels(StatsPanel, HomePanel);
        }

        public void OnClickSettings()
        {
            ShowPanels(SettingsGeneral, HomePanel);
        }

        public void OnClickAudioSettings()
        {
            ShowPanels(SettingsAudio, HomePanel);
        }

        public void OnClickGraphicsSettings()
        {
            ShowPanels(SettingsGraphics, HomePanel);
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