using UnityEngine;

namespace Assets.Scripts
{
    [System.Serializable]
    public class ScenerySettings
    {
        public PoolType poolType;

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

        [Range(0f, 1f)]
        [Tooltip("0 = far (slower speed), 1 = very close (same speed as ground)")]
        public float parallaxFactor = 0.2f;

        [Header("Direction")]
        public bool randomDirection = true;
        [Tooltip("Used if randomDirection is false. 1 for right, -1 for left.")]
        public int fixedDirection = -1;
    }

    public class ScenerySpawner : MonoBehaviour
    {
        [Header("Global Boundaries")]
        public float spawnX = 30f;
        public float despawnX = -30f;

        [Header("Scenery Configurations")]
        public ScenerySettings[] scenerySettings;

        private void Start()
        {
            PreWarmScenery();
        }

        private void Update()
        {
            foreach (var settings in scenerySettings)
            {
                settings.timer += Time.deltaTime;

                if (settings.timer >= settings.spawnInterval)
                {
                    SpawnObject(settings);
                    settings.timer = 0f;
                }
            }
        }

        private void PreWarmScenery()
        {
            float minX = Mathf.Min(spawnX, despawnX);
            float maxX = Mathf.Max(spawnX, despawnX);

            foreach (var settings in scenerySettings)
            {
                float avgSpeed = (settings.minSpeed + settings.maxSpeed) / 2f;
                float distanceStep = avgSpeed * settings.spawnInterval;

                if (distanceStep <= 0) continue;

                for (float currentX = minX; currentX < maxX; currentX += distanceStep)
                {
                    float randomOffset = Random.Range(-distanceStep * 0.5f, distanceStep * 0.5f);
                    SpawnObject(settings, currentX + randomOffset);
                }
            }
        }

        private void SpawnObject(ScenerySettings settings, float? prewarmX = null)
        {
            if (settings == null) return;

            GameObject spawnedObj = PoolManager.Instance.GetObject(settings.poolType);

            if (spawnedObj != null)
            {
                int direction;
                if (settings.randomDirection)
                {
                    int isMovingRight = Random.Range(0, 2);
                    direction = isMovingRight == 0 ? -1 : 1;
                }
                else
                {
                    direction = settings.fixedDirection;
                }

                float startX = direction == -1 ? spawnX : despawnX;
                float endX = direction == -1 ? despawnX : spawnX;

                float posX = prewarmX.HasValue ? prewarmX.Value : startX;
                float randomY = Random.Range(settings.minY, settings.maxY);
                spawnedObj.transform.position = new Vector3(posX, randomY, settings.zPosition);

                float randomScale = Random.Range(settings.minScale, settings.maxScale);
                spawnedObj.transform.localScale = new Vector3(randomScale, randomScale, 1f);

                ObjectMover mover = spawnedObj.GetComponent<ObjectMover>();
                if (mover == null)
                {
                    mover = spawnedObj.AddComponent<ObjectMover>();
                }

                float randomSpeed = Random.Range(settings.minSpeed, settings.maxSpeed);
                mover.Setup(settings.poolType, endX, true , settings.parallaxFactor, randomSpeed, direction);

                spawnedObj.SetActive(true);
            }
        }
    }
}