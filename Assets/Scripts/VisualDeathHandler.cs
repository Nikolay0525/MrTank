using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class VisualDeathHandler : MonoBehaviour
    {
        [Header("Effect Parameters")]
        public float baseExplosionSize = 1.0f;
        public Vector3 baseStainSize = new Vector3(3f, 6f, 1);

        [Header("Tank Effects (Attached)")]
        public GameObject fireObject;
        [Tooltip("How long the fire stays active before hiding")]
        public float fireDuration = 3f;
        [Tooltip("Duration of the fire scale up and scale down animations")]
        public float fireScaleDuration = 0.5f;
        [Tooltip("Regulates how chunky are animation resized during time, more means more smooth")]
        public int fireReSizeAnimationSteps = 10;

        [Header("Ground Effects (Pooled)")]
        [Tooltip("Exact name of the stain/mask object inside the prefab")]
        public string stainChildName = "Mask";
        [Tooltip("Exact name of the explosion animation object inside the prefab")]
        public string explosionChildName = "Explosion";

        [Tooltip("Time before the burnt grass mask appears")]
        public float stainAppearanceDelay = 1f;

        [Header("Offsets (Local)")]
        public Vector3 stainOffset = new Vector3(0, 0, 0);
        public Vector3 explosionOffset = new Vector3(0, 0.5f, 0);

        [Header("Randomization")]
        [Tooltip("Random stain/explosion scale multiplier (e.g., 0.8 to 1.2)")]
        public Vector2 sizeRandomRange = new Vector2(0.8f, 1.2f);
        [Tooltip("Random angle variation for the explosion animation")]
        public float explosionAngleRandomRange = 15f;
        [Tooltip("Should the ground stain have a fully random 360-degree rotation?")]
        public bool fullyRandomStainRotation = true;

        [Header("Animation")]
        [Tooltip("Reference to the Animator component used for vibration. Will be auto-assigned if empty.")]
        public Animator objectAnimator;

        private Coroutine fireAnimationCoroutine;
        private Coroutine stainDelayCoroutine;

        private Vector3 initialFireScale;

        private void Awake()
        {
            if (fireObject != null)
            {
                initialFireScale = fireObject.transform.localScale;
            }

            if (objectAnimator == null)
            {
                objectAnimator = GetComponentInChildren<Animator>();
            }
        }

        private void OnEnable()
        {
            if (objectAnimator != null)
            {
                objectAnimator.enabled = true;
            }
        }

        private IEnumerator FireAnimationRoutine()
        {
            if (fireObject == null) yield break;

            fireObject.transform.localScale = Vector3.zero;
            fireObject.SetActive(true);

            float time = 0f;
            while (time < fireScaleDuration)
            {
                time += Time.deltaTime;
                float normalizedTime = time / fireScaleDuration;

                float steppedTime = Mathf.Floor(normalizedTime * fireReSizeAnimationSteps) / fireReSizeAnimationSteps;

                fireObject.transform.localScale = Vector3.Lerp(Vector3.zero, initialFireScale, steppedTime);
                yield return null;
            }
            fireObject.transform.localScale = initialFireScale;

            yield return new WaitForSeconds(fireDuration);

            time = 0f;
            while (time < fireScaleDuration)
            {
                time += Time.deltaTime;
                float normalizedTime = time / fireScaleDuration;

                float steppedTime = Mathf.Floor(normalizedTime * fireReSizeAnimationSteps) / fireReSizeAnimationSteps;

                fireObject.transform.localScale = Vector3.Lerp(initialFireScale, Vector3.zero, steppedTime);
                yield return null;
            }

            fireObject.transform.localScale = Vector3.zero;
            fireObject.SetActive(false);
        }

        private IEnumerator ShowStainRoutine(GameObject stainObj, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (stainObj != null)
            {
                stainObj.SetActive(true);
            }
        }

        public void HandleDeathVisuals()
        {
            if (objectAnimator != null)
            {
                objectAnimator.enabled = false;
            }

            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            SpriteRenderer mainSprite = null;

            foreach (SpriteRenderer sr in allRenderers)
            {
                if (sr != null)
                {
                    if (fireObject != null && sr.transform.IsChildOf(fireObject.transform))
                    {
                        continue;
                    }

                    sr.color = Color.black;
                    if (mainSprite == null) mainSprite = sr;
                }
            }

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }

            if (fireObject != null)
            {
                fireObject.SetActive(true);

                if (fireAnimationCoroutine != null)
                {
                    StopCoroutine(fireAnimationCoroutine);
                }

                fireAnimationCoroutine = StartCoroutine(FireAnimationRoutine());
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

                        stainTransform.gameObject.SetActive(false);

                        if (stainDelayCoroutine != null)
                        {
                            StopCoroutine(stainDelayCoroutine);
                        }

                        stainDelayCoroutine = StartCoroutine(ShowStainRoutine(stainTransform.gameObject, stainAppearanceDelay));
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
            if (objectAnimator != null)
            {
                objectAnimator.enabled = true;
            }

            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in allRenderers)
            {
                if (fireObject != null && sr.transform.IsChildOf(fireObject.transform))
                {
                    continue;
                }
                sr.color = Color.white;
            }

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col in colliders)
            {
                col.enabled = true;
            }

            if (fireAnimationCoroutine != null)
            {
                StopCoroutine(fireAnimationCoroutine);
                fireAnimationCoroutine = null;
            }

            if (fireObject != null)
            {
                fireObject.SetActive(false);
                fireObject.transform.localScale = initialFireScale;
            }

            if (stainDelayCoroutine != null)
            {
                StopCoroutine(stainDelayCoroutine);
                stainDelayCoroutine = null;
            }
        }
    }
}