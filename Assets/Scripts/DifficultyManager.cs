using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Global Progression")]
    [Tooltip("Кількість метрів, яка вважається одним 'рівнем' складності")]
    public float distancePerLevel = 60f;

    [Header("Player Aim Time")]
    public float baseAimTime = 5f;
    public float minAimTime = 2f;
    [Tooltip("На скільки секунд зменшується час прицілювання за кожен 'рівень'")]
    public float aimTimeReductionPerLevel = 0.1f;

    [Header("Enemy Accuracy")]
    public float baseHitChance = 0.1f; // 10% базово
    public float maxHitChance = 0.9f;  // 90% максимум
    [Tooltip("На скільки зростає точність за кожен 'рівень' дистанції")]
    public float globalAccuracyBonusPerLevel = 0.05f;
    [Tooltip("На скільки зростає точність за кожен постріл у поточній дуелі")]
    public float localSightingBonus = 0.1f;

    [Header("Enemy Miss Radius")]
    public float initialMissRadius = 8f;
    public float minMissRadius = 2f;
    [Tooltip("На скільки метрів зменшується розкид за кожен 'рівень' дистанції")]
    public float radiusReductionPerLevel = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Допоміжний метод: рахує "рівень" складності на основі проїханих чанків
    private float GetCurrentDifficultyLevel()
    {
        return ChunkManager.TotalDistanceTraveled / distancePerLevel;
    }

    // ФОРМУЛА 1: Час прицілювання гравця
    public float CalculatePlayerAimTime()
    {
        float level = GetCurrentDifficultyLevel();
        float calculatedTime = baseAimTime - (level * aimTimeReductionPerLevel);
        return Mathf.Max(minAimTime, calculatedTime);
    }

    // ФОРМУЛА 2: Точність ворога (з урахуванням глобального досвіду та локальної пристрілки)
    public float CalculateEnemyHitChance(int localShotsFired)
    {
        float level = GetCurrentDifficultyLevel();
        float globalBonus = level * globalAccuracyBonusPerLevel;
        float localBonus = localShotsFired * localSightingBonus;

        return Mathf.Clamp(baseHitChance + globalBonus + localBonus, 0f, maxHitChance);
    }

    // ФОРМУЛА 3: Радіус промаху ворога
    public float CalculateEnemyMissRadius()
    {
        float level = GetCurrentDifficultyLevel();
        float calculatedRadius = initialMissRadius - (level * radiusReductionPerLevel);
        return Mathf.Max(minMissRadius, calculatedRadius);
    }
}