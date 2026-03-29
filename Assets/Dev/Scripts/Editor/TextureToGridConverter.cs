using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PackNFlow.Editor
{
    public static class TextureToGridConverter
    {
        public static void Convert(LevelData level, float tolerance)
        {
            var texture = level.referenceTexture;
            if (texture == null) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                level.referenceTexture = texture;
            }

            level.blockEntries ??= new List<PixelBlockData>();
            level.blockEntries.Clear();
            level.palette.Clear();

            int texW = texture.width;
            int texH = texture.height;
            Color32[] pixels = texture.GetPixels32();

            int gridW = level.blockZoneWidth;
            int gridH = level.blockZoneHeight;

            if (level.enableColorGrouping)
            {
                ConvertGrouped(level, pixels, texW, texH, gridW, gridH, tolerance);
            }
            else
            {
                ConvertExact(level, pixels, texW, texH, gridW, gridH, tolerance);
            }
        }

        static void ConvertExact(LevelData level, Color32[] pixels, int texW, int texH,
            int gridW, int gridH, float tolerance)
        {
            for (int gy = 0; gy < gridH; gy++)
            {
                for (int gx = 0; gx < gridW; gx++)
                {
                    int tx = Mathf.Clamp(Mathf.FloorToInt((float)gx / gridW * texW), 0, texW - 1);
                    int ty = Mathf.Clamp(Mathf.FloorToInt((float)gy / gridH * texH), 0, texH - 1);
                    int flippedY = (texH - 1) - ty;
                    Color32 pixel = pixels[flippedY * texW + tx];

                    if (pixel.a == 0) continue;

                    int colorId = level.RegisterColor(pixel, tolerance);
                    level.blockEntries.Add(new PixelBlockData(new Vector2Int(gx, gy), colorId));
                }
            }
        }

        static void ConvertGrouped(LevelData level, Color32[] pixels, int texW, int texH,
            int gridW, int gridH, float tolerance)
        {
            var uniqueColors = new List<Color32>();
            var gridPixelColors = new Color32[gridW, gridH];

            for (int gy = 0; gy < gridH; gy++)
            {
                for (int gx = 0; gx < gridW; gx++)
                {
                    int tx = Mathf.Clamp(Mathf.FloorToInt((float)gx / gridW * texW), 0, texW - 1);
                    int ty = Mathf.Clamp(Mathf.FloorToInt((float)gy / gridH * texH), 0, texH - 1);
                    int flippedY = (texH - 1) - ty;
                    Color32 pixel = pixels[flippedY * texW + tx];

                    if (pixel.a == 0)
                    {
                        gridPixelColors[gx, gy] = default;
                        continue;
                    }

                    gridPixelColors[gx, gy] = pixel;

                    bool found = false;
                    for (int i = 0; i < uniqueColors.Count; i++)
                    {
                        if (ColorMath.ExactMatch(uniqueColors[i], pixel))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        uniqueColors.Add(pixel);
                }
            }

            var clusters = new List<List<int>>();
            var clusterAssignments = new int[uniqueColors.Count];
            for (int i = 0; i < clusterAssignments.Length; i++)
                clusterAssignments[i] = -1;

            for (int i = 0; i < uniqueColors.Count; i++)
            {
                if (clusterAssignments[i] >= 0) continue;

                var cluster = new List<int> { i };
                clusterAssignments[i] = clusters.Count;

                for (int j = i + 1; j < uniqueColors.Count; j++)
                {
                    if (clusterAssignments[j] >= 0) continue;

                    if (ColorMath.WithinTolerance(uniqueColors[i], uniqueColors[j], tolerance))
                    {
                        cluster.Add(j);
                        clusterAssignments[j] = clusters.Count;
                    }
                }

                clusters.Add(cluster);
            }

            var groupColorMap = new Color32[uniqueColors.Count];
            var groupToPaletteId = new int[clusters.Count];
            int nextId = 0;

            for (int ci = 0; ci < clusters.Count; ci++)
            {
                var cluster = clusters[ci];
                long sumR = 0, sumG = 0, sumB = 0;
                foreach (int idx in cluster)
                {
                    sumR += uniqueColors[idx].r;
                    sumG += uniqueColors[idx].g;
                    sumB += uniqueColors[idx].b;
                }
                int count = cluster.Count;
                byte avgR = (byte)(sumR / count);
                byte avgG = (byte)(sumG / count);
                byte avgB = (byte)(sumB / count);
                var avgColor = new Color32(avgR, avgG, avgB, 255);

                int paletteId = nextId++;
                groupToPaletteId[ci] = paletteId;
                level.palette.Add(new LevelColor(paletteId, avgColor, paletteId));

                foreach (int idx in cluster)
                    groupColorMap[idx] = avgColor;
            }

            var colorToGroupId = new Dictionary<Color32, int>();
            for (int i = 0; i < uniqueColors.Count; i++)
                colorToGroupId[uniqueColors[i]] = groupToPaletteId[clusterAssignments[i]];

            for (int gy = 0; gy < gridH; gy++)
            {
                for (int gx = 0; gx < gridW; gx++)
                {
                    Color32 pixel = gridPixelColors[gx, gy];
                    if (pixel.a == 0) continue;

                    if (colorToGroupId.TryGetValue(pixel, out int colorId))
                    {
                        level.blockEntries.Add(new PixelBlockData(new Vector2Int(gx, gy), colorId));
                    }
                }
            }
        }
    }
}
