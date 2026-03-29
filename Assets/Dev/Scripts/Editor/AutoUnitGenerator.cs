using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PackNFlow.Editor
{
    public static class AutoUnitGenerator
    {
        public static void Generate(LevelData level)
        {
            if (level.palette == null || level.palette.Count == 0)
            {
                Debug.LogWarning("No color palette. Import a texture first.");
                return;
            }

            if (level.blockEntries == null || level.blockEntries.Count == 0)
            {
                Debug.LogWarning("No block data. Import a texture first.");
                return;
            }

            int numCols = level.unitColumnCount;
            int maxDepth = level.unitColumnDepth;
            int totalSlots = numCols * maxDepth;

            var colorCounts = new Dictionary<int, int>();
            foreach (var block in level.blockEntries)
            {
                int colorId = level.enableColorGrouping
                    ? level.ResolveUnitColorToken(block.ColorId)
                    : block.ColorId;

                if (!colorCounts.ContainsKey(colorId))
                    colorCounts[colorId] = 0;
                colorCounts[colorId]++;
            }

            int totalBlocks = level.blockEntries.Count;
            int slotCount = Mathf.Min(totalSlots, totalBlocks);

            var colorSlots = AllocateSlots(colorCounts, slotCount);

            var specs = new List<(int color, int cap)>();
            var colorList = colorSlots.Keys.ToList();
            Shuffle(colorList);

            foreach (int colorId in colorList)
            {
                int slots = colorSlots[colorId];
                int blocks = colorCounts[colorId];
                var chunks = SplitEven(blocks, slots);
                foreach (int cap in chunks)
                    specs.Add((colorId, cap));
            }

            Shuffle(specs);

            level.columnDataList ??= new List<UnitColumnData>();
            level.columnDataList.Clear();
            for (int i = 0; i < numCols; i++)
                level.columnDataList.Add(new UnitColumnData());

            int nextId = 0;
            for (int row = 0; row < maxDepth && specs.Count > 0; row++)
            {
                for (int col = 0; col < numCols && specs.Count > 0; col++)
                {
                    var (color, cap) = specs[0];
                    specs.RemoveAt(0);
                    var data = new UnitData(nextId++, cap, color, -1, new Vector2Int(col, row), false);
                    level.columnDataList[col].entries.Add(data);
                }
            }
        }

        static Dictionary<int, int> AllocateSlots(Dictionary<int, int> colorCounts, int slotCount)
        {
            var slots = new Dictionary<int, int>();
            int allocated = 0;

            foreach (int colorId in colorCounts.Keys)
            {
                slots[colorId] = 1;
                allocated++;
                if (allocated >= slotCount) return slots;
            }

            int extra = slotCount - allocated;
            while (extra > 0)
            {
                int bestColor = -1;
                float bestRatio = -1f;

                foreach (var kvp in colorCounts)
                {
                    float ratio = (float)kvp.Value / slots[kvp.Key];
                    if (ratio > bestRatio)
                    {
                        bestRatio = ratio;
                        bestColor = kvp.Key;
                    }
                }

                slots[bestColor]++;
                extra--;
            }

            return slots;
        }

        static List<int> SplitEven(int total, int count)
        {
            if (total <= 0 || count <= 0) return new List<int>();

            count = Mathf.Min(count, total);
            var result = new int[count];
            int baseVal = total / count;
            int rem = total % count;

            for (int i = 0; i < count; i++)
                result[i] = baseVal;
            for (int i = 0; i < rem; i++)
                result[i]++;

            for (int i = result.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            int jitterPasses = count / 2;
            for (int p = 0; p < jitterPasses; p++)
            {
                int a = Random.Range(0, result.Length);
                int b = Random.Range(0, result.Length);
                if (a == b) continue;

                if (result[a] > 1 && result[b] > 1)
                {
                    int transfer = Random.Range(1, Mathf.Min(result[a], result[b]) + 1);
                    if (Random.value < 0.5f) transfer = Mathf.Min(transfer, result[a] - 1);
                    else transfer = Mathf.Min(transfer, result[b] - 1);
                    if (transfer <= 0) continue;

                    if (Random.value < 0.5f)
                        { result[a] += transfer; result[b] -= transfer; }
                    else
                        { result[b] += transfer; result[a] -= transfer; }
                }
            }

            return result.ToList();
        }

        static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
