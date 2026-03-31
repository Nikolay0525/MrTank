using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
public class TerrainChunk : MonoBehaviour
{
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

    public void GenerateChunk(float globalXOffset)
    {
        Vector3[] vertices = new Vector3[resolution + 1];
        Vector2[] colliderPoints = new Vector2[resolution + 1];
        int[] triangles = new int[resolution * 6];

        float step = width / resolution;

        // Вычисление точек верхнего контура рельефа
        for (int i = 0; i <= resolution; i++)
        {
            float localX = i * step;
            float globalX = globalXOffset + localX;

            // Заміна константи 0f на статичну змінну сиду
            float y = Mathf.PerlinNoise(globalX * noiseScale, ChunkManager.SessionSeed) * heightMultiplier;

            vertices[i] = new Vector3(localX, y, 0f);
            colliderPoints[i] = new Vector2(localX, y);
        }

        // Формирование полного массива вершин (включая основание)
        Vector3[] fullVertices = new Vector3[(resolution + 1) * 2];
        float bottomY = -10f; // Глубина основания ландшафта

        for (int i = 0; i <= resolution; i++)
        {
            fullVertices[i] = vertices[i];
            fullVertices[i + resolution + 1] = new Vector3(vertices[i].x, bottomY, 0f);
        }

        // Расчет индексов треугольников
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

        // Применение данных к сетке
        mesh.Clear();
        mesh.vertices = fullVertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Применение коллайдера
        edgeCollider.points = colliderPoints;
    }
}