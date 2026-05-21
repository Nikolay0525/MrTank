using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class EnemySpawnerManager : MonoBehaviour
    {
        public static EnemySpawnerManager Instance;

        [Header("Enemy Types Configuration")]
        public List<PoolType> enemyPoolTypes;

        private Dictionary<GameObject, PoolType> activeEnemiesMap = new Dictionary<GameObject, PoolType>();

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
            return GetRandomEnemy();
        }

        private GameObject GetRandomEnemy()
        {
            if (enemyPoolTypes == null || enemyPoolTypes.Count == 0)
            {
                Debug.LogWarning("[EnemySpawnerManager] Enemy pool list is empty! Assign types in the Inspector.");
                return null;
            }

            int randomIndex = Random.Range(0, enemyPoolTypes.Count);
            PoolType selectedPoolType = enemyPoolTypes[randomIndex];

            GameObject newEnemy = PoolManager.Instance.GetObject(selectedPoolType);

            if (newEnemy != null)
            {
                activeEnemiesMap[newEnemy] = selectedPoolType;
            }

            return newEnemy;
        }

        public void ReturnEnemy(GameObject enemy)
        {
            if (activeEnemiesMap.TryGetValue(enemy, out PoolType poolType))
            {
                PoolManager.Instance.ReturnObject(poolType, enemy);

                activeEnemiesMap.Remove(enemy);
            }
            else
            {
                Debug.LogWarning($"[EnemySpawnerManager] Trying to return an unknown enemy: {enemy.name} not found in dictionary. Destroying it.");
                Destroy(enemy);
            }
        }
    }
}