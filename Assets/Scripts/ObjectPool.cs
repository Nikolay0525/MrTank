using System.Collections.Generic;
using UnityEngine;

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
            GameObject obj = Instantiate(prefab);
            obj.transform.SetParent(transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
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

        GameObject newObj = Instantiate(prefab);
        newObj.transform.SetParent(transform);
        newObj.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }
}