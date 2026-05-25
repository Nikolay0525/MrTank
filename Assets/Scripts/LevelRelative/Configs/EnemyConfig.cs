using Assets.Scripts.Enums;
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
        public int minDistanceToSpawn = 0;
        public int maxDistanceToSpawn = 99999;
        public float spawnWeight = 1f;
    }
}
