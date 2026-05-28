using Assets.ScriptableObjects;
using System;
using UnityEngine;

namespace Assets.Scripts
{

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        public enum ShootDirection { Right, Left }

        [Header("Damage Configuration")]
        public DamageType damageType = DamageType.Direct;
        public float damageAmount = 25f;

        [Header("Targeting")]
        public LayerMask hittableLayers;

        [Header("Explosive Parameters (For AoE)")]
        public float explosionRadius = 2.5f;

        private Rigidbody2D rb;
        private Action<bool> onResolutionCallback;
        private bool isInitialized = false;
        public void Initialize(Vector2 initialVelocity, float damage, float sizeMultiplier, Action<bool> callback = null)
        {
            rb.linearVelocity = initialVelocity;

            damageAmount = damage;
            transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier, 1f);

            onResolutionCallback = callback;
            isInitialized = true;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            LevelManager.OnTankEquipped += HandleRestart;
        }


        private void OnDisable()
        {
            isInitialized = false;
            onResolutionCallback = null;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            LevelManager.OnTankEquipped -= HandleRestart;
        }


        private void HandleRestart(SceneData sceneData)
        {
            PoolManager.Instance.ReturnObject(gameObject);
        }

        private void FixedUpdate()
        {
            if (isInitialized && rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                Vector2 v = rb.linearVelocity;

                float angleRad = Mathf.Atan2(v.y, v.x);
                float angleDeg = angleRad * Mathf.Rad2Deg;

                angleDeg -= 90f;

                transform.rotation = Quaternion.Euler(0, 0, angleDeg);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & hittableLayers) == 0)
            {
                return;
            }

            bool isHit = false;
            isInitialized = false;

            Health health = collision.gameObject.GetComponentInParent<Health>();
            EffectType effectType = health != null
                ? EffectType.EnemyHit
                : EffectType.GroundHit;

            Transform parentTransform = collision.collider.transform;
            Vector2 hitPoint = collision.contacts[0].point;
            Vector2 hitNormal = collision.contacts[0].normal;

            if (EffectsManager.Instance != null)
            {
                EffectsManager.Instance.SpawnEffect(hitPoint, hitNormal, transform.rotation, parentTransform, effectType);
            }

            switch (damageType)
            {
                case DamageType.Direct:
                    isHit = ApplyDirectDamage(collision.gameObject);
                    break;
                case DamageType.AreaOfEffect:
                    isHit = ApplyAreaDamage(collision.contacts[0].point);
                    break;
            }

            onResolutionCallback?.Invoke(isHit);

            PoolManager.Instance.ReturnObject(gameObject);
        }

        private bool ApplyDirectDamage(GameObject target)
        {
            Health health = target.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                return true;
            }
            return false;
        }

        private bool ApplyAreaDamage(Vector2 impactPoint)
        {
            bool hitAnyEnemy = false;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(impactPoint, explosionRadius, hittableLayers);

            foreach (Collider2D hitCollider in colliders)
            {
                Health health = hitCollider.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damageAmount);
                    hitAnyEnemy = true;
                }
            }
            return hitAnyEnemy;
        }
    }
}
