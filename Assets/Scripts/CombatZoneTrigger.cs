using UnityEngine;

public class CombatZoneTrigger : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask enemyLayer;

    private TankController playerController;

    private void Awake()
    {
        playerController = FindObjectOfType<TankController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            if (playerController != null)
            {
                EnemyAI enemyComponent = other.GetComponentInParent<EnemyAI>();

                if (enemyComponent != null)
                {
                    playerController.EnterCombatState(enemyComponent);
                }
                else
                {
                    Debug.LogError($"Об'єкт {other.name} знаходиться на шарі Enemy, але не містить компонента EnemyAI.");
                }
            }
        }
    }
}