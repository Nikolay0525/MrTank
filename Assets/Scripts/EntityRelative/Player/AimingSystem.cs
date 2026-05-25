using System;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class AimingSystem : MonoBehaviour
    {
        [Header("Prefabs used")]
        public GameObject projectilePrefab;

        [Header("Dependencies")]
        public Transform gunPivot;

        [Header("Ballistics")]
        public float projectileSpeed = 12f;
        public float minAngle = 0f;
        public float maxAngle = 90f;
        public float aimSpeed = 3f;

        [Header("Ammunition Stats")]
        public float damage = 15f;
        public float projectileSize = 0.8f;

        [Header("Trajectory Rendering")]
        public int linePoints = 60;

        [Header("Spawning")]
        public Transform firePoint;

        [Header("Visual Effects")]
        public Animator fireEffectAnimator;
        public GameObject gunshotObj;
        public string fireAnimationName = "Gunshot";

        private LineRenderer lineRenderer;
        private float currentAngle;
        private bool isAiming = false;
        private Quaternion defaultGunRotation;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = linePoints;
            lineRenderer.enabled = false;

            ResetGunRotation();
        }

        private void Update()
        {
            if (isAiming)
            {
                OscillateAngle();
                DrawTrajectory();
                RotateGun();
            }
        }

        public void StartAiming()
        {
            isAiming = true;
            lineRenderer.enabled = true;
        }

        public void CancelAiming()
        {
            isAiming = false;
            lineRenderer.enabled = false;

            if (gunPivot != null)
            {
                gunPivot.localRotation = defaultGunRotation;
            }

            ResetGunRotation();
        }

        public GameObject ExecuteShot(Action<bool> onResolutionCallback = null)
        {
            if (fireEffectAnimator != null)
            {
                gunshotObj.SetActive(true);
                fireEffectAnimator.Play(fireAnimationName, 0, 0f);
            }

            isAiming = false;
            lineRenderer.enabled = false;

            float worldAngleRad = gunPivot.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 shootVector = new Vector2(Mathf.Cos(worldAngleRad), Mathf.Sin(worldAngleRad)) * projectileSpeed;

            GameObject projectileInstance = PoolManager.Instance.GetObject(projectilePrefab);

            if (projectileInstance != null && firePoint != null)
            {
                projectileInstance.transform.position = firePoint.position;
                projectileInstance.transform.rotation = firePoint.rotation;
                projectileInstance.SetActive(true);

                Projectile projScript = projectileInstance.GetComponent<Projectile>();

                if (projScript != null)
                {
                    projScript.Initialize(shootVector, damage, projectileSize, onResolutionCallback);
                }
            }

            return projectileInstance;
        }

        public void ResetGunRotation()
        {
            if (gunPivot != null)
            {
                gunPivot.localRotation = defaultGunRotation;
            }
        }

        private void OscillateAngle()
        {
            float t = (Mathf.Sin(Time.time * aimSpeed) + 1f) / 2f;
            currentAngle = Mathf.Lerp(minAngle, maxAngle, t);
        }

        private void RotateGun()
        {
            if (gunPivot != null)
            {
                gunPivot.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
            }
        }

        private void DrawTrajectory()
        {
            float worldAngleRad = gunPivot.eulerAngles.z * Mathf.Deg2Rad;

            Vector2 startPos = firePoint.position;
            Vector2 gravity = Physics2D.gravity;

            float timeStep = 0.1f; // Default fallback value
            if (SettingsManager.Instance != null)
            {
                timeStep = SettingsManager.Instance.trajectoryQuality;
            }

            for (int i = 0; i < linePoints; i++)
            {
                float t = i * timeStep;

                Vector2 point = startPos + new Vector2(
                    projectileSpeed * Mathf.Cos(worldAngleRad) * t,
                    projectileSpeed * Mathf.Sin(worldAngleRad) * t + 0.5f * gravity.y * t * t
                );

                lineRenderer.SetPosition(i, point);
            }
        }
    }
}