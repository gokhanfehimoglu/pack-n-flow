using UnityEngine;

namespace PackNFlow
{
    public static class PositionInterpolator
    {
        public static int ComputeStepCount(Vector3? lastPosition, Vector3 currentPosition, float cellSize)
        {
            if (!lastPosition.HasValue)
                return 1;

            float distance = Vector3.Distance(lastPosition.Value, currentPosition);
            return distance > cellSize ? Mathf.CeilToInt(distance / cellSize) : 1;
        }

        public static Vector3 Interpolate(Vector3 from, Vector3 to, int step, int totalSteps)
        {
            float t = (float)step / totalSteps;
            return Vector3.Lerp(from, to, t);
        }
    }
}
