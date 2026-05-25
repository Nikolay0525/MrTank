using UnityEngine;

namespace Assets.Scripts
{
    public class CombatZoneTrigger : MonoBehaviour
    {
        [Header("Settings")]
        public LayerMask enemyLayer;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & enemyLayer) != 0)
            {
                if (TankController.Instance != null)
                {
                    EnemyAI enemyComponent = other.GetComponentInParent<EnemyAI>();

                    if (enemyComponent != null)
                    {
                        TankController.Instance.EnterCombatState(enemyComponent);
                    }
                    else
                    {
                        Debug.LogError($"[CombatZoneTrigger] The {other.name} object is on the Enemy layer, but does not contain an EnemyAI component.");
                    }
                }
            }
        }
    }
}
