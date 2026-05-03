using UnityEngine;

namespace Assets.Scripts
{
    public class VisualDeathHandler : MonoBehaviour
    {
        [Header("Effect Parameters")]
        public float baseExplosionSize = 1.0f;
        public Vector3 baseStainSize = new Vector3(3f, 6f, 1);

        [Header("Tank Effects (Attached)")]
        public ParticleSystem fireEffect;

        [Header("Ground Effects (Pooled)")]
        [Tooltip("Exact name of the stain/mask object inside the prefab")]
        public string stainChildName = "Mask";
        [Tooltip("Exact name of the explosion animation object inside the prefab")]
        public string explosionChildName = "Explosion";

        [Header("Offsets (Local)")]
        public Vector3 stainOffset = new Vector3(0, 0, 0);
        public Vector3 explosionOffset = new Vector3(0, 0.5f, 0);

        [Header("Randomization")]
        [Tooltip("Random scale multiplier (e.g., 0.8 to 1.2)")]
        public Vector2 sizeRandomRange = new Vector2(0.8f, 1.2f);
        [Tooltip("Random angle variation for the explosion animation")]
        public float explosionAngleRandomRange = 15f;
        [Tooltip("Should the ground stain have a fully random 360-degree rotation?")]
        public bool fullyRandomStainRotation = true;

        public void HandleDeathVisuals()
        {
            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            SpriteRenderer mainSprite = null;

            foreach (SpriteRenderer sr in allRenderers)
            {
                if (sr != null)
                {
                    sr.color = Color.black;
                    if (mainSprite == null) mainSprite = sr;
                }
            }

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }

            if (fireEffect != null)
            {
                fireEffect.transform.localScale = Vector3.one * baseExplosionSize;
                fireEffect.Play();
            }

            if (DeathEffectPoolManager.Instance != null)
            {
                GameObject groundFx = DeathEffectPoolManager.Instance.GetDeathEffect();

                if (groundFx != null)
                {
                    Quaternion spawnRotation = transform.rotation;
                    Vector3 spawnPos = transform.position;

                    if (mainSprite != null)
                    {
                        spawnRotation = mainSprite.transform.rotation;
                        spawnPos = new Vector3(mainSprite.bounds.center.x, mainSprite.bounds.min.y, transform.position.z);
                    }

                    groundFx.transform.rotation = spawnRotation;
                    groundFx.transform.position = spawnPos;
                    groundFx.transform.SetParent(transform.parent);

                    Transform stainTransform = groundFx.transform.Find(stainChildName);

                    Transform explosionTransform = groundFx.transform.Find(explosionChildName);

                    if (stainTransform != null)
                    {
                        float randomStainScale = baseExplosionSize * Random.Range(sizeRandomRange.x, sizeRandomRange.y);

                        stainTransform.localScale = baseStainSize * randomStainScale;
                        stainTransform.localPosition = stainOffset;

                        if (fullyRandomStainRotation)
                        {
                            stainTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                        }
                        else
                        {
                            stainTransform.localRotation = Quaternion.identity;
                        }
                    }

                    if (explosionTransform != null)
                    {
                        float randomExpScale = baseExplosionSize * Random.Range(sizeRandomRange.x, sizeRandomRange.y);

                        explosionTransform.localScale = Vector3.one * randomExpScale;
                        explosionTransform.localPosition = explosionOffset;

                        float randomExpRot = Random.Range(-explosionAngleRandomRange, explosionAngleRandomRange);
                        explosionTransform.localRotation = Quaternion.Euler(0, 0, randomExpRot);
                    }

                    groundFx.SetActive(true);
                }
            }
        }

        public void ResetVisuals()
        {
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