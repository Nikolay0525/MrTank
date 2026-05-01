using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class ObjectPool : MonoBehaviour
    {
        [Header("Pool Configuration")]
        public GameObject prefab;
        public int poolSize = 10;

        private List<GameObject> pool;

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            pool = new List<GameObject>();
            for (int i = 0; i < poolSize; i++)
            {
                CreateNewObject();
            }
        }

        private GameObject CreateNewObject()
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.SetParent(transform);
            obj.SetActive(false);
            pool.Add(obj);
            return obj;
        }

        public GameObject GetPooledObject()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].activeInHierarchy)
                {
                    return pool[i];
                }
            }

            return CreateNewObject();
        }

        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(transform);
        }
    }
}
