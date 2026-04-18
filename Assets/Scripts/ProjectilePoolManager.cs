using UnityEngine;

[RequireComponent(typeof(ObjectPool))]
public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance { get; private set; }

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

    public GameObject GetProjectile()
    {
        return pool.GetPooledObject();
    }
}