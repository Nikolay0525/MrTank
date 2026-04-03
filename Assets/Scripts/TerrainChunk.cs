using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
public class TerrainChunk : MonoBehaviour
{
    [Header("Entity Spawning")]
    public GameObject enemyPrefab;
    [Range(0f, 1f)] public float spawnProbability = 0.4f;
    public float spawnHeightOffset = 0.5f;

    [Header("Texture Mapping")]
    public float textureScale = 10f;

    [Header("Generation Parameters")]
    public float width = 20f;
    public float heightMultiplier = 5f;
    public float noiseScale = 0.05f;
    public int resolution = 20;

    private Mesh mesh;
    private EdgeCollider2D edgeCollider;

    // Клас-контейнер для передачі даних між потоками
    private class ChunkData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
        public Vector2[] colliderPoints;
    }

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    // Головний асинхронний метод
    public async void GenerateChunkAsync(float globalXOffset)
    {
        // 1. ГОЛОВНИЙ ПОТІК: Очищення старих об'єктів (API Unity)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Копіюємо змінні, щоб безпечно передати їх у фоновий потік
        float currentSeed = ChunkManager.SessionSeed;
        float currentWidth = width;
        int currentRes = resolution;
        float currentHMulti = heightMultiplier;
        float currentNScale = noiseScale;
        float currentTScale = textureScale;

        // 2. ФОНОВИЙ ПОТІК: Запускаємо важкі розрахунки
        ChunkData data = await Task.Run(() =>
            CalculateChunkData(globalXOffset, currentSeed, currentWidth, currentRes, currentHMulti, currentNScale, currentTScale)
        );

        // 3. ПОВЕРНЕННЯ В ГОЛОВНИЙ ПОТІК: Застосування даних до Unity-компонентів
        mesh.Clear();
        mesh.vertices = data.vertices;
        mesh.triangles = data.triangles;
        mesh.uv = data.uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        edgeCollider.points = data.colliderPoints;

        SpawnEnemy(data.colliderPoints);
    }

    // Цей метод виконується у фоновому потоці. ТУТ НЕ МОЖНА ВИКЛИКАТИ UNITY API (Mesh, Transform тощо)
    private ChunkData CalculateChunkData(float globalXOffset, float seed, float w, int res, float hMulti, float nScale, float tScale)
    {
        float flatZone = 20f;
        float transitionZone = 20f;

        // РОБИМО ПЕРЕКРИТТЯ: додаємо 1 додатковий сегмент до генерації
        int overlapRes = res + 1;

        Vector3[] vertices = new Vector3[overlapRes + 1];
        Vector2[] colliderPoints = new Vector2[overlapRes + 1];

        float step = w / res; // Увага: крок рахуємо за старим res, щоб ширина не зламалася!

        for (int i = 0; i <= overlapRes; i++)
        {
            float localX = i * step; // Останній X буде виходити за межі чанка
            float globalX = globalXOffset + localX;

            float rawY = Mathf.PerlinNoise(globalX * nScale, seed) * hMulti;
            float weight = 0f;

            if (globalX <= flatZone) weight = 0f;
            else if (globalX <= flatZone + transitionZone)
            {
                float t = (globalX - flatZone) / transitionZone;
                weight = Mathf.SmoothStep(0f, 1f, t);
            }
            else weight = 1f;

            float finalY = rawY * weight;

            vertices[i] = new Vector3(localX, finalY, 0f);
            colliderPoints[i] = new Vector2(localX, finalY);
        }

        // Оновлюємо розміри масивів під нову кількість вершин
        Vector3[] fullVertices = new Vector3[(overlapRes + 1) * 2];
        Vector2[] uvs = new Vector2[(overlapRes + 1) * 2];
        float bottomY = -10f;

        for (int i = 0; i <= overlapRes; i++)
        {
            fullVertices[i] = vertices[i];
            fullVertices[i + overlapRes + 1] = new Vector3(vertices[i].x, bottomY, 0f);

            float globalX = globalXOffset + vertices[i].x;
            uvs[i] = new Vector2(globalX * tScale, vertices[i].y * tScale);
            uvs[i + overlapRes + 1] = new Vector2(globalX * tScale, bottomY * tScale);
        }

        // Кількість трикутників теж збільшується
        int[] triangles = new int[overlapRes * 6];
        int vert = 0;
        int tris = 0;
        for (int i = 0; i < overlapRes; i++)
        {
            triangles[tris + 0] = vert + 0;
            triangles[tris + 1] = vert + overlapRes + 1;
            triangles[tris + 2] = vert + 1;
            triangles[tris + 3] = vert + 1;
            triangles[tris + 4] = vert + overlapRes + 1;
            triangles[tris + 5] = vert + overlapRes + 2;
            vert++;
            tris += 6;
        }

        return new ChunkData
        {
            vertices = fullVertices,
            triangles = triangles,
            uvs = uvs,
            colliderPoints = colliderPoints
        };
    }

    private void SpawnEnemy(Vector2[] surfacePoints)
    {
        float safeZoneLimit = 20f;

        if (transform.position.x < safeZoneLimit) return;
        if (enemyPrefab == null || Random.value > spawnProbability) return;

        int safeMargin = 10;
        if (surfacePoints.Length <= safeMargin * 2) return;

        int randomIndex = Random.Range(safeMargin, surfacePoints.Length - safeMargin);
        Vector2 spawnPoint = surfacePoints[randomIndex];

        Vector3 finalSpawnPosition = new Vector3(spawnPoint.x, spawnPoint.y + spawnHeightOffset, 0f);

        GameObject enemyInstance = Instantiate(enemyPrefab, transform);
        enemyInstance.transform.localPosition = finalSpawnPosition;
    }
}