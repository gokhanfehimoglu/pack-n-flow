using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PackNFlow.Editor
{
    public class LevelDesignWindow : EditorWindow
    {
        private const string LevelsRoot = "Assets/Dev/Data/Levels";
        private const string MenuPath = "Tools/Pack-N-Flow/Level Designer";

        private static LevelDesignWindow _instance;

        private LevelData _activeLevel;
        private string[] _levelPaths;
        private string[] _levelNames;
        private int _selectedIndex;

        private Vector2 _sidebarScroll;
        private Vector2 _gridScroll;

        private EditorTool _activeTool;
        private int _brushColorId;
        private float _zoomLevel = 1f;

        private Color _newColorPick = Color.white;
        private bool _showingColorPicker;
        private float _tolerance = 0f;

        [MenuItem(MenuPath)]
        static void Open()
        {
            var win = GetWindow<LevelDesignWindow>("Level Designer");
            _instance = win;
            win.minSize = new Vector2(900, 600);
            win.RefreshLevelList();
        }

        void OnEnable()
        {
            _instance = this;
            wantsMouseMove = true;
            RefreshLevelList();
            Selection.selectionChanged += OnSelectionChanged;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeObject is LevelData ld && ld != _activeLevel)
            {
                LoadLevel(ld);
            }
        }

        void RefreshLevelList()
        {
            if (!AssetDatabase.IsValidFolder(LevelsRoot))
            {
                AssetDatabase.CreateFolder("Assets/Dev/Data", "Levels");
                AssetDatabase.CreateFolder(LevelsRoot, "Debug");
                AssetDatabase.CreateFolder(LevelsRoot, "Release");
                AssetDatabase.Refresh();
            }

            var paths = new List<string>();
            CollectLevelAssets(LevelsRoot, paths);
            _levelPaths = paths.ToArray();
            _levelNames = paths.Select(Path.GetFileNameWithoutExtension).ToArray();

            if (_activeLevel != null)
            {
                int idx = System.Array.IndexOf(_levelPaths, AssetDatabase.GetAssetPath(_activeLevel));
                if (idx >= 0) _selectedIndex = idx;
            }
        }

        static void CollectLevelAssets(string folder, List<string> results)
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { folder });
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                string ext = Path.GetExtension(p);
                if (ext == ".asset")
                    results.Add(p);
            }
        }

        void LoadLevel(LevelData level)
        {
            _activeLevel = level;
            int idx = System.Array.IndexOf(_levelPaths, AssetDatabase.GetAssetPath(level));
            if (idx >= 0) _selectedIndex = idx;
            _zoomLevel = 1f;
            Repaint();
        }

        void CreateNewLevel()
        {
            string defaultName = $"Level_{_levelPaths?.Length ?? 0:D3}";
            string newPath = EditorUtility.SaveFilePanelInProject("New Level", defaultName, "asset", "Create new level", LevelsRoot);
            if (string.IsNullOrEmpty(newPath)) return;

            var level = CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(level, newPath);
            AssetDatabase.SaveAssets();

            RefreshLevelList();
            LoadLevel(level);
            Selection.activeObject = level;
        }

        void NavigateLevel(int direction)
        {
            if (_levelPaths == null || _levelPaths.Length == 0) return;
            _selectedIndex = Mathf.Clamp(_selectedIndex + direction, 0, _levelPaths.Length - 1);
            var level = AssetDatabase.LoadAssetAtPath<LevelData>(_levelPaths[_selectedIndex]);
            if (level != null)
            {
                LoadLevel(level);
                Selection.activeObject = level;
            }
        }

        void OnGUI()
        {
            DrawNavigationBar();

            if (_activeLevel == null)
            {
                EditorGUILayout.HelpBox("Select or create a level to begin editing.", MessageType.Info);
                if (GUILayout.Button("Create New Level", GUILayout.Height(40)))
                    CreateNewLevel();
                return;
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSidebar();
                DrawPixelGridArea();
            }
        }

        void DrawNavigationBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("\u25C0", EditorStyles.toolbarButton, GUILayout.Width(30)))
                    NavigateLevel(-1);

                int displayCount = _levelPaths?.Length ?? 0;
                string label = _activeLevel != null
                    ? _levelNames != null && _selectedIndex < _levelNames.Length
                        ? _levelNames[_selectedIndex]
                        : _activeLevel.name
                    : "No Level";

                EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.MinWidth(100));

                if (GUILayout.Button("\u25B6", EditorStyles.toolbarButton, GUILayout.Width(30)))
                    NavigateLevel(1);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("+ New Level", EditorStyles.toolbarButton))
                    CreateNewLevel();

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                    RefreshLevelList();

                EditorGUILayout.LabelField($"{_selectedIndex + 1}/{displayCount}", EditorStyles.miniLabel, GUILayout.Width(40));
            }
        }

        void DrawSidebar()
        {
            float sidebarWidth = 280f;
            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll, GUILayout.Width(sidebarWidth), GUILayout.ExpandHeight(false));
            {
                DrawColorSection();
                DrawGridSettingsSection();
                DrawUnitAreaSection();
                DrawAutoGenerationSection();
                DrawUnitQueueSection();
                DrawRackAndConveyorSection();
                DrawTextureImportSection();
                DrawValidationSection();
            }
            EditorGUILayout.EndScrollView();

            Rect separator = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(separator.xMax - 1, separator.y, 1, separator.height), new Color(0.3f, 0.3f, 0.3f));
        }

        void DrawPixelGridArea()
        {
            EditorGUILayout.BeginVertical();
            {
                DrawGridToolbar();
                PixelGridPanel.Draw(_activeLevel, ref _gridScroll, ref _zoomLevel, _activeTool, _brushColorId);
            }
            EditorGUILayout.EndVertical();
        }

        void DrawGridToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                string[] toolLabels = { "Paint", "Erase" };
                int newTool = GUILayout.Toolbar((int)_activeTool, toolLabels, EditorStyles.toolbarButton);
                if (newTool != (int)_activeTool)
                {
                    _activeTool = (EditorTool)newTool;
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label($"Zoom: {_zoomLevel * 100:F0}%", EditorStyles.miniLabel);

                if (GUILayout.Button("Fit", EditorStyles.toolbarButton, GUILayout.Width(40)))
                    _zoomLevel = 1f;
            }
        }

        #region Color Section

        void DrawColorSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Color Palette", EditorStyles.boldLabel);

            var palette = _activeLevel.palette;
            if (palette == null || palette.Count == 0)
            {
                EditorGUILayout.HelpBox("No colors. Import a texture or add manually.", MessageType.Info);
                if (GUILayout.Button("+ Add Color"))
                {
                    _showingColorPicker = true;
                    _newColorPick = Color.white;
                }
            }
            else
            {
                DrawColorSwatches();

                if (_showingColorPicker)
                {
                    _newColorPick = EditorGUILayout.ColorField("New Color", _newColorPick);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Add", GUILayout.Width(60)))
                        {
                            AddColorToPalette((Color32)_newColorPick);
                            _showingColorPicker = false;
                        }
                        if (GUILayout.Button("Cancel", GUILayout.Width(60)))
                            _showingColorPicker = false;
                    }
                }
                else
                {
                    if (GUILayout.Button("+ Add Color"))
                    {
                        _showingColorPicker = true;
                        _newColorPick = Color.white;
                    }
                }

                EditorGUILayout.LabelField($"Active Color: {_brushColorId}", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(4);
        }

        void DrawColorSwatches()
        {
            bool grouping = _activeLevel.enableColorGrouping;
            var shown = new HashSet<int>();
            const int cols = 5;
            int drawn = 0;

            for (int i = 0; i < _activeLevel.palette.Count; i++)
            {
                var entry = _activeLevel.palette[i];
                int displayId = grouping ? entry.UnitColorToken : entry.Id;

                if (grouping && !shown.Add(displayId))
                    continue;

                if (drawn % cols == 0)
                {
                    if (drawn > 0) GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                bool selected = displayId == _brushColorId;
                Color c = entry.Color;

                Rect r = GUILayoutUtility.GetRect(24, 22, GUILayout.Width(24), GUILayout.Height(22));
                EditorGUI.DrawRect(r, c);

                if (selected)
                {
                    Color border = c.grayscale > 0.5f ? Color.black : Color.white;
                    EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 2), border);
                    EditorGUI.DrawRect(new Rect(r.x, r.yMax - 2, r.width, 2), border);
                    EditorGUI.DrawRect(new Rect(r.x, r.y, 2, r.height), border);
                    EditorGUI.DrawRect(new Rect(r.xMax - 2, r.y, 2, r.height), border);
                }

                Rect rmBtn = new Rect(r.xMax - 8, r.y, 8, 8);
                var prevColor = GUI.color;
                GUI.color = new Color(1, 0.3f, 0.3f);
                GUI.Label(rmBtn, "x", EditorStyles.miniLabel);
                GUI.color = prevColor;

                if (Event.current.type == EventType.MouseDown)
                {
                    if (r.Contains(Event.current.mousePosition))
                    {
                        _brushColorId = displayId;
                        _activeTool = EditorTool.Paint;
                        Event.current.Use();
                    }
                    else if (rmBtn.Contains(Event.current.mousePosition))
                    {
                        RemoveColorFromPalette(entry.Id);
                        Event.current.Use();
                    }
                }

                drawn++;
            }

            if (drawn > 0) GUILayout.EndHorizontal();
        }

        void AddColorToPalette(Color32 color)
        {
            Undo.RecordObject(_activeLevel, "Add Color");
            int newId = _activeLevel.RegisterColor(color);
            _brushColorId = _activeLevel.enableColorGrouping
                ? _activeLevel.ResolveUnitColorToken(newId)
                : newId;
            EditorUtility.SetDirty(_activeLevel);
        }

        void RemoveColorFromPalette(int colorId)
        {
            Undo.RecordObject(_activeLevel, "Remove Color");
            _activeLevel.palette.RemoveAll(c => c.Id == colorId);
            _activeLevel.blockEntries.RemoveAll(b => b.ColorId == colorId);

            for (int i = 0; i < _activeLevel.columnDataList.Count; i++)
            {
                _activeLevel.columnDataList[i].entries.RemoveAll(u => u.ColorId == colorId);
            }

            if (_activeLevel.palette.Count > 0)
                _brushColorId = _activeLevel.palette[0].Id;
            else
                _brushColorId = 0;

            EditorUtility.SetDirty(_activeLevel);
        }

        #endregion

        #region Grid Settings Section

        void DrawGridSettingsSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Block Zone", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            int newW = EditorGUILayout.IntSlider("Width", _activeLevel.blockZoneWidth, 1, 100);
            int newH = EditorGUILayout.IntSlider("Height", _activeLevel.blockZoneHeight, 1, 100);
            float newCellSize = EditorGUILayout.FloatField("Cell Size", _activeLevel.blockZoneCellSize);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_activeLevel, "Change Grid Settings");
                _activeLevel.blockZoneWidth = newW;
                _activeLevel.blockZoneHeight = newH;
                _activeLevel.blockZoneCellSize = Mathf.Max(0.01f, newCellSize);
                PruneBlockEntries();
                EditorUtility.SetDirty(_activeLevel);
            }
        }

        void PruneBlockEntries()
        {
            if (_activeLevel.blockEntries == null) return;
            _activeLevel.blockEntries.RemoveAll(b =>
                b.Coordinates.x >= _activeLevel.blockZoneWidth ||
                b.Coordinates.y >= _activeLevel.blockZoneHeight);
        }

        #endregion

        #region Unit Area Section

        void DrawUnitAreaSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Unit Area", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            int newCols = EditorGUILayout.IntSlider("Columns", _activeLevel.unitColumnCount, 1, 8);
            int newDepth = EditorGUILayout.IntSlider("Depth", _activeLevel.unitColumnDepth, 1, 40);
            float newGridSize = EditorGUILayout.FloatField("Cell Size", _activeLevel.unitGridCellSize);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_activeLevel, "Change Unit Area");
                _activeLevel.unitColumnCount = newCols;
                _activeLevel.unitColumnDepth = newDepth;
                _activeLevel.unitGridCellSize = Mathf.Max(0.1f, newGridSize);
                SyncColumnDataList();
                EditorUtility.SetDirty(_activeLevel);
            }
        }

        void SyncColumnDataList()
        {
            while (_activeLevel.columnDataList.Count < _activeLevel.unitColumnCount)
                _activeLevel.columnDataList.Add(new UnitColumnData());

            while (_activeLevel.columnDataList.Count > _activeLevel.unitColumnCount)
                _activeLevel.columnDataList.RemoveAt(_activeLevel.columnDataList.Count - 1);
        }

        #endregion

        #region Unit Queue Section

        void DrawUnitQueueSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Unit Queue", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Click to select. Drag to reorder. Right-click for options.", MessageType.None);

            if (_activeLevel.columnDataList == null)
                _activeLevel.columnDataList = new List<UnitColumnData>();

            for (int col = 0; col < _activeLevel.columnDataList.Count; col++)
            {
                var column = _activeLevel.columnDataList[col];
                if (column.entries.Count == 0) continue;

                EditorGUILayout.LabelField($"Column {col}", EditorStyles.miniBoldLabel);

                for (int row = 0; row < column.entries.Count; row++)
                {
                    var unit = column.entries[row];
                    Color32 unitColor = _activeLevel.GetColorById(unit.ColorId);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Rect swatchRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20), GUILayout.Height(20));
                        EditorGUI.DrawRect(swatchRect, unitColor);

                        Rect labelRect = GUILayoutUtility.GetRect(140, 20);
                        GUI.Label(labelRect, $"({unit.Coordinates.x},{unit.Coordinates.y}) Cap:{unit.PullCapacity}", EditorStyles.miniLabel);

                        if (Event.current.type == EventType.ContextClick && labelRect.Contains(Event.current.mousePosition))
                        {
                            var menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Change Color"), false, () => ShowUnitColorPicker(col, row));
                            menu.AddItem(new GUIContent("Set Capacity..."), false, () => PromptCapacity(col, row));
                            menu.AddItem(new GUIContent($"Toggle Concealed ({(unit.IsConcealed ? "ON" : "OFF")})"), false, () =>
                            {
                                Undo.RecordObject(_activeLevel, "Toggle Concealed");
                                column.entries[row] = new UnitData(unit.Id, unit.PullCapacity, unit.ColorId,
                                    unit.TetheredUnitId, unit.Coordinates, !unit.IsConcealed);
                                EditorUtility.SetDirty(_activeLevel);
                            });
                            menu.AddItem(new GUIContent("Remove Unit"), false, () =>
                            {
                                Undo.RecordObject(_activeLevel, "Remove Unit");
                                column.entries.RemoveAt(row);
                                ReindexColumn(col);
                                EditorUtility.SetDirty(_activeLevel);
                            });
                            menu.AddItem(new GUIContent("Add Unit Below"), false, () =>
                            {
                                Undo.RecordObject(_activeLevel, "Add Unit");
                                int newId = GetNextUnitId();
                                int newY = row + 1;
                                var newUnit = new UnitData(newId, 5, _brushColorId, -1,
                                    new Vector2Int(col, newY), false);
                                column.entries.Insert(newY, newUnit);
                                ReindexColumn(col);
                                EditorUtility.SetDirty(_activeLevel);
                            });
                            menu.ShowAsContext();
                            Event.current.Use();
                        }
                    }
                }
            }

            if (GUILayout.Button("+ Add Unit to Column 0"))
            {
                Undo.RecordObject(_activeLevel, "Add Unit");
                SyncColumnDataList();
                int newId = GetNextUnitId();
                int col = 0;
                int row = _activeLevel.columnDataList[col].entries.Count;
                var newUnit = new UnitData(newId, 5, _brushColorId, -1, new Vector2Int(col, row), false);
                _activeLevel.columnDataList[col].entries.Add(newUnit);
                EditorUtility.SetDirty(_activeLevel);
            }
        }

        void ShowUnitColorPicker(int col, int row)
        {
            var unit = _activeLevel.columnDataList[col].entries[row];
            ShowPopupColorMenu(col, row, unit.ColorId);
        }

        void ShowPopupColorMenu(int col, int row, int currentColorId)
        {
            var menu = new GenericMenu();

            foreach (var c in _activeLevel.palette)
            {
                int cid = _activeLevel.enableColorGrouping ? c.UnitColorToken : c.Id;
                bool isCurrent = cid == currentColorId;
                menu.AddItem(new GUIContent($"Color {cid}"), isCurrent, () =>
                {
                    Undo.RecordObject(_activeLevel, "Change Unit Color");
                    var unit = _activeLevel.columnDataList[col].entries[row];
                    _activeLevel.columnDataList[col].entries[row] = new UnitData(
                        unit.Id, unit.PullCapacity, cid, unit.TetheredUnitId,
                        unit.Coordinates, unit.IsConcealed);
                    EditorUtility.SetDirty(_activeLevel);
                });
            }

            menu.ShowAsContext();
        }

        void PromptCapacity(int col, int row)
        {
            var unit = _activeLevel.columnDataList[col].entries[row];
            var menu = new GenericMenu();
            int[] options = { 1, 2, 3, 5, 10, 15, 20 };
            foreach (int cap in options)
            {
                bool selected = cap == unit.PullCapacity;
                menu.AddItem(new GUIContent($"{cap}"), selected, () =>
                {
                    Undo.RecordObject(_activeLevel, "Set Capacity");
                    var u = _activeLevel.columnDataList[col].entries[row];
                    _activeLevel.columnDataList[col].entries[row] = new UnitData(
                        u.Id, cap, u.ColorId, u.TetheredUnitId, u.Coordinates, u.IsConcealed);
                    EditorUtility.SetDirty(_activeLevel);
                });
            }
            menu.ShowAsContext();
        }

        void ReindexColumn(int col)
        {
            var entries = _activeLevel.columnDataList[col].entries;
            for (int i = 0; i < entries.Count; i++)
            {
                var u = entries[i];
                entries[i] = new UnitData(u.Id, u.PullCapacity, u.ColorId, u.TetheredUnitId,
                    new Vector2Int(col, i), u.IsConcealed);
            }
        }

        int GetNextUnitId()
        {
            int max = -1;
            foreach (var col in _activeLevel.columnDataList)
                foreach (var u in col.entries)
                    if (u.Id > max) max = u.Id;
            return max + 1;
        }

        #endregion

        #region Rack & Conveyor Section

        void DrawRackAndConveyorSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Rack & Conveyor", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            int newRack = EditorGUILayout.IntSlider("Rack Slots", _activeLevel.rackSlotCount, 0, 10);
            int newConveyor = EditorGUILayout.IntSlider("Carriages", _activeLevel.conveyorCarriageCount, 1, 15);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_activeLevel, "Change Rack/Conveyor");
                _activeLevel.rackSlotCount = newRack;
                _activeLevel.conveyorCarriageCount = newConveyor;
                EditorUtility.SetDirty(_activeLevel);
            }
        }

        #endregion

        #region Texture Import Section

        void DrawTextureImportSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Texture Import", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newTex = (Texture2D)EditorGUILayout.ObjectField("Source", _activeLevel.referenceTexture, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_activeLevel, "Set Texture");
                _activeLevel.referenceTexture = newTex;
                EditorUtility.SetDirty(_activeLevel);
            }

            if (_activeLevel.referenceTexture != null)
            {
                EditorGUILayout.LabelField($"{_activeLevel.referenceTexture.width} x {_activeLevel.referenceTexture.height} px", EditorStyles.miniLabel);
            }

            float newTolerance = EditorGUILayout.Slider("Tolerance", _tolerance, 0f, 128f);
            if (!Mathf.Approximately(newTolerance, _tolerance))
                _tolerance = newTolerance;

            EditorGUI.BeginChangeCheck();
            bool newGrouping = EditorGUILayout.Toggle("Color Grouping", _activeLevel.enableColorGrouping);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_activeLevel, "Toggle Grouping");
                _activeLevel.enableColorGrouping = newGrouping;
                EditorUtility.SetDirty(_activeLevel);
            }

            if (_activeLevel.referenceTexture != null)
            {
                if (GUILayout.Button("Import to Grid", GUILayout.Height(28)))
                {
                    float tolerance = _tolerance;
                    TextureToGridConverter.Convert(_activeLevel, tolerance);
                    AutoUnitGenerator.Generate(_activeLevel);
                    if (_activeLevel.palette.Count > 0)
                        _brushColorId = _activeLevel.enableColorGrouping
                            ? _activeLevel.palette[0].UnitColorToken
                            : _activeLevel.palette[0].Id;
                    EditorUtility.SetDirty(_activeLevel);
                }
            }
        }

        #endregion

        #region Auto Generation Section

        void DrawAutoGenerationSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Auto Unit Generation", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Uses Column Count and Depth from Unit Area to control queue lengths. Larger depth = smaller individual capacities.", MessageType.Info);

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Generate Units", GUILayout.Height(30)))
            {
                Undo.RecordObject(_activeLevel, "Auto Generate Units");
                AutoUnitGenerator.Generate(_activeLevel);
                EditorUtility.SetDirty(_activeLevel);
            }
        }

        #endregion

        #region Validation Section

        void DrawValidationSection()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            if (_activeLevel.palette == null || _activeLevel.palette.Count == 0)
            {
                EditorGUILayout.LabelField("No palette data.", EditorStyles.miniLabel);
                return;
            }

            var colorGroups = new Dictionary<int, (Color color, int blocks, int capacity)>();

            foreach (var c in _activeLevel.palette)
            {
                int groupId = _activeLevel.enableColorGrouping ? c.UnitColorToken : c.Id;
                if (!colorGroups.ContainsKey(groupId))
                    colorGroups[groupId] = (c.Color, 0, 0);
            }

            if (_activeLevel.blockEntries != null)
            {
                foreach (var b in _activeLevel.blockEntries)
                {
                    int groupId = _activeLevel.enableColorGrouping
                        ? _activeLevel.ResolveUnitColorToken(b.ColorId)
                        : b.ColorId;
                    if (colorGroups.TryGetValue(groupId, out var v))
                        colorGroups[groupId] = (v.color, v.blocks + 1, v.capacity);
                }
            }

            foreach (var col in _activeLevel.columnDataList)
            {
                foreach (var u in col.entries)
                {
                    int groupId = _activeLevel.enableColorGrouping
                        ? _activeLevel.ResolveUnitColorToken(u.ColorId)
                        : u.ColorId;
                    if (colorGroups.TryGetValue(groupId, out var v))
                        colorGroups[groupId] = (v.color, v.blocks, v.capacity + u.PullCapacity);
                }
            }

            foreach (var kvp in colorGroups)
            {
                var (color, blocks, capacity) = kvp.Value;
                bool balanced = blocks == capacity;

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(6);
                    var prev = GUI.color;
                    GUI.color = color;
                    GUILayout.Label(balanced ? "\u2713" : "\u26A0", GUILayout.Width(16));
                    GUI.color = prev;
                    EditorGUILayout.LabelField($"Blocks:{blocks} Cap:{capacity}", EditorStyles.miniLabel);
                }
            }
        }

        #endregion
    }
}
