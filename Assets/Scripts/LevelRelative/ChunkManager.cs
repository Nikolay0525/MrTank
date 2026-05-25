using UnityEngine;
using Assets.ScriptableObjects;

namespace Assets.Scripts
{
    public class ChunkManager : MonoBehaviour
    {
        public static float TotalDistanceTraveled { get; private set; }
        public static float SessionSeed { get; private set; }

        [Header("Dependencies")]
        public Transform cameraTransform;

        [Header("Scene Configuration")]
        public SceneData currentSceneData;

        [Header("Generation Parameters")]
        public float chunkWidth = 20f;
        public int initialChunks = 5;
        public float spawnDistance = 30f;

        private void Awake()
        {
            SessionSeed = Random.Range(0f, 100000f);

            if (currentSceneData != null && currentSceneData.layerPrefabs != null)
            {
                foreach (var layer in currentSceneData.layerPrefabs)
                {
                    layer.isFirst = true;
                    layer.lastSpawned = null;
                    layer.currentGlobalX = 0f;
                }
            }
        }

        private void Start()
        {
            if (currentSceneData == null || currentSceneData.layerPrefabs == null) return;

            for (int i = 0; i < initialChunks; i++)
            {
                foreach (var layer in currentSceneData.layerPrefabs)
                {
                    SpawnNextChunk(layer);
                }
            }
        }

        private void Update()
        {
            if (currentSceneData == null || currentSceneData.layerPrefabs == null) return;

            foreach (var layer in currentSceneData.layerPrefabs)
            {
                CheckAndSpawn(layer);
            }
        }

        private void SpawnNextChunk(LayerConfig layer)
        {
            GameObject chunkObj = PoolManager.Instance.GetObject(layer.gameObject);
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

            ObjectMover mover = chunkObj.GetComponent<ObjectMover>();
            if (mover != null)
            {
                mover.Setup(layer.despawnX, false, layer.parallaxMultiplier, 0f, -1);
            }

            chunkObj.SetActive(true);
            layer.lastSpawned = chunkObj.transform;
            layer.currentGlobalX += chunkWidth;
        }

        private void CheckAndSpawn(LayerConfig layer)
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
    }
}