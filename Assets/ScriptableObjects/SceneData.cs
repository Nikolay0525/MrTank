using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewSceneData", menuName = "Tank Game/Scene Data")]
    public class SceneData : ScriptableObject
    {
        [Header("Player Settings")]
        public GameObject playerTankPrefab;

        [Header("EnvironmentManager Settings")]
        public Assets.Scripts.LayerConfig[] layerPrefabs;

        [Header("EnemySpawner Settings")]
        public List<Assets.Scripts.EnemyConfig> enemyPrefabs;

        [Header("ScenerySpawner Settings")]
        public List<Assets.Scripts.SceneryConfig> sceneryPrefabs;

        [Header("Effects Settings")]
        public EffectConfig groundHitEffectPrefab;
        public EffectConfig enemyHitEffectPrefab;
    }
}
