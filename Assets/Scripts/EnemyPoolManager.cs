using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance;

    [System.Serializable]
    public class EnemySetup
    {
        public string enemyName;
        public GameObject prefab;
        public int initialPoolSize = 3;
    }

    [Header("Global Spawn Settings")]
    [Tooltip("Шанс появи ворога на новому чанку (0.75 = 75%)")]
    [Range(0f, 1f)] public float globalSpawnProbability = 0.75f;

    [Header("Enemy Types")]
    public List<EnemySetup> enemiesToPool;

    private Dictionary<GameObject, List<GameObject>> poolDictionary;

    private void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<GameObject, List<GameObject>>();

        foreach (var setup in enemiesToPool)
        {
            List<GameObject> objectPool = new List<GameObject>();
            for (int i = 0; i < setup.initialPoolSize; i++)
            {
                GameObject obj = Instantiate(setup.prefab, transform);
                obj.SetActive(false);
                objectPool.Add(obj);
            }
            poolDictionary.Add(setup.prefab, objectPool);
        }
    }

    public GameObject TryGetEnemy()
    {
        if (Random.value > globalSpawnProbability)
        {
            return null;
        }

        return GetRandomEnemy();
    }

    private GameObject GetRandomEnemy()
    {
        if (enemiesToPool.Count == 0)
        {
            Debug.LogWarning("EnemyPoolManager: Список ворогів порожній! Заповніть його в інспекторі.");
            return null;
        }

        int randomIndex = Random.Range(0, enemiesToPool.Count);
        GameObject selectedPrefab = enemiesToPool[randomIndex].prefab;

        List<GameObject> pool = poolDictionary[selectedPrefab];
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        GameObject newObj = Instantiate(selectedPrefab, transform);
        newObj.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }

    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        enemy.transform.SetParent(transform);
    }
}