using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class VisualDeathHandler : MonoBehaviour
    {
        [Header("Hierarchy Setup")]
        public Transform tankVisualsRoot;

        [Header("Tank Effects (Attached)")]
        public GameObject fireObject;
        public float fireDuration = 3f;
        public float fireScaleDuration = 0.5f;
        public int fireReSizeAnimationSteps = 10;

        [Header("Ground Effects (Attached Child)")]
        public GameObject deathEffectObject;
        public string stainChildName = "Mask";
        public string explosionChildName = "Explosion";
        public float stainAppearanceDelay = 1f;

        [Header("Randomization (Multipliers based on Editor scale)")]
        public Vector2 sizeRandomRange = new Vector2(0.8f, 1.2f);
        public float explosionAngleRandomRange = 15f;
        public bool fullyRandomStainRotation = true;

        [Header("Animation")]
        public Animator objectAnimator;

        private Coroutine fireAnimationCoroutine;
        private Coroutine stainDelayCoroutine;

        private Vector3 initialFireScale;
        private Vector3 initialStainScale = Vector3.one;
        private Vector3 initialExplosionScale = Vector3.one;

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

            if (deathEffectObject != null)
            {
                Transform stainTransform = deathEffectObject.transform.Find(stainChildName);
                if (stainTransform != null) initialStainScale = stainTransform.localScale;

                Transform explosionTransform = deathEffectObject.transform.Find(explosionChildName);
                if (explosionTransform != null) initialExplosionScale = explosionTransform.localScale;

                deathEffectObject.SetActive(false);
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

            if (tankVisualsRoot != null)
            {
                SpriteRenderer[] allRenderers = tankVisualsRoot.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (SpriteRenderer sr in allRenderers)
                {
                    if (fireObject != null && sr.transform.IsChildOf(fireObject.transform))
                    {
                        continue;
                    }
                    sr.color = Color.black;
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
                if (fireAnimationCoroutine != null) StopCoroutine(fireAnimationCoroutine);
                fireAnimationCoroutine = StartCoroutine(FireAnimationRoutine());
            }

            if (deathEffectObject != null)
            {
                Transform stainTransform = deathEffectObject.transform.Find(stainChildName);
                Transform explosionTransform = deathEffectObject.transform.Find(explosionChildName);

                if (stainTransform != null)
                {
                    float randomStainMultiplier = Random.Range(sizeRandomRange.x, sizeRandomRange.y);
                    stainTransform.localScale = initialStainScale * randomStainMultiplier;

                    stainTransform.localRotation = fullyRandomStainRotation ? Quaternion.Euler(0, 0, Random.Range(0f, 360f)) : Quaternion.identity;
                    stainTransform.gameObject.SetActive(false);

                    if (stainDelayCoroutine != null) StopCoroutine(stainDelayCoroutine);
                    stainDelayCoroutine = StartCoroutine(ShowStainRoutine(stainTransform.gameObject, stainAppearanceDelay));
                }

                if (explosionTransform != null)
                {
                    float randomExpMultiplier = Random.Range(sizeRandomRange.x, sizeRandomRange.y);
                    explosionTransform.localScale = initialExplosionScale * randomExpMultiplier;

                    explosionTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-explosionAngleRandomRange, explosionAngleRandomRange));
                }

                deathEffectObject.SetActive(true);
            }
        }

        public void ResetVisuals()
        {
            if (objectAnimator != null)
            {
                objectAnimator.enabled = true;
            }

            if (tankVisualsRoot != null)
            {
                SpriteRenderer[] allRenderers = tankVisualsRoot.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (SpriteRenderer sr in allRenderers)
                {
                    if (fireObject != null && sr.transform.IsChildOf(fireObject.transform))
                    {
                        continue;
                    }
                    sr.color = Color.white;
                }
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

            if (deathEffectObject != null)
            {
                deathEffectObject.SetActive(false);
            }
        }
    }
}