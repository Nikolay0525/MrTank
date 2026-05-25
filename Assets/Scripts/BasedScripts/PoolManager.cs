using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private Dictionary<GameObject, ObjectPool<GameObject>> pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

        private Dictionary<GameObject, GameObject> instanceToPrefabMap = new Dictionary<GameObject, GameObject>();

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

        public void InitializePool(GameObject prefab, int defaultSize = 10, int maxSize = 100)
        {
            if (prefab == null || pools.ContainsKey(prefab)) return;

            ObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(prefab);
                    obj.transform.SetParent(transform);
                    return obj;
                },
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: true,
                defaultCapacity: defaultSize,
                maxSize: maxSize
            );

            pools.Add(prefab, newPool);
        }

        public GameObject GetObject(GameObject prefab)
        {
            if (prefab == null) return null;

            if (!pools.ContainsKey(prefab))
            {
                Debug.LogWarning($"[PoolManager] Pool for prefab '{prefab.name}' not found. Creating it dynamically.");
                InitializePool(prefab);
            }

            GameObject instance = pools[prefab].Get();
            instanceToPrefabMap[instance] = prefab;

            return instance;
        }

        public void ReturnObject(GameObject instance)
        {
            if (instance == null) return;

            if (instanceToPrefabMap.TryGetValue(instance, out GameObject originalPrefab))
            {
                if (pools.TryGetValue(originalPrefab, out ObjectPool<GameObject> pool))
                {
                    instance.transform.SetParent(this.transform);
                    pool.Release(instance);
                    return;
                }
            }

            Debug.LogWarning($"[PoolManager] Attempt to return an untracked object: '{instance.name}'. Destroying it.");
            Destroy(instance);
        }
    }
}