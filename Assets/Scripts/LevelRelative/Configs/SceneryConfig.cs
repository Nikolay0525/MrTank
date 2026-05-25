using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class SceneryConfig : MonoBehaviour
    {
        [Header("Spawn Timing")]
        public float spawnInterval = 3f;
        [HideInInspector] public float timer;

        [Header("Position & Scale")]
        public float minY = 5f;
        public float maxY = 15f;
        public float zPosition = 5f;
        public float minScale = 0.8f;
        public float maxScale = 1.5f;

        [Header("Movement")]
        public float minSpeed = 0.5f;
        public float maxSpeed = 2f;
        [Range(0f, 1f)] public float parallaxFactor = 0.2f;

        [Header("Direction")]
        public bool randomDirection = true;
        public int fixedDirection = -1;
    }
}
