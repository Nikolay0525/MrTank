using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GradientFog : MonoBehaviour
{
    [Header("Gradient Colors")]
    public Color topColor = new Color(1f, 1f, 1f, 0f); // Transparent top
    public Color bottomColor = new Color(0.7f, 0.8f, 0.9f, 0.8f); // Haze/Fog color at the bottom

    [Header("Dimensions")]
    public float width = 50f;
    public float height = 20f;

    private void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Create 4 vertices for a simple Quad
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-width / 2, -height / 2, 0), // Bottom Left
            new Vector3(width / 2, -height / 2, 0),  // Bottom Right
            new Vector3(-width / 2, height / 2, 0),  // Top Left
            new Vector3(width / 2, height / 2, 0)    // Top Right
        };

        // Assign colors to vertices
        Color[] colors = new Color[4]
        {
            bottomColor, bottomColor, topColor, topColor
        };

        // Define triangles to render the plane
        int[] triangles = new int[6] { 0, 2, 1, 2, 3, 1 };

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
    }
}