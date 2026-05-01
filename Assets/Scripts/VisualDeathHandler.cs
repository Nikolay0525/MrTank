using UnityEngine;

namespace Assets.Scripts
{
    public class VisualDeathHandler : MonoBehaviour
    {
        [Header("Death Effects")]
        public GameObject deathEffectPrefab;

        public Vector3 effectOffset = new Vector3(0, -0.5f, 0);

        private GameObject currentDeathEffect;

        public void HandleDeathVisuals()
        {
            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

            foreach (SpriteRenderer sr in allRenderers)
            {
                if (sr != null)
                {
                    sr.color = Color.black;
                }
            }

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }

            if (deathEffectPrefab != null && currentDeathEffect == null)
            {
                currentDeathEffect = Instantiate(deathEffectPrefab, transform.position + effectOffset, Quaternion.identity);
                currentDeathEffect.transform.SetParent(this.transform);
            }
        }

        public void ResetVisuals()
        {
            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in allRenderers)
            {
                if (sr != null)
                {
                    sr.color = Color.white;
                }
            }

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col in colliders)
            {
                col.enabled = true;
            }

            if (currentDeathEffect != null)
            {
                Destroy(currentDeathEffect);
            }
        }
    }
}
