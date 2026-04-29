using System.Collections.Generic;
using UnityEngine;

public class RepairStationPoolManager : MonoBehaviour
{
    public static RepairStationPoolManager Instance { get; private set; }

    [Header("Pool Configuration")]
    public GameObject prefab;
    public int poolSize = 3; // We don't need many repair stations active at once

    private List<GameObject> pool;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
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

    public GameObject GetRepairStation()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        // Expand pool if we run out of objects
        GameObject newObj = Instantiate(prefab);
        newObj.transform.SetParent(transform);
        newObj.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }

    public void ReturnRepairStation(GameObject station)
    {
        station.SetActive(false);
        // Parent it back to the manager to keep the hierarchy clean
        station.transform.SetParent(transform);
    }
}