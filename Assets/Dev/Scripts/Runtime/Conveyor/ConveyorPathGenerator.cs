using System.Collections.Generic;
using Dreamteck.Splines;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PackNFlow
{
    public class ConveyorPathGenerator : MonoBehaviour
    {
        [SerializeField] private SplineComputer spline;

        [Header("Dimensions")]
        [SerializeField] private float trackWidth = 14f;
        [SerializeField] private float trackLength = 20f;
        [SerializeField] private float cornerRadius = 2.5f;

        [Header("Sampling")]
        [SerializeField, Range(2, 64)] private int arcResolution = 20;
        [SerializeField] private float edgeStep = 1f;

        [Header("Visual Offsets")]
        [SerializeField] private float pointScale = 1.5f;
        [SerializeField] private float elevationY = 1.5f;
        [SerializeField] private float depthZ = -4f;

        [Button]
        private void GeneratePath()
        {
            if (spline == null) return;

            var origin = transform.position + Vector3.up * elevationY + Vector3.forward * depthZ;
            var points = BuildPerimeter(origin, trackWidth, trackLength, cornerRadius, arcResolution, edgeStep);

            var splinePoints = new SplinePoint[points.Count];
            for (var i = 0; i < points.Count; i++)
            {
                splinePoints[i] = new SplinePoint
                {
                    position = points[i],
                    normal = Vector3.up,
                    size = pointScale,
                    color = Color.white
                };
            }

            spline.type = Spline.Type.Linear;
            spline.SetPoints(splinePoints);
            spline.RebuildImmediate();
            spline.GetComponent<SplineMesh>()?.RebuildImmediate();
        }

        private static List<Vector3> BuildPerimeter(
            Vector3 origin, float width, float length, float radius, int arcRes, float step)
        {
            float halfW = width * 0.5f;
            float halfL = length * 0.5f;
            float r = Mathf.Clamp(radius, 0f, Mathf.Min(halfW, halfL));

            var perimeter = new List<Vector3>();

            Vector3 blCorner = new(origin.x - halfW + r, origin.y, origin.z - halfL + r);
            Vector3 brCorner = new(origin.x + halfW - r, origin.y, origin.z - halfL + r);
            Vector3 trCorner = new(origin.x + halfW - r, origin.y, origin.z + halfL - r);
            Vector3 tlCorner = new(origin.x - halfW + r, origin.y, origin.z + halfL - r);

            Vector3[] corners = { blCorner, brCorner, trCorner, tlCorner };
            float[] startAngles = { 180f, 270f, 0f, 90f };

            Vector3 firstEdgeStart = new(origin.x - halfW + r, origin.y, origin.z - halfL);
            perimeter.Add(firstEdgeStart);

            for (var side = 0; side < 4; side++)
            {
                var c = corners[side];
                float prevAngle = startAngles[side] - 90f;

                Vector3 arcStart = new(
                    c.x + Mathf.Cos(prevAngle * Mathf.Deg2Rad) * r,
                    c.y,
                    c.z + Mathf.Sin(prevAngle * Mathf.Deg2Rad) * r);

                SampleLine(perimeter, arcStart,
                    new(c.x + Mathf.Cos(startAngles[side] * Mathf.Deg2Rad) * r, c.y,
                        c.z + Mathf.Sin(startAngles[side] * Mathf.Deg2Rad) * r),
                    step);

                SampleArc(perimeter, c, r, startAngles[side], startAngles[side] + 90f, arcRes);
            }

            return perimeter;
        }

        private static void SampleLine(List<Vector3> buffer, Vector3 from, Vector3 to, float step)
        {
            int count = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / Mathf.Max(0.0001f, step)));
            for (var i = 1; i <= count; i++)
            {
                buffer.Add(Vector3.Lerp(from, to, (float)i / count));
            }
        }

        private static void SampleArc(List<Vector3> buffer, Vector3 center, float radius,
            float degFrom, float degTo, int segments)
        {
            segments = Mathf.Max(1, segments);
            float radFrom = degFrom * Mathf.Deg2Rad;
            float radTo = degTo * Mathf.Deg2Rad;

            for (var i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(radFrom, radTo, (float)i / segments);
                buffer.Add(new Vector3(
                    center.x + Mathf.Cos(angle) * radius,
                    center.y,
                    center.z + Mathf.Sin(angle) * radius));
            }
        }
    }
}
