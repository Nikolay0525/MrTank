using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class LayerConfig : MonoBehaviour
    {
        public float verticalOffset;
        public float zOffset;

        [Header("Movement Settings")]
        public float despawnX = -30f;
        public float parallaxMultiplier = 1f;

        [Header("Logic Setup")]
        public ChunkLogicType logicType;
        public bool isMainDistanceTracker;

        [HideInInspector] public bool isFirst = true;
        [HideInInspector] public float currentGlobalX = 0f;
        [HideInInspector] public Transform lastSpawned;
    }
}
