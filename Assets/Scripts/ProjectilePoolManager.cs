using UnityEngine;

[RequireComponent(typeof(ObjectPool))]
public class ProjectilePoolManager : MonoBehaviour
{
    // Одинак (Singleton) тільки для цього конкретного менеджера
    public static ProjectilePoolManager Instance { get; private set; }

    // Посилання на базовий універсальний пул
    private ObjectPool pool;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Автоматично знаходимо ObjectPool на цьому ж об'єкті
            pool = GetComponent<ObjectPool>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Публічний метод для зручного доступу
    public GameObject GetProjectile()
    {
        return pool.GetPooledObject();
    }
}