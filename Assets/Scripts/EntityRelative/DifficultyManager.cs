using UnityEngine;

namespace Assets.Scripts
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        [Header("Progression Stats")]
        public int TotalKills = 0;
        public int TotalKillstreak = 0;
        public int EnemiesPassedSinceLastStation = 999;
        [Tooltip("Kills per level")]
        public int KillsPerLevel = 1;

        [Header("Player Settings")]
        public float BaseAimTime = 5f;
        public float MinAimTime = 1.5f;
        public float AimReductionPerLevel = 0.2f;

        [Header("Enemy Settings")]
        public float BaseHitChance = 0.15f;
        public float MaxHitChance = 0.9f;
        public float HitChanceGainPerLevel = 0.07f;

        public float InitialMissRadius = 7f;
        public float MinMissRadius = 1.5f;
        public float RadiusReductionPerLevel = 0.6f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(gameObject);
        }

        public void AddKill()
        {
            TotalKills++;
            TotalKillstreak++;
            StatsManager.Instance.AddKill();
            StatsManager.Instance.SetNewKillStreakScore(TotalKillstreak);
            if (TotalKillstreak > 1)
            {
                StatsManager.Instance.AddCoins(TotalKillstreak);
            }
            else StatsManager.Instance.AddCoins(1);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.CurrentScoreText.text = "Score: " + TotalKills.ToString();
            }
        }

        public void ResetKillStreak()
        {
            TotalKillstreak = 0;
        }

        public float GetDifficultyLevel()
        {
            return (float)TotalKills / KillsPerLevel;
        }

        public string GetCurrentScore()
        {
            return UIManager.Instance.CurrentScoreText.text = "Score: " + TotalKills.ToString();
        }

        public float GetPlayerAimTime()
        {
            float level = GetDifficultyLevel();
            float time = BaseAimTime - (level * AimReductionPerLevel);
            return Mathf.Max(MinAimTime, time);
        }

        public float GetEnemyHitChance(int shotsFiredInDuel)
        {
            float level = GetDifficultyLevel();
            float globalBonus = level * HitChanceGainPerLevel;
            float localBonus = shotsFiredInDuel * 0.1f;

            return Mathf.Clamp(BaseHitChance + globalBonus + localBonus, 0f, MaxHitChance);
        }

        public float GetEnemyMissRadius(int shotsFiredInDuel)
        {
            float level = GetDifficultyLevel();
            float radius = InitialMissRadius - (level * RadiusReductionPerLevel);
            float localRadiusReduction = shotsFiredInDuel * 0.5f;

            return Mathf.Max(MinMissRadius, radius - localRadiusReduction);
        }

        public void ResetStationCounter()
        {
            EnemiesPassedSinceLastStation = 0;
        }

        public void ResetDifficulty()
        {
            Instance.TotalKills = 0;
            Instance.TotalKillstreak = 0;
            Instance.EnemiesPassedSinceLastStation = 0;
        }
    }
}
