using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
public class TerrainChunk : MonoBehaviour
{
    [Header("Visuals")]
    public LineRenderer grassTopRenderer;

    [Header("Texture Mapping")]
    public float textureScale = 10f;

    [Header("Generation Parameters")]
    public float width = 20f;
    public float heightMultiplier = 5f;
    public float noiseScale = 0.05f;
    public int resolution = 20;

    private Mesh mesh;
    private EdgeCollider2D edgeCollider;

    [Header("GPU Instanced Trees")]
    public Mesh treeMesh;          
    public Material[] treeMaterials;   
    [Range(0f, 1f)] public float treeDensity = 0.1f;
    public float minTreeDistance = 2f;
    public float maxSlopeAngle = 25f;

    private Vector3[][] treeLocalPositionsPerType;
    private Vector3[][] treeScalesPerType;
    private Matrix4x4[][] treeMatricesPerType;

    private class ChunkData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
        public Vector2[] colliderPoints;

        public Vector3[][] localPositionsPerType;
        public Vector3[][] scalesPerType;
    }

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    private void Update()
    {
        if (treeLocalPositionsPerType == null || treeMaterials == null || treeMesh == null) return;

        if (treeMatricesPerType == null || treeMatricesPerType.Length != treeMaterials.Length)
        {
            treeMatricesPerType = new Matrix4x4[treeMaterials.Length][];
        }

        for (int typeIndex = 0; typeIndex < treeMaterials.Length; typeIndex++)
        {
            Vector3[] localPosArray = treeLocalPositionsPerType[typeIndex];

            if (localPosArray == null || localPosArray.Length == 0) continue;

            if (treeMatricesPerType[typeIndex] == null || treeMatricesPerType[typeIndex].Length != localPosArray.Length)
            {
                treeMatricesPerType[typeIndex] = new Matrix4x4[localPosArray.Length];
            }

            for (int i = 0; i < localPosArray.Length; i++)
            {
                Vector3 worldPos = transform.position + localPosArray[i];
                treeMatricesPerType[typeIndex][i] = Matrix4x4.TRS(worldPos, Quaternion.identity, treeScalesPerType[typeIndex][i]);
            }

            Graphics.DrawMeshInstanced(treeMesh, 0, treeMaterials[typeIndex], treeMatricesPerType[typeIndex]);
        }
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

        edgeCollider.points = data.colliderPoints;

        if (grassTopRenderer != null)
        {
            grassTopRenderer.positionCount = data.colliderPoints.Length;

            grassTopRenderer.sortingLayerName = "Ground"; 
            grassTopRenderer.sortingOrder = 10;           

            float grassOffset = 0.15f; // the higher value the higher grass

            for (int i = 0; i < data.colliderPoints.Length; i++)
            {
                grassTopRenderer.SetPosition(i, new Vector3(
                    data.colliderPoints[i].x,
                    data.colliderPoints[i].y + grassOffset,
                    0f
                ));
            }
        }

        if (this == null) return;

        mesh.Clear();
        mesh.vertices = data.vertices;
        mesh.triangles = data.triangles;
        mesh.uv = data.uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        edgeCollider.points = data.colliderPoints;

        treeLocalPositionsPerType = data.localPositionsPerType;
        treeScalesPerType = data.scalesPerType;

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

            // Отримуємо стандартний шум від 0 до 1
            float rawNoise = Mathf.PerlinNoise(globalX * nScale, seed);

            // Зміщуємо його в діапазон від -1 до 1
            float centeredNoise = (rawNoise * 2f) - 1f;

            // Множимо на висоту
            float rawY = centeredNoise * hMulti;
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

        int typesCount = treeMaterials != null ? treeMaterials.Length : 0;

        List<Vector3>[] localPosLists = new List<Vector3>[typesCount];
        List<Vector3>[] scaleLists = new List<Vector3>[typesCount];

        for (int i = 0; i < typesCount; i++)
        {
            localPosLists[i] = new List<Vector3>();
            scaleLists[i] = new List<Vector3>();
        }

        System.Random prng = new System.Random((int)(globalXOffset * 1000f + seed));
        float lastTreeX = -9999f;

        for (int i = 1; i < colliderPoints.Length - 1; i++)
        {
            Vector2 currentPoint = colliderPoints[i];

            if (currentPoint.x - lastTreeX < minTreeDistance) continue;

            if (typesCount > 0 && prng.NextDouble() <= treeDensity)
            {
                Vector2 nextPoint = colliderPoints[i + 1];
                Vector2 prevPoint = colliderPoints[i - 1];
                Vector2 surfaceDirection = (nextPoint - prevPoint).normalized;

                float slopeAngle = Vector2.Angle(Vector2.right, surfaceDirection);
                if (slopeAngle > 90f) slopeAngle = 180f - slopeAngle;

                if (slopeAngle <= maxSlopeAngle)
                {
                    int randomTreeType = prng.Next(0, typesCount);

                    float randomScaleY = (float)prng.NextDouble() * 0.5f + 2f;
                    float randomScaleX = randomScaleY;
                    Vector3 scale = new Vector3(randomScaleX, randomScaleY, 1f);

                    float correctedY = currentPoint.y + (randomScaleY / 2f);

                    Vector3 localPos = new Vector3(currentPoint.x, correctedY, 1f);

                    localPosLists[randomTreeType].Add(localPos);
                    scaleLists[randomTreeType].Add(scale);

                    lastTreeX = currentPoint.x;
                }
            }
        }

        Vector3[][] finalLocalPos = new Vector3[typesCount][];
        Vector3[][] finalScales = new Vector3[typesCount][];

        for (int i = 0; i < typesCount; i++)
        {
            finalLocalPos[i] = localPosLists[i].ToArray();
            finalScales[i] = scaleLists[i].ToArray();
        }

        return new ChunkData
        {
            vertices = fullVertices,
            triangles = triangles,
            uvs = uvs,
            colliderPoints = colliderPoints,
            localPositionsPerType = finalLocalPos, // Передаємо згруповані дані
            scalesPerType = finalScales
        };
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