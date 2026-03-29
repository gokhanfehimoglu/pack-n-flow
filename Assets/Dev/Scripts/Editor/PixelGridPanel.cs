using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PackNFlow.Editor
{
    public static class PixelGridPanel
    {
        private const float BaseCellSize = 22f;
        private const float MinZoom = 0.25f;
        private const float MaxZoom = 4f;

        private static Vector2Int _hoverCell = new Vector2Int(-1, -1);

        public static void Draw(LevelData level, ref Vector2 scroll, ref float zoom, EditorTool tool, int colorId)
        {
            if (level == null) return;

            int gridW = level.blockZoneWidth;
            int gridH = level.blockZoneHeight;
            if (gridW <= 0 || gridH <= 0) return;

            float cellSize = BaseCellSize * zoom;

            Rect viewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            var evt = Event.current;

            if (evt.type == EventType.ScrollWheel && viewRect.Contains(evt.mousePosition))
            {
                float delta = evt.delta.y > 0 ? -0.1f : 0.1f;
                zoom = Mathf.Clamp(zoom + delta, MinZoom, MaxZoom);
                cellSize = BaseCellSize * zoom;
                evt.Use();
            }

            float contentW = gridW * cellSize;
            float contentH = gridH * cellSize;
            float offsetX = Mathf.Max(0f, (viewRect.width - contentW) * 0.5f);
            float offsetY = Mathf.Max(0f, (viewRect.height - contentH) * 0.5f);
            var contentRect = new Rect(0, 0, contentW + offsetX * 2f, contentH + offsetY * 2f);

            scroll = GUI.BeginScrollView(viewRect, scroll, contentRect);
            {
                Matrix4x4 invMatrix = GUI.matrix.inverse;

                int startCol = Mathf.Max(0, Mathf.FloorToInt((scroll.x - offsetX) / cellSize) - 1);
                int endCol = Mathf.Min(gridW, Mathf.CeilToInt((scroll.x + viewRect.width - offsetX) / cellSize) + 1);
                int startRow = Mathf.Max(0, Mathf.FloorToInt((scroll.y - offsetY) / cellSize) - 1);
                int endRow = Mathf.Min(gridH, Mathf.CeilToInt((scroll.y + viewRect.height - offsetY) / cellSize) + 1);

                DrawBackground(offsetX, offsetY, contentW, contentH);

                for (int y = startRow; y < endRow; y++)
                {
                    for (int x = startCol; x < endCol; x++)
                    {
                        float px = offsetX + x * cellSize;
                        float py = offsetY + y * cellSize;
                        var cellRect = new Rect(px, py, cellSize, cellSize);

                        var block = FindBlock(level, x, y);
                        if (block.HasValue)
                        {
                            Color32 c = level.GetColorById(block.Value.ColorId);
                            EditorGUI.DrawRect(cellRect, c);
                        }
                        else
                        {
                            EditorGUI.DrawRect(cellRect, new Color(0.15f, 0.15f, 0.15f, 1f));
                        }

                        if (cellSize >= 6f)
                        {
                            Color lineColor = (x + y) % 2 == 0
                                ? new Color(1f, 1f, 1f, 0.04f)
                                : new Color(0f, 0f, 0f, 0.04f);
                            EditorGUI.DrawRect(cellRect, lineColor);
                        }

                        if (x == _hoverCell.x && y == _hoverCell.y && cellSize >= 4f)
                        {
                            Color outlineColor = tool == EditorTool.Paint
                                ? new Color(1f, 1f, 1f, 0.9f)
                                : new Color(1f, 0.3f, 0.3f, 0.9f);
                            EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 2f), outlineColor);
                            EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.yMax - 2f, cellRect.width, 2f), outlineColor);
                            EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 2f, cellRect.height), outlineColor);
                            EditorGUI.DrawRect(new Rect(cellRect.xMax - 2f, cellRect.y, 2f, cellRect.height), outlineColor);
                        }
                    }
                }

                Vector2 localMouse = invMatrix.MultiplyPoint(evt.mousePosition);
                Vector2 contentPos = localMouse - new Vector2(offsetX, offsetY);
                int gx = Mathf.FloorToInt(contentPos.x / cellSize);
                int gy = Mathf.FloorToInt(contentPos.y / cellSize);

                if (evt.type == EventType.MouseMove || evt.type == EventType.Repaint)
                {
                    if (gx >= 0 && gx < gridW && gy >= 0 && gy < gridH)
                        _hoverCell = new Vector2Int(gx, gy);
                    else
                        _hoverCell = new Vector2Int(-1, -1);
                }

                if (evt.type == EventType.MouseDown || evt.type == EventType.MouseDrag)
                {
                    if (evt.button == 0 && gx >= 0 && gx < gridW && gy >= 0 && gy < gridH)
                    {
                        Undo.RecordObject(level, tool == EditorTool.Paint ? "Paint Block" : "Erase Block");

                        if (tool == EditorTool.Paint)
                            SetBlockAt(level, gx, gy, colorId);
                        else if (tool == EditorTool.Erase)
                            RemoveBlockAt(level, gx, gy);

                        EditorUtility.SetDirty(level);
                        evt.Use();
                    }
                }
            }
            GUI.EndScrollView(false);
        }

        static void DrawBackground(float ox, float oy, float cw, float ch)
        {
            EditorGUI.DrawRect(new Rect(ox, oy, cw, ch), new Color(0.12f, 0.12f, 0.12f, 1f));
        }

        static PixelBlockData? FindBlock(LevelData level, int x, int y)
        {
            if (level.blockEntries == null) return null;
            for (int i = 0; i < level.blockEntries.Count; i++)
            {
                if (level.blockEntries[i].Coordinates.x == x && level.blockEntries[i].Coordinates.y == y)
                    return level.blockEntries[i];
            }
            return null;
        }

        static void SetBlockAt(LevelData level, int x, int y, int colorId)
        {
            if (level.blockEntries == null)
                level.blockEntries = new List<PixelBlockData>();

            for (int i = 0; i < level.blockEntries.Count; i++)
            {
                if (level.blockEntries[i].Coordinates.x == x && level.blockEntries[i].Coordinates.y == y)
                {
                    var existing = level.blockEntries[i];
                    level.blockEntries[i] = new PixelBlockData(existing.Coordinates, colorId);
                    return;
                }
            }

            level.blockEntries.Add(new PixelBlockData(new Vector2Int(x, y), colorId));
        }

        static void RemoveBlockAt(LevelData level, int x, int y)
        {
            if (level.blockEntries == null) return;
            level.blockEntries.RemoveAll(b => b.Coordinates.x == x && b.Coordinates.y == y);
        }
    }
}
