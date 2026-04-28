using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static float TotalDistanceTraveled { get; private set; }
    public static float SessionSeed { get; private set; }

    [Header("Dependencies")]
    public ObjectPool chunkPool;
    public ObjectPool closeHillPool;
    public ObjectPool hillPool;
    public ObjectPool mountainPool;
    public Transform cameraTransform;

    [Header("Generation Parameters")]
    public float chunkWidth = 20f;
    public int initialChunks = 5; 
    public float spawnDistance = 30f;
    public float closeHillsVerticalOffset = 2f;
    public float hillsVerticalOffset = 4f;
    public float mountainVerticalOffset = 6f;

    private float currentGlobalGroundX = 0f;
    private Transform lastSpawnedGround;
    private bool isFirstGround = true;

    private float currentGlobalCloseHillX = 0f;
    private Transform lastSpawnedCloseHill;
    private bool isFirstCloseHill = true;

    private float currentGlobalHillX = 0f;
    private Transform lastSpawnedHill;
    private bool isFirstHill = true;

    private float currentGlobalMountainX = 0f;
    private Transform lastSpawnedMountain;
    private bool isFirstMountain = true;

    private void Awake()
    {
        SessionSeed = Random.Range(0f, 100000f);
    }

    private void Start()
    {
        for (int i = 0; i < initialChunks; i++)
        {
            SpawnNextGround();
            SpawnNextCloseHill();
            SpawnNextHill();
            SpawnNextMountain();
        }
    }

    private void Update()
    {
        CheckAndSpawnGround();
        CheckAndSpawnCloseHill();
        CheckAndSpawnHill();
        CheckAndSpawnMountain();
    }

    private void SpawnNextGround()
    {
        GameObject chunkObj = chunkPool.GetPooledObject();
        float spawnX = 0f;

        if (isFirstGround)
        {
            spawnX = cameraTransform.position.x - chunkWidth;
            currentGlobalGroundX = spawnX;
            isFirstGround = false;
        }
        else if (lastSpawnedGround != null)
        {
            spawnX = lastSpawnedGround.position.x + chunkWidth;
        }

        chunkObj.transform.position = new Vector3(spawnX, 0, 0f);

        TerrainChunk chunkLogic = chunkObj.GetComponent<TerrainChunk>();
        if (chunkLogic != null)
        {
            chunkLogic.GenerateChunkAsync(currentGlobalGroundX);
        }

        chunkObj.SetActive(true);
        lastSpawnedGround = chunkObj.transform;

        currentGlobalGroundX += chunkWidth;

        TotalDistanceTraveled = currentGlobalGroundX;
    }

    private void CheckAndSpawnGround()
    {
        if (lastSpawnedGround != null)
        {
            float rightEdge = lastSpawnedGround.position.x + chunkWidth;
            float distanceToEdge = rightEdge - cameraTransform.position.x;

            if (distanceToEdge < spawnDistance)
            {
                SpawnNextGround();
            }
        }
    }

    private void SpawnNextCloseHill()
    {
        GameObject CloseHillObj = closeHillPool.GetPooledObject();
        float spawnX = 0f;

        if (isFirstCloseHill)
        {
            spawnX = cameraTransform.position.x - chunkWidth;
            currentGlobalCloseHillX = spawnX;
            isFirstCloseHill = false;
        }
        else if (lastSpawnedCloseHill != null)
        {
            spawnX = lastSpawnedCloseHill.position.x + chunkWidth;
        }

        CloseHillObj.transform.position = new Vector3(spawnX, closeHillsVerticalOffset, 30f);

        TerrainChunk CloseHillLogic = CloseHillObj.GetComponent<TerrainChunk>();
        if (CloseHillLogic != null)
        {
            CloseHillLogic.GenerateChunkAsync(currentGlobalCloseHillX);
        }

        CloseHillObj.SetActive(true);
        lastSpawnedCloseHill = CloseHillObj.transform;

        currentGlobalCloseHillX += chunkWidth;
    }

    private void CheckAndSpawnCloseHill()
    {
        if (lastSpawnedCloseHill != null)
        {
            float rightEdge = lastSpawnedCloseHill.position.x + chunkWidth;
            float distanceToEdge = rightEdge - cameraTransform.position.x;

            if (distanceToEdge < spawnDistance)
            {
                SpawnNextCloseHill();
            }
        }
    }

    private void SpawnNextHill()
    {
        GameObject hillObj = hillPool.GetPooledObject();
        float spawnX = 0f;

        if (isFirstHill)
        {
            spawnX = cameraTransform.position.x - chunkWidth;
            currentGlobalHillX = spawnX;
            isFirstHill = false;
        }
        else if (lastSpawnedHill != null)
        {
            spawnX = lastSpawnedHill.position.x + chunkWidth;
        }

        hillObj.transform.position = new Vector3(spawnX, hillsVerticalOffset, 60f);

        BackgroundChunk hillLogic = hillObj.GetComponent<BackgroundChunk>();
        if (hillLogic != null)
        {
            hillLogic.GenerateChunkAsync(currentGlobalHillX);
        }

        hillObj.SetActive(true);
        lastSpawnedHill = hillObj.transform;

        currentGlobalHillX += chunkWidth;
    }

    private void CheckAndSpawnHill()
    {
        if (lastSpawnedHill != null)
        {
            float rightEdge = lastSpawnedHill.position.x + chunkWidth;
            float distanceToEdge = rightEdge - cameraTransform.position.x;

            if (distanceToEdge < spawnDistance)
            {
                SpawnNextHill();
            }
        }
    }

    private void SpawnNextMountain()
    {
        GameObject mountainObj = mountainPool.GetPooledObject();
        float spawnX = 0f;

        if (isFirstMountain)
        {
            spawnX = cameraTransform.position.x - chunkWidth;
            currentGlobalMountainX = spawnX;
            isFirstMountain = false;
        }
        else if (lastSpawnedMountain != null)
        {
            spawnX = lastSpawnedMountain.position.x + chunkWidth;
        }

        mountainObj.transform.position = new Vector3(spawnX, mountainVerticalOffset, 90f);

        BackgroundChunk mountainLogic = mountainObj.GetComponent<BackgroundChunk>();
        if (mountainLogic != null)
        {
            mountainLogic.GenerateChunkAsync(currentGlobalMountainX);
        }

        mountainObj.SetActive(true);
        lastSpawnedMountain = mountainObj.transform;

        currentGlobalMountainX += chunkWidth;
    }

    private void CheckAndSpawnMountain()
    {
        if (lastSpawnedMountain != null)
        {
            float rightEdge = lastSpawnedMountain.position.x + chunkWidth;
            float distanceToEdge = rightEdge - cameraTransform.position.x;

            if (distanceToEdge < spawnDistance)
            {
                SpawnNextMountain();
            }
        }
    }
}