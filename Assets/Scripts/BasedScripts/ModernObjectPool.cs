using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts
{
    public class ModernObjectPool : MonoBehaviour
    {
        [Header("Pool Configuration")]
        public GameObject prefab;
        public int defaultSize = 10;
        public int maxSize = 100;

        private ObjectPool<GameObject> pool;

        private void Awake()
        {
            pool = new ObjectPool<GameObject>(
                createFunc: CreateNewObject,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnToPool,
                actionOnDestroy: OnDestroyObject,
                collectionCheck: true,
                defaultCapacity: defaultSize,
                maxSize: maxSize
            );
        }

        private GameObject CreateNewObject()
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.SetParent(transform);
            return obj;
        }

        private void OnTakeFromPool(GameObject obj) => obj.SetActive(true);
        private void OnReturnToPool(GameObject obj) => obj.SetActive(false);
        private void OnDestroyObject(GameObject obj) => Destroy(obj);

        public GameObject GetPooledObject()
        {
            return pool.Get();
        }

        public void ReturnObject(GameObject obj)
        {
            pool.Release(obj);
        }
    }
}