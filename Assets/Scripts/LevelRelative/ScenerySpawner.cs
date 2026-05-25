using UnityEngine;
using Assets.ScriptableObjects;

namespace Assets.Scripts
{
    public class ScenerySpawner : MonoBehaviour
    {
        [Header("Global Boundaries")]
        public float spawnX = 30f;
        public float despawnX = -30f;

        [Header("Scene Configuration")]
        public SceneData currentSceneData;

        private void Start()
        {
            PreWarmScenery();
        }

        private void Update()
        {
            if (currentSceneData == null || currentSceneData.sceneryPrefabs == null) return;

            foreach (var config in currentSceneData.sceneryPrefabs)
            {
                config.timer += Time.deltaTime;

                if (config.timer >= config.spawnInterval)
                {
                    SpawnObject(config);
                    config.timer = 0f;
                }
            }
        }

        private void PreWarmScenery()
        {
            if (currentSceneData == null || currentSceneData.sceneryPrefabs == null) return;

            float minX = Mathf.Min(spawnX, despawnX);
            float maxX = Mathf.Max(spawnX, despawnX);

            foreach (var config in currentSceneData.sceneryPrefabs)
            {
                float avgSpeed = (config.minSpeed + config.maxSpeed) / 2f;
                float distanceStep = avgSpeed * config.spawnInterval;

                if (distanceStep <= 0) continue;

                for (float currentX = minX; currentX < maxX; currentX += distanceStep)
                {
                    float randomOffset = Random.Range(-distanceStep * 0.5f, distanceStep * 0.5f);
                    SpawnObject(config, currentX + randomOffset);
                }
            }
        }

        private void SpawnObject(SceneryConfig config, float? prewarmX = null)
        {
            if (config == null) return;

            GameObject spawnedObj = PoolManager.Instance.GetObject(config.gameObject);

            if (spawnedObj != null)
            {
                int direction;
                if (config.randomDirection)
                {
                    int isMovingRight = Random.Range(0, 2);
                    direction = isMovingRight == 0 ? -1 : 1;
                }
                else
                {
                    direction = config.fixedDirection;
                }

                float startX = direction == -1 ? spawnX : despawnX;
                float endX = direction == -1 ? despawnX : spawnX;

                float posX = prewarmX.HasValue ? prewarmX.Value : startX;
                float randomY = Random.Range(config.minY, config.maxY);
                spawnedObj.transform.position = new Vector3(posX, randomY, config.zPosition);

                float randomScale = Random.Range(config.minScale, config.maxScale);
                spawnedObj.transform.localScale = new Vector3(randomScale, randomScale, 1f);

                ObjectMover mover = spawnedObj.GetComponent<ObjectMover>();
                if (mover == null)
                {
                    mover = spawnedObj.AddComponent<ObjectMover>();
                }

                float randomSpeed = Random.Range(config.minSpeed, config.maxSpeed);
                mover.Setup(endX, true, config.parallaxFactor, randomSpeed, direction);

                spawnedObj.SetActive(true);
            }
        }
    }
}