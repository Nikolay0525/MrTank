using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyConfig : MonoBehaviour
    {
        [Header("Enemy Classification")]
        public EnemyType enemyType;

        [Header("Spawn Conditions")]
        public int minDifficultyLevelToSpawn = 15;
        public int maxDifficultyLevelToSpawn = 30;
        public float spawnWeight = 0.5f;
    }
}
