using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
public class TerrainChunk : MonoBehaviour
{
    [Header("Texture Mapping")]
    public float textureScale = 10f;

    [Header("Generation Parameters")]
    public float width = 20f;
    public float heightMultiplier = 5f;
    public float noiseScale = 0.05f;
    public int resolution = 20;

    private Mesh mesh;
    private EdgeCollider2D edgeCollider;

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

    public async void GenerateChunkAsync(float globalXOffset)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.GetComponentInChildren<EnemyAI>(true) != null)
            {
                EnemyPoolManager.Instance.ReturnEnemy(child);
            }
            else
            {
                Destroy(child);
            }
        }

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

        edgeCollider.points = data.colliderPoints;

        SpawnEnemy(data.colliderPoints);
    }

    private ChunkData CalculateChunkData(float globalXOffset, float seed, float w, int res, float hMulti, float nScale, float tScale)
    {
        float flatZone = 10f;
        float transitionZone = 10f;

        int overlapRes = res + 1;
        Vector3[] vertices = new Vector3[overlapRes + 1];
        Vector2[] colliderPoints = new Vector2[overlapRes + 1];

        float step = w / res;

        for (int i = 0; i <= overlapRes; i++)
        {
            float localX = i * step;
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

        return new ChunkData { vertices = fullVertices, triangles = triangles, uvs = uvs, colliderPoints = colliderPoints };
    }

    private void SpawnEnemy(Vector2[] colliderPoints)
    {
        if (colliderPoints.Length > 0 && transform.position.x > 20f)
        {
            int spawnIndex = colliderPoints.Length / 2;
            Vector2 spawnPosition = (Vector2)transform.position + colliderPoints[spawnIndex];

            spawnPosition.y += 0.5f;

            GameObject enemy = EnemyPoolManager.Instance.TryGetEnemy();

            if (enemy != null)
            {
                enemy.transform.position = spawnPosition;
                enemy.transform.rotation = Quaternion.identity;
                enemy.transform.SetParent(this.transform);

                EnemyAI ai = enemy.GetComponent<EnemyAI>();
                if (ai != null)
                {
                    ai.ResetState();
                }

                enemy.SetActive(true);
            }
        }
    }
}