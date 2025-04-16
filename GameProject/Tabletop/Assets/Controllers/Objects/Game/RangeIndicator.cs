using Model;
using UnityEngine;

namespace Controllers.Objects.Game
{
    public class RangeIndicator : MonoBehaviour
    {
        private const int SEGMENT_COUNT = 16;
        private const float LINE_WIDTH = 0.2f;

        [SerializeField] private int[] presetRanges = { 6, 12, 24, 32, 48 };
        [SerializeField] private Material rangeMaterial;
        [SerializeField] private Color circleColor;

        private void Start()
        {
            // Create a range circle for every range in the preset array
            foreach (int range in presetRanges)
            {
                CreateRangeCircle(range);
            }
            // Initially turn off the ranges
            this.gameObject.SetActive(false);
        }

        private void CreateRangeCircle(int range)
        {
            // Create the gameobjects for a singe range circle
            GameObject rangeIndicator = new GameObject($"Range_{range}m");
            rangeIndicator.transform.SetParent(this.transform);
            rangeIndicator.transform.localPosition = Vector3.zero;

            MeshFilter meshFilter = rangeIndicator.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = rangeIndicator.AddComponent<MeshRenderer>();

            meshFilter.mesh = CreateSegmentedRangeMesh(range * Defines.RANGE_ADJUSTMENT);
            meshRenderer.material = new Material(rangeMaterial)
            {
                color = GetRangeColor(range)
            };
        }

        private Mesh CreateSegmentedRangeMesh(float radius)
        {
            Mesh mesh = new Mesh();
            int vertexCount = SEGMENT_COUNT * 4;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[SEGMENT_COUNT * 6];

            float angleStep = 360f / SEGMENT_COUNT;

            for (int i = 0; i < SEGMENT_COUNT; i++)
            {
                float angle = i * angleStep;
                float nextAngle = (i + 1) * angleStep;

                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                Vector3 nextDir = Quaternion.Euler(0, nextAngle, 0) * Vector3.forward;

                // Outer vertices
                int vIndex = i * 4;
                vertices[vIndex] = dir * radius;
                vertices[vIndex + 1] = nextDir * radius;

                // Inner vertices
                vertices[vIndex + 2] = dir * (radius - LINE_WIDTH);
                vertices[vIndex + 3] = nextDir * (radius - LINE_WIDTH);

                // Triangles
                int tIndex = i * 6;
                triangles[tIndex] = vIndex;
                triangles[tIndex + 1] = vIndex + 2;
                triangles[tIndex + 2] = vIndex + 1;
                triangles[tIndex + 3] = vIndex + 1;
                triangles[tIndex + 4] = vIndex + 2;
                triangles[tIndex + 5] = vIndex + 3;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private Color GetRangeColor(int range)
        {
            // Color coding based on range
            return range switch
            {
                6 => new Color(1, 0, 0, 0.4f),     // Red for shortest range
                12 => new Color(1, 0.5f, 0, 0.4f),  // Orange
                24 => new Color(1, 1, 0, 0.4f),     // Yellow
                32 => new Color(0, 1, 0, 0.4f),     // Green
                48 => new Color(0, 0, 1, 0.4f),     // Blue for longest range
                _ => new Color(1, 1, 1, 0.4f)       // Default white
            };
        }
    }
}
