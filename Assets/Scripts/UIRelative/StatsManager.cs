using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class StatsManager : MonoBehaviour
    {
        [System.Serializable]
        public class PlayerStats
        {
            public int TotalKills;
            public int maxKillStreak;
            public int bestScore;
            public int coins;

            public List<int> unlockedTankIDs = new List<int> { 0 };
            public int selectedTankID = 0;
        }

        public static StatsManager Instance { get; private set; }

        public PlayerStats currentStats;
        private string saveFilePath;

        private string encryptionKey = "prikol";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            saveFilePath = Path.Combine(Application.persistentDataPath, "player_stats.json");
            LoadStats();
        }
        private void LoadStats()
        {
            if (File.Exists(saveFilePath))
            {
                string encryptedJson = File.ReadAllText(saveFilePath);

                string decryptedJson = XorCipher(encryptedJson, encryptionKey);

                currentStats = JsonUtility.FromJson<PlayerStats>(decryptedJson);
            }
            else
            {
                currentStats = new PlayerStats();
            }
        }

        private string XorCipher(string data, string key)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                result.Append((char)(data[i] ^ key[i % key.Length]));
            }
            return result.ToString();
        }
        public void SaveStats()
        {
            string json = JsonUtility.ToJson(currentStats, true);

            string encryptedJson = XorCipher(json, encryptionKey);

            File.WriteAllText(saveFilePath, encryptedJson);
        }

        public void AddKill()
        {
            currentStats.TotalKills++;
            SaveStats();
        }

        public void SetNewKillStreakScore(int newKillStreak)
        {
            if (newKillStreak > currentStats.maxKillStreak)
            {
                currentStats.maxKillStreak = newKillStreak;
                SaveStats();
            }
        }

        public void UpdateBestScore(int newScore)
        {
            if (newScore > currentStats.bestScore)
            {
                currentStats.bestScore = newScore;
                SaveStats();
            }
        }
        public void AddCoins(int amount)
        {
            currentStats.coins += amount;
            SaveStats();
        }

        public int GetCurrentCoins()
        {
            return currentStats.coins;
        }
    }
}
