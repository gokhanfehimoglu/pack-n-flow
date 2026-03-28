using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public static class BlockScanner
    {
        public delegate bool BlockMatchFn(Unit unit, out PixelBlock block, out Edge edge);
        public delegate bool PullAttemptFn(Unit unit, PixelBlock block, Edge edge);

        public static int Sweep(List<Unit> activeUnits, BlockMatchFn findMatch, PullAttemptFn tryPull)
        {
            int pulled = 0;

            for (var i = activeUnits.Count - 1; i >= 0; i--)
            {
                var unit = activeUnits[i];

                if (!unit.IsScanReady)
                    continue;

                if (!findMatch(unit, out var block, out var edge))
                    continue;

                if (tryPull(unit, block, edge))
                    pulled++;
            }

            return pulled;
        }
    }
}
