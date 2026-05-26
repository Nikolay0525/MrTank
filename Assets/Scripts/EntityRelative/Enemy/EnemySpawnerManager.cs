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

            int randomIndex = Random.Range(0, currentSceneData.enemyPrefabs.Count);
            EnemyConfig selectedConfig = currentSceneData.enemyPrefabs[randomIndex];

            GameObject enemyPrefab = selectedConfig.gameObject;
            GameObject spawnedEnemy = PoolManager.Instance.GetObject(enemyPrefab);

            if (spawnedEnemy != null)
            {
                activeEnemies.Add(spawnedEnemy);
            }

            return spawnedEnemy;
        }

        public void ReturnEnemy(GameObject enemy)
        {
            activeEnemies.Remove(enemy);

            PoolManager.Instance.ReturnObject(enemy);
        }
    }
}