using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public static class BlockScanner
    {
        public delegate bool BlockGatherFn(Unit unit, List<PixelBlock> results, out Edge edge);
        public delegate bool PullAttemptFn(Unit unit, PixelBlock block, Edge edge);

        private static readonly List<PixelBlock> _blockBuffer = new();

        public static int Sweep(List<Unit> activeUnits, BlockGatherFn gather, PullAttemptFn tryPull)
        {
            int pulled = 0;

            for (var i = activeUnits.Count - 1; i >= 0; i--)
            {
                var unit = activeUnits[i];

                if (!unit.IsScanReady)
                    continue;

                if (!gather(unit, _blockBuffer, out var edge))
                    continue;

                foreach (var block in _blockBuffer)
                {
                    if (tryPull(unit, block, edge))
                        pulled++;
                }
            }

            return pulled;
        }
    }
}
