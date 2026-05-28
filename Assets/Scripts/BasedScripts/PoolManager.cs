using Assets.ScriptableObjects;
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

        public void PrewarmSceneData(SceneData newSceneData)
        {
            if (newSceneData == null) return;

            if (newSceneData.playerTankPrefab != null)
            {
                AimingSystem aimingSystem = newSceneData.playerTankPrefab.GetComponent<AimingSystem>();
                GameObject projectile = aimingSystem.projectilePrefab;
                if (projectile != null)
                {
                    InitializePool(projectile, 1, 5);
                }
            }

            if (newSceneData.enemyHitEffectPrefab != null)
            {
                InitializePool(newSceneData.enemyHitEffectPrefab.gameObject, 2, 10);
            }

            if (newSceneData.groundHitEffectPrefab != null)
            {
                InitializePool(newSceneData.groundHitEffectPrefab.gameObject, 2, 10);
            }

            if (newSceneData.layerPrefabs != null)
            {
                foreach (var layer in newSceneData.layerPrefabs)
                {
                    InitializePool(layer.gameObject, 4, 20);
                }
            }

            if (newSceneData.enemyPrefabs != null)
            {
                foreach (var enemyConfig in newSceneData.enemyPrefabs)
                {
                    InitializePool(enemyConfig.gameObject, 5, 25);
                }
            }

            if (newSceneData.sceneryPrefabs != null)
            {
                foreach (var scenery in newSceneData.sceneryPrefabs)
                {
                    InitializePool(scenery.gameObject, 20, 100);
                }
            }
        }

        public void CleanupOldPools(SceneData newSceneData)
        {
            if (newSceneData == null || pools.Count == 0) return;

            HashSet<GameObject> requiredPrefabs = new HashSet<GameObject>();

            if (newSceneData.playerTankPrefab != null)
            {
                AimingSystem aimingSystem = newSceneData.playerTankPrefab.GetComponent<AimingSystem>();
                if (aimingSystem != null && aimingSystem.projectilePrefab != null)
                {
                    requiredPrefabs.Add(aimingSystem.projectilePrefab);
                }
            }

            if (newSceneData.enemyHitEffectPrefab != null)
            {
                requiredPrefabs.Add(newSceneData.enemyHitEffectPrefab.gameObject);
            }

            if (newSceneData.groundHitEffectPrefab != null)
            {
                requiredPrefabs.Add(newSceneData.groundHitEffectPrefab.gameObject);
            }

            if (newSceneData.layerPrefabs != null)
            {
                foreach (var layer in newSceneData.layerPrefabs) requiredPrefabs.Add(layer.gameObject);
            }

            if (newSceneData.enemyPrefabs != null)
            {
                foreach (var enemyConfig in newSceneData.enemyPrefabs) requiredPrefabs.Add(enemyConfig.gameObject);
            }

            if (newSceneData.sceneryPrefabs != null)
            {
                foreach (var scenery in newSceneData.sceneryPrefabs) requiredPrefabs.Add(scenery.gameObject);
            }

            RemoveOldPools(requiredPrefabs);
        }

        private void RemoveOldPools(HashSet<GameObject> requiredPrefabs)
        {
            List<GameObject> existingKeys = new List<GameObject>(pools.Keys);

            foreach (var existingPrefab in existingKeys)
            {
                if (!requiredPrefabs.Contains(existingPrefab))
                {
                    pools[existingPrefab].Dispose();
                    pools.Remove(existingPrefab);
                }
            }

            List<GameObject> existingInstances = new List<GameObject>(instanceToPrefabMap.Keys);
            foreach (var instance in existingInstances)
            {
                if (!requiredPrefabs.Contains(instanceToPrefabMap[instance]))
                {
                    instanceToPrefabMap.Remove(instance);
                }
            }
        }

        private void InitializePool(GameObject prefab, int defaultSize = 10, int maxSize = 100)
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