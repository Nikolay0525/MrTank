using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static float TotalDistanceTraveled { get; private set; }
    public static float SessionSeed { get; private set; }

    [Header("Dependencies")]
    public ObjectPool chunkPool;
    public Transform cameraTransform;

    [Header("Generation Parameters")]
    public float chunkWidth = 20f;
    public int initialChunks = 5; 
    public float spawnDistance = 30f;
    public float verticalOffset = -1f;

    private float currentGlobalX = 0f;
    private Transform lastSpawnedChunk;

    private bool isFirstChunk = true;

    private void Awake()
    {
        SessionSeed = Random.Range(0f, 100000f);
    }

    private void Start()
    {
        for (int i = 0; i < initialChunks; i++)
        {
            SpawnNextChunk();
        }
    }

    private void Update()
    {
        if (lastSpawnedChunk != null)
        {
            float lastChunkRightEdge = lastSpawnedChunk.position.x + chunkWidth;
            float distanceToEdge = lastChunkRightEdge - cameraTransform.position.x;

            if (distanceToEdge < spawnDistance)
            {
                SpawnNextChunk();
            }
        }
    }

    private void SpawnNextChunk()
    {
        GameObject chunkObj = chunkPool.GetPooledObject();

        float spawnX = 0f;

        if (isFirstChunk)
        {
            spawnX = cameraTransform.position.x - chunkWidth;

            currentGlobalX = spawnX;
            isFirstChunk = false;
        }
        else if (lastSpawnedChunk != null)
        {
            spawnX = lastSpawnedChunk.position.x + chunkWidth;
        }

        chunkObj.transform.position = new Vector3(spawnX, verticalOffset, 0f);

        TerrainChunk chunkLogic = chunkObj.GetComponent<TerrainChunk>();
        if (chunkLogic != null)
        {
            chunkLogic.GenerateChunkAsync(currentGlobalX);
        }

        chunkObj.SetActive(true);

        lastSpawnedChunk = chunkObj.transform;
        currentGlobalX += chunkWidth;
        TotalDistanceTraveled = currentGlobalX;
    }
}