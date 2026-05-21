using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public enum ChunkLogicType
    {
        ComplexTerrain,
        SimpleBackground
    }

    [System.Serializable]
    public class LandscapeLayer
    {
        public PoolType poolType;
        public float verticalOffset;
        public float zOffset;

        [Header("Logic Setup")]
        public ChunkLogicType logicType;
        [Tooltip("Check this ONLY for the main ground to track distance")]
        public bool isMainDistanceTracker;

        [HideInInspector] public bool isFirst = true;
        [HideInInspector] public float currentGlobalX = 0f;
        [HideInInspector] public Transform lastSpawned;
    }

    public class ChunkManager : MonoBehaviour
    {
        public static float TotalDistanceTraveled { get; private set; }
        public static float SessionSeed { get; private set; }

        [Header("Dependencies")]
        public Transform cameraTransform;

        [Header("Landscape Layers Setup")]
        public LandscapeLayer[] layers;

        [Header("Generation Parameters")]
        public float chunkWidth = 20f;
        public int initialChunks = 5;
        public float spawnDistance = 30f;

        private Dictionary<GameObject, PoolType> activeChunksMap = new Dictionary<GameObject, PoolType>();

        private void Awake()
        {
            SessionSeed = Random.Range(0f, 100000f);

            foreach (var layer in layers)
            {
                layer.isFirst = true;
                layer.lastSpawned = null;
                layer.currentGlobalX = 0f;
            }
        }

        private void Start()
        {
            for (int i = 0; i < initialChunks; i++)
            {
                foreach (var layer in layers)
                {
                    SpawnNextChunk(layer);
                }
            }
        }

        private void Update()
        {
            foreach (var layer in layers)
            {
                CheckAndSpawn(layer);
            }
        }

        private void SpawnNextChunk(LandscapeLayer layer)
        {
            GameObject chunkObj = PoolManager.Instance.GetObject(layer.poolType);
            float spawnX = 0f;

            if (layer.isFirst)
            {
                spawnX = cameraTransform.position.x - chunkWidth;
                layer.currentGlobalX = spawnX;
                layer.isFirst = false;
            }
            else if (layer.lastSpawned != null)
            {
                spawnX = layer.lastSpawned.position.x + chunkWidth;
            }

            chunkObj.transform.position = new Vector3(spawnX, layer.verticalOffset, layer.zOffset);

            if (layer.logicType == ChunkLogicType.ComplexTerrain)
            {
                TerrainChunk chunkLogic = chunkObj.GetComponent<TerrainChunk>();
                if (chunkLogic != null) chunkLogic.GenerateChunkAsync(layer.currentGlobalX);
            }
            else if (layer.logicType == ChunkLogicType.SimpleBackground)
            {
                BackgroundChunk chunkLogic = chunkObj.GetComponent<BackgroundChunk>();
                if (chunkLogic != null) chunkLogic.GenerateChunkAsync(layer.currentGlobalX);
            }

            if (layer.isMainDistanceTracker)
            {
                TotalDistanceTraveled = layer.currentGlobalX;
            }

            ScrollableObject scrollable = chunkObj.GetComponent<ScrollableObject>();
            if (scrollable != null)
            {
                scrollable.OnDespawnReached -= HandleChunkDespawn;
                scrollable.OnDespawnReached += HandleChunkDespawn;
            }

            activeChunksMap[chunkObj] = layer.poolType;

            chunkObj.SetActive(true);
            layer.lastSpawned = chunkObj.transform;
            layer.currentGlobalX += chunkWidth;
        }

        private void CheckAndSpawn(LandscapeLayer layer)
        {
            if (layer.lastSpawned != null)
            {
                float rightEdge = layer.lastSpawned.position.x + chunkWidth;
                float distanceToEdge = rightEdge - cameraTransform.position.x;

                if (distanceToEdge < spawnDistance)
                {
                    SpawnNextChunk(layer);
                }
            }
        }

        private void HandleChunkDespawn(GameObject chunkObj)
        {
            ScrollableObject scrollable = chunkObj.GetComponent<ScrollableObject>();
            if (scrollable != null)
            {
                scrollable.OnDespawnReached -= HandleChunkDespawn;
            }

            if (activeChunksMap.TryGetValue(chunkObj, out PoolType correctPoolType))
            {
                PoolManager.Instance.ReturnObject(correctPoolType, chunkObj);
                activeChunksMap.Remove(chunkObj);
            }
        }
    }
}