using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
public class TerrainChunk : MonoBehaviour
{
    [Header("Entity Spawning")]
    [Tooltip("Посилання на префаб ворога (Pz.III)")]
    public GameObject enemyPrefab;
    [Tooltip("Ймовірність появи ворога на цьому чанку (від 0.0 до 1.0)")]
    [Range(0f, 1f)] public float spawnProbability = 0.4f;
    [Tooltip("Зміщення по осі Y для запобігання застряганню в текстурах")]
    public float spawnHeightOffset = 0.5f;

    [Header("Texture Mapping")]
    public float textureScale = 10f; // Регулює щільність тайлінгу текстури

    [Header("Generation Parameters")]
    public float width = 20f;
    public float heightMultiplier = 5f;
    public float noiseScale = 0.05f;
    public int resolution = 20;

    private Mesh mesh;
    private EdgeCollider2D edgeCollider;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    private void SpawnEnemy(Vector2[] surfacePoints)
    {
        // Перевірка наявності префабу та розрахунок математичної ймовірності
        if (enemyPrefab == null || Random.value > spawnProbability) return;

        // Вибір випадкової координати на поверхні чанка.
        // Відступи у 10 індексів з країв гарантують, що ворог не з'явиться на стику двох чанків.
        int safeMargin = 10;
        if (surfacePoints.Length <= safeMargin * 2) return; // Захист від помилки індексу

        int randomIndex = Random.Range(safeMargin, surfacePoints.Length - safeMargin);
        Vector2 spawnPoint = surfacePoints[randomIndex];

        // Формування локальної позиції зі зміщенням по висоті
        Vector3 finalSpawnPosition = new Vector3(spawnPoint.x, spawnPoint.y + spawnHeightOffset, 0f);

        // Інстанціювання об'єкта.
        // Передача 'transform' у якості другого параметра робить об'єкт дочірнім до поточного чанка.
        GameObject enemyInstance = Instantiate(enemyPrefab, transform);

        // Застосування локальних координат
        enemyInstance.transform.localPosition = finalSpawnPosition;
    }

    public void GenerateChunk(float globalXOffset)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        Vector3[] vertices = new Vector3[resolution + 1];
        Vector2[] colliderPoints = new Vector2[resolution + 1];
        int[] triangles = new int[resolution * 6];

        // Новий масив для UV-координат (розмір дорівнює повній кількості вершин)
        Vector2[] uvs = new Vector2[(resolution + 1) * 2];

        float step = width / resolution;

        // Випадковий сид береться з ChunkManager
        float currentSeed = ChunkManager.SessionSeed;

        for (int i = 0; i <= resolution; i++)
        {
            float localX = i * step;
            float globalX = globalXOffset + localX;

            float y = Mathf.PerlinNoise(globalX * noiseScale, currentSeed) * heightMultiplier;

            vertices[i] = new Vector3(localX, y, 0f);
            colliderPoints[i] = new Vector2(localX, y);
        }

        Vector3[] fullVertices = new Vector3[(resolution + 1) * 2];
        float bottomY = -10f;

        for (int i = 0; i <= resolution; i++)
        {
            fullVertices[i] = vertices[i];
            fullVertices[i + resolution + 1] = new Vector3(vertices[i].x, bottomY, 0f);

            // Математичний розрахунок проекції текстури (UV)
            // Координата X текстури прив'язується до глобальної осі X середовища
            // Координата Y текстури прив'язується до локальної осі Y вершини
            float globalX = globalXOffset + vertices[i].x;

            uvs[i] = new Vector2(globalX * textureScale, vertices[i].y * textureScale);
            uvs[i + resolution + 1] = new Vector2(globalX * textureScale, bottomY * textureScale);
        }

        // Розрахунок індексів трикутників залишається без змін
        int vert = 0;
        int tris = 0;
        for (int i = 0; i < resolution; i++)
        {
            triangles[tris + 0] = vert + 0;
            triangles[tris + 1] = vert + resolution + 1;
            triangles[tris + 2] = vert + 1;
            triangles[tris + 3] = vert + 1;
            triangles[tris + 4] = vert + resolution + 1;
            triangles[tris + 5] = vert + resolution + 2;

            vert++;
            tris += 6;
        }

        mesh.Clear();
        mesh.vertices = fullVertices;
        mesh.triangles = triangles;
        mesh.uv = uvs; // Передача масиву UV у відеокарту
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        edgeCollider.points = colliderPoints;

        SpawnEnemy(colliderPoints);
    }
}