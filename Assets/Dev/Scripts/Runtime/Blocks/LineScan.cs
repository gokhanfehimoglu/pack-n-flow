using UnityEngine;

namespace PackNFlow
{
    public static class LineScan
    {
        public static Edge DetermineEdge(Vector3 unitPos, Bounds blockZoneBounds)
        {
            bool withinX = unitPos.x <= blockZoneBounds.max.x && unitPos.x >= blockZoneBounds.min.x;
            bool withinZ = unitPos.z <= blockZoneBounds.max.z && unitPos.z >= blockZoneBounds.min.z;

            if (unitPos.z < blockZoneBounds.min.z && withinX) return Edge.South;
            if (unitPos.z > blockZoneBounds.max.z && withinX) return Edge.North;
            if (unitPos.x > blockZoneBounds.max.x && withinZ) return Edge.East;
            if (unitPos.x < blockZoneBounds.min.x && withinZ) return Edge.West;

            return (Edge)(-1);
        }

        public static bool ResolveStartAndDirection(Edge edge, Vector2Int coords, int gridWidth, int gridHeight,
            out int x, out int y, out int dx, out int dy, out int steps)
        {
            x = y = dx = dy = steps = 0;

            switch (edge)
            {
                case Edge.South:
                    x = coords.x;
                    y = gridHeight - 1;
                    dx = 0; dy = -1;
                    steps = gridHeight;
                    return x >= 0 && x < gridWidth;
                case Edge.North:
                    x = coords.x;
                    y = 0;
                    dx = 0; dy = 1;
                    steps = gridHeight;
                    return x >= 0 && x < gridWidth;
                case Edge.East:
                    y = coords.y;
                    x = gridWidth - 1;
                    dx = -1; dy = 0;
                    steps = gridWidth;
                    return y >= 0 && y < gridHeight;
                case Edge.West:
                    y = coords.y;
                    x = 0;
                    dx = 1; dy = 0;
                    steps = gridWidth;
                    return y >= 0 && y < gridHeight;
                default:
                    return false;
            }
        }

        public static Vector3 ProjectToGridPlane(Vector3 unitPos, Edge edge, GridLayout blockGrid)
        {
            return edge switch
            {
                Edge.South or Edge.North =>
                    new Vector3(unitPos.x, 0f, blockGrid.Center.z),
                Edge.East or Edge.West =>
                    new Vector3(blockGrid.Center.x, 0f, unitPos.z),
                _ => unitPos
            };
        }
    }
}
