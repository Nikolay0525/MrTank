using Assets.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class EnemySpawnerManager : MonoBehaviour
    {
        public static EnemySpawnerManager Instance;

        [Header("Scene Configuration")]
        public SceneData currentSceneData;

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

            return PoolManager.Instance.GetObject(enemyPrefab);
        }

        public void ReturnEnemy(GameObject enemy)
        {
            PoolManager.Instance.ReturnObject(enemy);
        }
    }
}