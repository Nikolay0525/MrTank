using UnityEngine;

namespace Assets.Scripts
{
    public class VisualDeathHandler : MonoBehaviour
    {
        [Header("Tank Effects (Attached)")]
        public ParticleSystem fireEffect; 

        [Header("Ground Effects (Pooled)")]
        public Vector3 groundEffectOffset = new Vector3(0, -0.5f, 0);

        public void HandleDeathVisuals()
        {
            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in allRenderers)
            {
                if (sr != null) sr.color = Color.black;
            }

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }

            if (fireEffect != null) fireEffect.Play();

            if (DeathEffectPoolManager.Instance != null)
            {
                GameObject groundFx = DeathEffectPoolManager.Instance.GetDeathEffect();

                if (groundFx != null)
                {
                    groundFx.transform.position = transform.position + groundEffectOffset;

                    groundFx.transform.SetParent(transform.parent);

                    groundFx.SetActive(true);
                }
            }
        }

        public void ResetVisuals()
        {
            // Відновлюємо танк для наступного спавну
            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in allRenderers)
            {
                if (sr != null) sr.color = Color.white;
            }

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col in colliders)
            {
                col.enabled = true;
            }

            if (fireEffect != null)
            {
                fireEffect.Stop();
                fireEffect.Clear();
            }
        }
    }
}
