using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(ObjectPool))]
    public class DeathEffectPoolManager : MonoBehaviour
    {
        public static DeathEffectPoolManager Instance { get; private set; }

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

        public GameObject GetDeathEffect()
        {
            return pool.GetPooledObject();
        }

        public void ReturnDeathEffect(GameObject effect)
        {
            pool.ReturnObject(effect);
        }
    }
}
