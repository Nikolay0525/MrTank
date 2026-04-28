using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(ObjectPool))]
    public class CloudSpawner : MonoBehaviour
    {
        private ObjectPool pool;

        [Header("Visuals")]
        public Sprite[] cloudSprites;

        [Header("Spawn Settings")]
        public float spawnInterval = 3f;
        public float spawnX = 30f;
        public float despawnX = -30f;
        public float minY = 5f;
        public float maxY = 15f;

        public float zPosition = 5f;

        [Header("Cloud Randomization")]
        public float minSpeed = 0.5f;
        public float maxSpeed = 2f;
        public float minScale = 0.8f;
        public float maxScale = 1.5f;

        private float timer;

        private void Awake()
        {
            pool = GetComponent<ObjectPool>();
        }

        private void Start()
        {
            PreWarmClouds();
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnCloud(spawnX);
                timer = 0f;
            }
        }

        private void PreWarmClouds()
        {
            float totalDistance = spawnX - despawnX;
            float avgSpeed = (minSpeed + maxSpeed) / 2f;
            float distanceStep = avgSpeed * spawnInterval;

            for (float currentX = despawnX; currentX < spawnX; currentX += distanceStep)
            {
                float randomOffset = Random.Range(-distanceStep * 0.5f, distanceStep * 0.5f);
                SpawnCloud(currentX + randomOffset);
            }
        }

        private void SpawnCloud(float xPosition)
        {
            GameObject cloudObj = pool.GetPooledObject();

            if (cloudObj != null)
            {
                float randomY = Random.Range(minY, maxY);
                cloudObj.transform.position = new Vector3(xPosition, randomY, zPosition);

                float randomScale = Random.Range(minScale, maxScale);
                cloudObj.transform.localScale = new Vector3(randomScale, randomScale, 1f);

                Cloud cloudScript = cloudObj.GetComponent<Cloud>();
                if (cloudScript == null) cloudScript = cloudObj.AddComponent<Cloud>();

                Sprite randomSprite = null;
                if (cloudSprites != null && cloudSprites.Length > 0)
                {
                    randomSprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
                }

                float randomSpeed = Random.Range(minSpeed, maxSpeed);
                cloudScript.Setup(randomSpeed, despawnX, randomSprite);

                cloudObj.SetActive(true);
            }
        }
    }
}
