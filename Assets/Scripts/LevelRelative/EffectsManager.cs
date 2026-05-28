using Assets.ScriptableObjects;
using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class EffectsManager : MonoBehaviour
    {
        public static EffectsManager Instance { get; private set; }

        private SceneData currentSceneData;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(gameObject);

            LevelManager.OnTankEquipped += InitializeWithNewData;
        }

        private void OnDestroy()
        {
            LevelManager.OnTankEquipped -= InitializeWithNewData;
        }

        private void InitializeWithNewData(SceneData newSceneData)
        {
            currentSceneData = newSceneData;
        }

        public void SpawnEffect(Vector2 position, Vector2 normal, Quaternion projectileRotation, Transform parent, EffectType type)
        {
            if (currentSceneData == null) return;

            EffectConfig configToSpawn = null;

            switch (type)
            {
                case EffectType.GroundHit:
                    configToSpawn = currentSceneData.groundHitEffectPrefab;
                    break;
                case EffectType.EnemyHit:
                    configToSpawn = currentSceneData.enemyHitEffectPrefab;
                    break;
            }

            if (configToSpawn == null)
            {
                Debug.LogWarning("[Effect Manager] Effect config is missing in current SceneData!");
                return;
            }

            GameObject effect = PoolManager.Instance.GetObject(configToSpawn.gameObject);

            if (effect != null)
            {
                effect.transform.position = position;
                effect.transform.SetParent(parent, true);

                if (configToSpawn.alignmentMode == AlignmentMode.SurfaceNormal)
                {
                    float angleRad = Mathf.Atan2(normal.y, normal.x);
                    float angleDeg = angleRad * Mathf.Rad2Deg;
                    effect.transform.rotation = Quaternion.Euler(0, 0, angleDeg - 90f);
                }
                else if (configToSpawn.alignmentMode == AlignmentMode.ProjectileTrajectory)
                {
                    effect.transform.rotation = projectileRotation * Quaternion.Euler(0, 0, 180f);
                }
                else
                {
                    effect.transform.rotation = Quaternion.identity;
                }

                effect.SetActive(true);
            }
        }
    }
}
