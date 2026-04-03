using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Current Settings")]
    public int currentFPS = 60;
    public bool isTrajectoryOptimized = false; // Твоя майбутня оптимізація прицілу

    private void Awake()
    {
        // Робимо його класичним Singleton, який не знищується при переході між сценами
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Цей об'єкт переходитиме з меню в гру
            LoadSettings();                // Завантажуємо збережені налаштування
            ApplySettings();               // Застосовуємо їх до рушія
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- МЕТОДИ ДЛЯ ЗБЕРЕЖЕННЯ ТА ЗАВАНТАЖЕННЯ (PlayerPrefs) ---

    private void LoadSettings()
    {
        // Якщо налаштування ще ніколи не зберігалися, ставимо 60 за замовчуванням
        currentFPS = PlayerPrefs.GetInt("TargetFPS", 60);

        // PlayerPrefs не підтримує bool, тому зберігаємо як 1 (true) або 0 (false)
        isTrajectoryOptimized = PlayerPrefs.GetInt("OptimizedTrajectory", 0) == 1;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("TargetFPS", currentFPS);
        PlayerPrefs.SetInt("OptimizedTrajectory", isTrajectoryOptimized ? 1 : 0);
        PlayerPrefs.Save(); // Примусово записуємо в пам'ять телефону
    }

    // --- МЕТОДИ ДЛЯ ЗАСТОСУВАННЯ НАЛАШТУВАНЬ ---

    private void ApplySettings()
    {
        Application.targetFrameRate = currentFPS;
        // Тут можна додати інші глобальні налаштування, наприклад VSync:
        // QualitySettings.vSyncCount = 0; 
    }

    // --- ПУБЛІЧНІ МЕТОДИ ДЛЯ ТВОГО UI МЕНЮ ---
    // Саме ці методи ти будеш викликати з кнопок чи Dropdown у Canvas

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
        // Твій AimingSystem зможе читати цю змінну:
        // if (SettingsManager.Instance.isTrajectoryOptimized) { малюємо простішу лінію }
    }
}