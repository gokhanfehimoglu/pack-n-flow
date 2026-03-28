using UnityEngine;

namespace PackNFlow
{
    public static class ColorMath
    {
        public static float EuclideanDistance(Color32 a, Color32 b)
        {
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return Mathf.Sqrt(dr * dr + dg * dg + db * db);
        }

        public static bool WithinTolerance(Color32 a, Color32 b, float tolerance) =>
            EuclideanDistance(a, b) <= tolerance;

        public static bool ExactMatch(Color32 a, Color32 b) =>
            a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }
}
