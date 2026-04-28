using System.Threading.Tasks;
using UnityEngine;

// Removed EdgeCollider2D as background doesn't need physics
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BackgroundChunk : MonoBehaviour
{
    [Header("Texture Mapping")]
    public float textureScale = 5f;

    [Header("Generation Parameters")]
    public float width = 20f;
    public float heightMultiplier = 15f; // Increased for massive background mountains
    public float noiseScale = 0.02f;     // Decreased for smoother slopes
    public int resolution = 20;

    private Mesh mesh;

    private class ChunkData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
    }

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public async void GenerateChunkAsync(float globalXOffset)
    {
        float currentSeed = ChunkManager.SessionSeed;
        float currentWidth = width;
        int currentRes = resolution;
        float currentHMulti = heightMultiplier;
        float currentNScale = noiseScale;
        float currentTScale = textureScale;

        ChunkData data = await Task.Run(() =>
            CalculateChunkData(globalXOffset, currentSeed, currentWidth, currentRes, currentHMulti, currentNScale, currentTScale)
        );

        if (this == null) return;

        mesh.Clear();
        mesh.vertices = data.vertices;
        mesh.triangles = data.triangles;
        mesh.uv = data.uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private ChunkData CalculateChunkData(float globalXOffset, float seed, float w, int res, float hMulti, float nScale, float tScale)
    {

        int overlapRes = res + 1;
        Vector3[] vertices = new Vector3[overlapRes + 1];

        float step = w / res;

        for (int i = 0; i <= overlapRes; i++)
        {
            float localX = i * step;
            float globalX = globalXOffset + localX;

            float rawNoise = Mathf.PerlinNoise(globalX * nScale, seed);

            float centeredNoise = (rawNoise * 2f) - 1f;

            float rawY = centeredNoise * hMulti;
            float weight = 1f;

            float finalY = rawY * weight;

            vertices[i] = new Vector3(localX, finalY, 0f);
        }

        Vector3[] fullVertices = new Vector3[(overlapRes + 1) * 2];
        Vector2[] uvs = new Vector2[(overlapRes + 1) * 2];

        float bottomY = -20f;

        for (int i = 0; i <= overlapRes; i++)
        {
            fullVertices[i] = vertices[i];
            fullVertices[i + overlapRes + 1] = new Vector3(vertices[i].x, bottomY, 0f);

            float globalX = globalXOffset + vertices[i].x;
            uvs[i] = new Vector2(globalX * tScale, vertices[i].y * tScale);
            uvs[i + overlapRes + 1] = new Vector2(globalX * tScale, bottomY * tScale);
        }

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
            uvs = uvs
        };
    }
}