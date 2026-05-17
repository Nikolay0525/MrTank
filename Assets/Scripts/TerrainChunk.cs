using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [System.Serializable]
    public class InstancedScenerySettings
    {
        public string name = "New Scenery";
        public Mesh mesh;
        public Material[] materials;

        [Range(0f, 1f)] public float density = 0.1f;
        public float minScale = 2.0f;
        public float maxScale = 4.0f;

        [Header("Spacing Rules")]
        public float minDistanceSameType = 2f;
        public float minDistanceOtherTypes = 1f;

        [Header("Rotation Settings")]
        public float minRotation = 0f;
        public float maxRotation = 0f;

        public float edgeMargin;

        public float maxSlopeAngle = 25f;
        public float zOffset = 1f;
        public float yOffset = 0f;
    }

    public struct SceneryGenData
    {
        public int materialsCount;
        public float density;
        public float minScale;
        public float maxScale;
        public float minDistanceSameType;
        public float minDistanceOtherTypes;
        public float minRotation;
        public float maxRotation;
        public float edgeMargin;
        public float maxSlopeAngle;
        public float zOffset;
        public float yOffset;
    }

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
    public class TerrainChunk : MonoBehaviour
    {
        [Header("Layer Settings")]
        public bool isBackgroundMode = false;

        [Header("Overall Y-Axis Params")]
        public float grassOffset = 0.15f;

        [Header("Z-Axis Settings")]
        public float grassZOffset = 0f;
        public float enemyZOffset = 0f;
        public float repairStationZOffset = 0f;

        [Header("Visuals")]
        public LineRenderer grassTopRenderer;
        public LineRenderer burntGrassRenderer;

        [Header("Texture Mapping")]
        public float textureScale = 10f;

        [Header("Generation Parameters")]
        public float width = 20f;
        public float heightMultiplier = 5f;
        public float noiseScale = 0.05f;
        public int resolution = 20;

        [Header("Repair Station Settings")]
        public GameObject repairStationPrefab;
        [Range(0f, 1f)] public float repairStationSpawnChance = 0.2f;

        private Mesh mesh;
        private EdgeCollider2D edgeCollider;

        [Header("GPU Instanced Scenery (Trees, Rocks, etc.)")]
        public InstancedScenerySettings[] scenerySettings;

        private Vector3[][][] sceneryLocalPositions;
        private Vector3[][][] sceneryScales;
        private Quaternion[][][] sceneryRotations;
        private Matrix4x4[][][] sceneryMatrices;

        private class ChunkData
        {
            public Vector3[] vertices;
            public int[] triangles;
            public Vector2[] uvs;
            public Vector2[] colliderPoints;

            public Vector3[][][] localPositions;
            public Vector3[][][] scales;

            public Quaternion[][][] rotations;
        }

        private void Awake()
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            edgeCollider = GetComponent<EdgeCollider2D>();
        }

        private void Update()
        {
            if (sceneryLocalPositions == null || scenerySettings == null || scenerySettings.Length == 0) return;

            if (sceneryMatrices == null || sceneryMatrices.Length != scenerySettings.Length)
            {
                sceneryMatrices = new Matrix4x4[scenerySettings.Length][][];
            }

            for (int s = 0; s < scenerySettings.Length; s++)
            {
                InstancedScenerySettings settings = scenerySettings[s];
                if (settings.mesh == null || settings.materials == null || settings.materials.Length == 0) continue;

                if (sceneryMatrices[s] == null || sceneryMatrices[s].Length != settings.materials.Length)
                {
                    sceneryMatrices[s] = new Matrix4x4[settings.materials.Length][];
                }

                for (int m = 0; m < settings.materials.Length; m++)
                {
                    Vector3[] localPosArray = sceneryLocalPositions[s][m];

                    if (localPosArray == null || localPosArray.Length == 0) continue;

                    if (sceneryMatrices[s][m] == null || sceneryMatrices[s][m].Length != localPosArray.Length)
                    {
                        sceneryMatrices[s][m] = new Matrix4x4[localPosArray.Length];
                    }

                    for (int i = 0; i < localPosArray.Length; i++)
                    {
                        Vector3 worldPos = transform.position + localPosArray[i];
                        sceneryMatrices[s][m][i] = Matrix4x4.TRS(worldPos, sceneryRotations[s][m][i], sceneryScales[s][m][i]);
                    }

                    Graphics.DrawMeshInstanced(settings.mesh, 0, settings.materials[m], sceneryMatrices[s][m]);
                }
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
                else if (child.GetComponentInChildren<RepairStation>(true) != null)
                {
                    RepairStationPoolManager.Instance.ReturnRepairStation(child);
                }
                else if (child.GetComponent<GrassMarkFX>() != null)
                {
                    DeathEffectPoolManager.Instance.ReturnDeathEffect(child);
                }
                else if (burntGrassRenderer != null && child == burntGrassRenderer.gameObject)
                {
                    continue;
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

            SceneryGenData[] genData = new SceneryGenData[scenerySettings != null ? scenerySettings.Length : 0];
            for (int i = 0; i < genData.Length; i++)
            {
                genData[i] = new SceneryGenData
                {
                    materialsCount = scenerySettings[i].materials != null ? scenerySettings[i].materials.Length : 0,
                    density = scenerySettings[i].density,
                    minScale = scenerySettings[i].minScale,
                    maxScale = scenerySettings[i].maxScale,
                    minRotation = scenerySettings[i].minRotation,
                    maxRotation = scenerySettings[i].maxRotation,
                    minDistanceSameType = scenerySettings[i].minDistanceSameType,
                    minDistanceOtherTypes = scenerySettings[i].minDistanceOtherTypes,
                    edgeMargin = scenerySettings[i].edgeMargin, 
                    maxSlopeAngle = scenerySettings[i].maxSlopeAngle,
                    zOffset = scenerySettings[i].zOffset,
                    yOffset = scenerySettings[i].yOffset
                };
            }

            ChunkData data = await Task.Run(() =>
                CalculateChunkData(globalXOffset, currentSeed, currentWidth, currentRes, currentHMulti, currentNScale, currentTScale, genData)
            );

            if (this == null) return;

            edgeCollider.points = data.colliderPoints;

            if (grassTopRenderer != null)
            {
                grassTopRenderer.positionCount = data.colliderPoints.Length;
                if (!isBackgroundMode && burntGrassRenderer != null) burntGrassRenderer.positionCount = data.colliderPoints.Length;

                for (int i = 0; i < data.colliderPoints.Length; i++)
                {
                    Vector3 pos = new Vector3(
                        data.colliderPoints[i].x,
                        data.colliderPoints[i].y + grassOffset,
                        grassZOffset
                    );

                    grassTopRenderer.SetPosition(i, pos);
                    if(!isBackgroundMode && burntGrassRenderer != null) burntGrassRenderer.SetPosition(i, new Vector3(pos.x, pos.y, grassZOffset - 0.01f));
                }
            }

            mesh.Clear();
            mesh.vertices = data.vertices;
            mesh.triangles = data.triangles;
            mesh.uv = data.uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            sceneryLocalPositions = data.localPositions;
            sceneryScales = data.scales;
            sceneryRotations = data.rotations;

            if (!isBackgroundMode)
            {
                SpawnEnemy(data.colliderPoints);
                SpawnRepairStation(data.colliderPoints);
            }
        }

        private ChunkData CalculateChunkData(float globalXOffset, float seed, float w, int res, float hMulti, float nScale, float tScale, SceneryGenData[] sceneryGenData)
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

                float rawNoise = Mathf.PerlinNoise(globalX * nScale, seed);
                float centeredNoise = (rawNoise * 2f) - 1f;

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

            int sceneryCount = sceneryGenData.Length;
            List<Vector3>[][] localPosLists = new List<Vector3>[sceneryCount][];
            List<Vector3>[][] scaleLists = new List<Vector3>[sceneryCount][];
            List<Quaternion>[][] rotationLists = new List<Quaternion>[sceneryCount][];

            float[] lastSceneryX = new float[sceneryCount];

            float lastAnyX = -9999f;
            int lastAnyType = -1;
            float lastAnyMinDistOther = 0f; 

            for (int s = 0; s < sceneryCount; s++)
            {
                int matCount = sceneryGenData[s].materialsCount;
                localPosLists[s] = new List<Vector3>[matCount];
                scaleLists[s] = new List<Vector3>[matCount];
                rotationLists[s] = new List<Quaternion>[matCount];

                for (int m = 0; m < matCount; m++)
                {
                    localPosLists[s][m] = new List<Vector3>();
                    scaleLists[s][m] = new List<Vector3>();
                    rotationLists[s][m] = new List<Quaternion>();
                }
                lastSceneryX[s] = -9999f;
            }

            System.Random prng = new System.Random((int)(globalXOffset * 1000f + seed));

            for (int i = 1; i < colliderPoints.Length - 1; i++)
            {
                Vector2 currentPoint = colliderPoints[i];
                Vector2 nextPoint = colliderPoints[i + 1];
                Vector2 prevPoint = colliderPoints[i - 1];
                Vector2 surfaceDirection = (nextPoint - prevPoint).normalized;

                float slopeAngle = Vector2.Angle(Vector2.right, surfaceDirection);
                if (slopeAngle > 90f) slopeAngle = 180f - slopeAngle;

                for (int s = 0; s < sceneryCount; s++)
                {
                    SceneryGenData sData = sceneryGenData[s];

                    if (sData.materialsCount == 0) continue;

                    if (currentPoint.x < sData.edgeMargin || currentPoint.x > (w - sData.edgeMargin))
                    {
                        continue;
                    }

                    if (currentPoint.x - lastSceneryX[s] < sData.minDistanceSameType) continue;

                    if (lastAnyType != -1 && lastAnyType != s)
                    {
                        float requiredDistance = Mathf.Max(lastAnyMinDistOther, sData.minDistanceOtherTypes);

                        if (currentPoint.x - lastAnyX < requiredDistance) continue;
                    }

                    if (prng.NextDouble() <= sData.density && slopeAngle <= sData.maxSlopeAngle)
                    {
                        int randomMaterial = prng.Next(0, sData.materialsCount);

                        float randomScaleY = (float)(prng.NextDouble() * (sData.maxScale - sData.minScale) + sData.minScale);
                        float randomScaleX = randomScaleY;
                        Vector3 scale = new Vector3(randomScaleX, randomScaleY, 1f);

                        float correctedY = currentPoint.y + (randomScaleY / 2f) + +sData.yOffset;
                        Vector3 localPos = new Vector3(currentPoint.x, correctedY, sData.zOffset);

                        float randomAngle = (float)(prng.NextDouble() * (sData.maxRotation - sData.minRotation) + sData.minRotation);
                        Quaternion rotation = Quaternion.Euler(0f, 0f, randomAngle);

                        localPosLists[s][randomMaterial].Add(localPos);
                        scaleLists[s][randomMaterial].Add(scale);
                        rotationLists[s][randomMaterial].Add(rotation);

                        lastSceneryX[s] = currentPoint.x;
                        lastAnyX = currentPoint.x;
                        lastAnyType = s;
                        lastAnyMinDistOther = sData.minDistanceOtherTypes; 

                        break;
                    }
                }
            }

            Vector3[][][] finalLocalPos = new Vector3[sceneryCount][][];
            Vector3[][][] finalScales = new Vector3[sceneryCount][][];
            Quaternion[][][] finalRotations = new Quaternion[sceneryCount][][];

            for (int s = 0; s < sceneryCount; s++)
            {
                int matCount = sceneryGenData[s].materialsCount;
                finalLocalPos[s] = new Vector3[matCount][];
                finalScales[s] = new Vector3[matCount][];
                finalRotations[s] = new Quaternion[matCount][];

                for (int m = 0; m < matCount; m++)
                {
                    finalLocalPos[s][m] = localPosLists[s][m].ToArray();
                    finalScales[s][m] = scaleLists[s][m].ToArray();
                    finalRotations[s][m] = rotationLists[s][m].ToArray();
                }
            }

            return new ChunkData
            {
                vertices = fullVertices,
                triangles = triangles,
                uvs = uvs,
                colliderPoints = colliderPoints,
                localPositions = finalLocalPos,
                scales = finalScales,
                rotations = finalRotations
            };
        }

        private void SpawnEnemy(Vector2[] colliderPoints)
        {
            if (colliderPoints.Length > 0 && transform.position.x > 20f)
            {
                int spawnIndex = colliderPoints.Length / 2;
                Vector2 localPoint = colliderPoints[spawnIndex];

                Vector3 finalSpawnPosition = new Vector3(
                    transform.position.x + localPoint.x,
                    transform.position.y + localPoint.y + 0.5f,
                    transform.position.z + enemyZOffset
                );

                GameObject enemy = EnemyPoolManager.Instance.TryGetEnemy();

                if (enemy != null)
                {
                    enemy.transform.position = finalSpawnPosition;
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

        private void SpawnRepairStation(Vector2[] colliderPoints)
        {
            GameObject playerTank = GameObject.FindGameObjectWithTag("Player");
            Health health = playerTank.GetComponentInParent<Health>();

            if (health.currentHealth >= 100f || DifficultyManager.Instance.EnemiesPassedSinceLastStation < 3)
            {
                return;
            }

            if (UnityEngine.Random.value > repairStationSpawnChance)
                return;

            if (colliderPoints.Length < 10 || transform.position.x < 20f)
                return;

            List<Vector2> validSpawnPoints = new List<Vector2>();
            int centerIndex = colliderPoints.Length / 2;

            float safeDistance = 3f;

            for (int i = 1; i < colliderPoints.Length - 1; i++)
            {
                if (Mathf.Abs(i - centerIndex) < 5) continue;

                Vector2 currentPoint = colliderPoints[i];
                Vector2 nextPoint = colliderPoints[i + 1];
                Vector2 prevPoint = colliderPoints[i - 1];

                Vector2 surfaceDirection = (nextPoint - prevPoint).normalized;
                float slopeAngle = Vector2.Angle(Vector2.right, surfaceDirection);
                if (slopeAngle > 90f) slopeAngle = 180f - slopeAngle;

                if (slopeAngle > 12.5f) continue;

                bool isOccupied = false;

                if (sceneryLocalPositions != null)
                {
                    for (int s = 0; s < sceneryLocalPositions.Length; s++)
                    {
                        for (int m = 0; m < sceneryLocalPositions[s].Length; m++)
                        {
                            if (sceneryLocalPositions[s][m] == null) continue;

                            for (int t = 0; t < sceneryLocalPositions[s][m].Length; t++)
                            {
                                Vector3 objPos = sceneryLocalPositions[s][m][t];

                                if (Mathf.Abs(currentPoint.x - objPos.x) < safeDistance)
                                {
                                    isOccupied = true;
                                    break;
                                }
                            }
                            if (isOccupied) break;
                        }
                        if (isOccupied) break;
                    }
                }

                if (!isOccupied)
                {
                    validSpawnPoints.Add(currentPoint);
                }
            }

            if (validSpawnPoints.Count == 0) return;

            Vector2 spawnPoint = validSpawnPoints[UnityEngine.Random.Range(0, validSpawnPoints.Count)];

            Vector3 finalStationPosition = new Vector3(
                transform.position.x + spawnPoint.x,
                transform.position.y + spawnPoint.y + 0.75f,
                transform.position.z + repairStationZOffset
            );

            GameObject station = RepairStationPoolManager.Instance.GetRepairStation();

            if (station != null)
            {
                station.transform.position = finalStationPosition;
                station.transform.rotation = Quaternion.identity;

                station.transform.SetParent(this.transform);
                station.SetActive(true);

                DifficultyManager.Instance.ResetStationCounter();
            }
        }
    }
}