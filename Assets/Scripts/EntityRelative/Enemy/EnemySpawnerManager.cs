using Assets.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class EnemySpawnerManager : MonoBehaviour
    {
        public static EnemySpawnerManager Instance;

        [Header("Scene Configuration")]
        private SceneData currentSceneData;

        public HashSet<GameObject> activeEnemies = new HashSet<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            LevelManager.OnTankEquipped += InitializeWithNewData;
        }

        private void OnDestroy()
        {
            LevelManager.OnTankEquipped -= InitializeWithNewData;
        }

        private void InitializeWithNewData(SceneData newSceneData)
        {
            currentSceneData = newSceneData;

            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && enemy.activeInHierarchy)
                {
                    PoolManager.Instance.ReturnObject(enemy);
                }
            }
            activeEnemies.Clear();
        }

        public GameObject TryGetEnemy()
        {
            if (currentSceneData == null || currentSceneData.enemyPrefabs.Count == 0)
            {
                Debug.LogWarning("[EnemySpawnerManager] No enemies found in the current SceneData!");
                return null;
            }

            var currentDifficultyLevel = DifficultyManager.Instance.GetDifficultyLevel();

            float totalWeight = 0f;

            for (int i = 0; i < currentSceneData.enemyPrefabs.Count; i++)
            {
                EnemyConfig config = currentSceneData.enemyPrefabs[i];

                if (currentDifficultyLevel >= config.minDifficultyLevelToSpawn &&
                    currentDifficultyLevel <= config.maxDifficultyLevelToSpawn)
                {
                    totalWeight += config.spawnWeight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float accumulatedWeight = 0f;

            for (int i = 0; i < currentSceneData.enemyPrefabs.Count; i++)
            {
                EnemyConfig config = currentSceneData.enemyPrefabs[i];

                if (currentDifficultyLevel >= config.minDifficultyLevelToSpawn &&
                    currentDifficultyLevel <= config.maxDifficultyLevelToSpawn)
                {
                    accumulatedWeight += config.spawnWeight;

                    if (randomValue <= accumulatedWeight)
                    {
                        GameObject spawnedEnemy = PoolManager.Instance.GetObject(config.gameObject);

                        if (spawnedEnemy != null)
                        {
                            activeEnemies.Add(spawnedEnemy);
                        }

                        return spawnedEnemy;
                    }
                }
            }

            return null;
        }

        public void ReturnEnemy(GameObject enemy)
        {
            activeEnemies.Remove(enemy);

            PoolManager.Instance.ReturnObject(enemy);
        }
    }
}