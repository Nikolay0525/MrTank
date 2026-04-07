using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject garagePanel;  // Містить верхній бар і кнопку "To Battle"
    public GameObject gameHUDPanel; // Містить тільки рахунок під час гри
    public GameObject gameOverPanel; // Містить сірий квадрат з рахунком та кнопками
    public GameObject aboutUsPanel; // Містить сірий квадрат з рахунком та кнопками

    [Header("Game Over Text")]
    public TextMeshProUGUI finalScoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Показуємо тільки інтерфейс гаража
        ShowGarageUI();
    }

    public void ShowGarageUI()
    {
        garagePanel.SetActive(true);
        gameHUDPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        aboutUsPanel.SetActive(false);
    }

    // Підключаємо цей метод до зеленої кнопки "To Battle"
    //public void OnClickToBattle()
    //{
    //    garagePanel.SetActive(false);
    //    gameHUDPanel.SetActive(true);

    //    // Даємо команду танку почати рух
    //    TankController tank = FindObjectOfType<TankController>();
    //    if (tank != null)
    //    {
    //        tank.StartBattleFromGarage();
    //    }
    //}

    [Header("Лабораторна: Передача даних (Input)")]
    public TMP_InputField commanderInput;
    public TMP_InputField tankNameInput;

    [Header("Лабораторна: Передача даних (Output)")]
    public TextMeshProUGUI playerTagText;

    // Оновлюємо твій існуючий метод старту бою!
    public void OnClickToBattle()
    {
        // 1. Зчитуємо дані з першої "Активності" (Гаража)
        string commanderName = commanderInput.text;
        string tankName = tankNameInput.text;

        // Перевірка, якщо користувач нічого не ввів
        if (string.IsNullOrEmpty(commanderName)) commanderName = "Player 1";
        if (string.IsNullOrEmpty(tankName)) tankName = "KV-2";

        // 2. Передаємо дані на другу "Активність" (HUD)
        playerTagText.text = $"Commander: {commanderName}\nTank: {tankName}";

        // 3. Здійснюємо перехід (Аналог Intent)
        garagePanel.SetActive(false);
        gameHUDPanel.SetActive(true);

        // Запускаємо танк
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

    // Підключаємо до зеленої кнопки "Retry"
    public void OnClickRetry()
    {
        // Записуємо в "пам'ять" танка, що наступного разу треба стартувати одразу
        TankController.shouldAutoStart = true;

        // Перезавантажуємо сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Підключаємо до червоної кнопки "Garage" на екрані смерті
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