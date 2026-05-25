using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class SceneryConfig : MonoBehaviour
    {
        [Header("Movement Rules")]
        public float minSpeed = 0.5f;
        public float maxSpeed = 2f;
        public float parallaxFactor = 0.2f;

        [Header("Spawn Rules")]
        public float minY = 5f;
        public float maxY = 15f;
        public float zPosition = 5f;
        public bool randomDirection = true;
        public int fixedDirection = -1;
    }
}
