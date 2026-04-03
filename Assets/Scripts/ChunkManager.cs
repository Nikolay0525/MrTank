using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static float TotalDistanceTraveled { get; private set; }

    [Header("Dependencies")]
    public ObjectPool chunkPool;
    public Transform cameraTransform;

    [Header("Generation Parameters")]
    public float chunkWidth = 20f;
    public int initialChunks = 3;
    public float spawnDistance = 30f;
    public float verticalOffset = -1f;

    // Математична координата для шуму Перліна (неперервно зростає)
    private float currentGlobalX = 0f;

    // Фізичне посилання на останній створений сегмент
    private Transform lastSpawnedChunk;

    public static float SessionSeed { get; private set; }

    private void Awake()
    {
        // Генерація псевдовипадкового числа для зсуву в просторі шуму
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
            // Обчислення фізичної позиції правого краю останнього рухомого чанка
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

        // Розрахунок фізичної позиції на сцені з прив'язкою до рухомого об'єкта
        float spawnX = 0f;
        if (lastSpawnedChunk != null)
        {
            spawnX = lastSpawnedChunk.position.x + chunkWidth;
        }

        chunkObj.transform.position = new Vector3(spawnX, verticalOffset, 0f);

        // Передача неперервної математичної координати у функцію генерації сітки
        TerrainChunk chunkLogic = chunkObj.GetComponent<TerrainChunk>();
        if (chunkLogic != null)
        {
            chunkLogic.GenerateChunkAsync(currentGlobalX);
        }

        chunkObj.SetActive(true);

        // Оновлення посилань та лічильників для наступного циклу
        lastSpawnedChunk = chunkObj.transform;
        currentGlobalX += chunkWidth;
        TotalDistanceTraveled = currentGlobalX;
    }
}