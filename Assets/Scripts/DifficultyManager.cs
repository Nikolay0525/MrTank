using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Progression Stats")]
    public int totalKills = 0;
    [Tooltip("Скільки кілів потрібно для підняття одного рівня складності")]
    public int killsPerLevel = 1;

    [Header("Player Settings")]
    public float baseAimTime = 5f;
    public float minAimTime = 1.5f;
    public float aimReductionPerLevel = 0.2f;

    [Header("Enemy Settings")]
    public float baseHitChance = 0.15f;
    public float maxHitChance = 0.9f;
    public float hitChanceGainPerLevel = 0.07f;
    
    public float initialMissRadius = 7f;
    public float minMissRadius = 1.5f;
    public float radiusReductionPerLevel = 0.6f;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else Destroy(gameObject);
    }

    public void AddKill()
    {
        totalKills++;
    }

    public float GetDifficultyLevel()
    {
        return (float)totalKills / killsPerLevel;
    }


    public float GetPlayerAimTime()
    {
        float level = GetDifficultyLevel();
        float time = baseAimTime - (level * aimReductionPerLevel);
        return Mathf.Max(minAimTime, time);
    }

    public float GetEnemyHitChance(int shotsFiredInDuel)
    {
        float level = GetDifficultyLevel();
        float globalBonus = level * hitChanceGainPerLevel;
        float localBonus = shotsFiredInDuel * 0.1f; 
        
        return Mathf.Clamp(baseHitChance + globalBonus + localBonus, 0f, maxHitChance);
    }

    public float GetEnemyMissRadius(int shotsFiredInDuel)
    {
        float level = GetDifficultyLevel();
        float radius = initialMissRadius - (level * radiusReductionPerLevel);
        float localRadiusReduction = shotsFiredInDuel * 0.5f;

        return Mathf.Max(minMissRadius, radius - localRadiusReduction);
    }
}