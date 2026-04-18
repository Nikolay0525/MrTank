using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject garagePanel;  
    public GameObject gameHUDPanel; 
    public GameObject gameOverPanel; 
    public GameObject aboutUsPanel; 

    [Header("Game Over Text")]
    public TextMeshProUGUI finalScoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ShowGarageUI();
    }

    public void ShowGarageUI()
    {
        garagePanel.SetActive(true);
        gameHUDPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        aboutUsPanel.SetActive(false);
    }

    public void OnClickToBattle()
    {
        garagePanel.SetActive(false);
        gameHUDPanel.SetActive(true);

        TankController tank = FindObjectOfType<TankController>();
        if (tank != null)
        {
            tank.StartBattleFromGarage();
        }
    }


    public void ShowGameOver()
    {
        gameHUDPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        if (DifficultyManager.Instance != null)
        {
            finalScoreText.text = "Score: " + DifficultyManager.Instance.totalKills.ToString();
        }
    }

    public void OnClickRetry()
    {
        TankController.shouldAutoStart = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClickBackToGarageGO()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void OnClickBackToGarage()
    {
        garagePanel.SetActive(true);
        aboutUsPanel.SetActive(false);
    }
    
    public void OnClickAboutUs()
    {
        aboutUsPanel.SetActive(true);
        garagePanel.SetActive(false);
    }
}