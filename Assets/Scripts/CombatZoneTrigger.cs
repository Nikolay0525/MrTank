using UnityEngine;

public class CombatZoneTrigger : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask enemyLayer;

    // Посилання на контролер танка гравця
    private TankController playerController;

    private void Awake()
    {
        playerController = FindObjectOfType<TankController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Перевірка належності об'єкта до шару ворогів за допомогою бітової маски
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            if (playerController != null)
            {
                // Пошук компонента EnemyAI на об'єкті колізії або його батьківських об'єктах
                EnemyAI enemyComponent = other.GetComponentInParent<EnemyAI>();

                if (enemyComponent != null)
                {
                    // Делегування об'єкта противника у кінцевий автомат гравця
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