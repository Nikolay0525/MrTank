using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts
{
    [Serializable]
    public class PoolConfig
    {
        public PoolType poolType;
        public GameObject prefab;
        public int defaultSize = 10;
        public int maxSize = 100;
    }

    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Header("Pool Configurations")]
        public List<PoolConfig> poolConfigs;

        private Dictionary<PoolType, ObjectPool<GameObject>> pools;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializePools();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePools()
        {
            pools = new Dictionary<PoolType, ObjectPool<GameObject>>();

            foreach (var config in poolConfigs)
            {
                GameObject prefabToSpawn = config.prefab;

                ObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        GameObject obj = Instantiate(prefabToSpawn);
                        obj.transform.SetParent(transform);
                        return obj;
                    },
                    actionOnGet: (obj) => obj.SetActive(true),
                    actionOnRelease: (obj) => obj.SetActive(false),
                    actionOnDestroy: (obj) => Destroy(obj),
                    collectionCheck: true,
                    defaultCapacity: config.defaultSize,
                    maxSize: config.maxSize
                );

                pools.Add(config.poolType, newPool);
            }
        }


        public GameObject GetObject(PoolType poolType)
        {
            if (pools.TryGetValue(poolType, out ObjectPool<GameObject> pool))
            {
                return pool.Get();
            }

            Debug.LogError($"[PoolManager] Pool with name '{poolType}' not find!");
            return null;
        }

        public void ReturnObject(PoolType poolType, GameObject obj)
        {
            if (pools.TryGetValue(poolType, out ObjectPool<GameObject> pool))
            {
                obj.transform.SetParent(this.transform);

                pool.Release(obj);
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Attempt to return object into not existing pool: '{poolType}'. Object destroyed.");
                Destroy(obj);
            }
        }
    }
}