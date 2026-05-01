using UnityEngine;

namespace Assets.Scripts
{

    [RequireComponent(typeof(ObjectPool))]
    public class RepairStationPoolManager : MonoBehaviour
    {
        public static RepairStationPoolManager Instance { get; private set; }

        private ObjectPool pool;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                pool = GetComponent<ObjectPool>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public GameObject GetRepairStation()
        {
            return pool.GetPooledObject();
        }

        public void ReturnRepairStation(GameObject station)
        {
            pool.ReturnObject(station);
        }
    }
}