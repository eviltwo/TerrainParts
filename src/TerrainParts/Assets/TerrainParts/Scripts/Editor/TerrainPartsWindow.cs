using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainPartsWindow : EditorWindow
    {
        private static readonly string[] _tabNames = new string[]
        {
            "Parts",
            "Builds",
            "Others",
        };

        [SerializeField]
        private TerrainPartsBuildSettings _defaultBuildSettings = null;

        [SerializeField]
        private Texture2D _gizmosIconTexture = null;

        private int _selectedTab = 0;
        private ToolCategory _toolCategory = ToolCategoryExtention.Everything;
        private Vector2 _scrollPosition;
        private TerrainPartsBuildSettings _buildSettings;
        private GizmoIconReplicator _gizmoIconReplicator;

        [MenuItem("Window/TerrainParts")]
        public static void Open()
        {
            var window = GetWindow<TerrainPartsWindow>();
            window.titleContent = new GUIContent("Terrain Parts");
            window.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;

            var buildSettingsGuid = EditorUserSettings.GetConfigValue(TerrainPartsEditorDefines.UserBuildSettingsKey);
            if (!string.IsNullOrEmpty(buildSettingsGuid))
            {
                var userBuildSettingsPath = AssetDatabase.GUIDToAssetPath(buildSettingsGuid);
                if (!string.IsNullOrEmpty(userBuildSettingsPath))
                {
                    _buildSettings = AssetDatabase.LoadAssetAtPath<TerrainPartsBuildSettings>(userBuildSettingsPath);
                }
            }

            if (_buildSettings == null)
            {
                _buildSettings = _defaultBuildSettings;
            }

            _gizmoIconReplicator = new GizmoIconReplicator(_gizmosIconTexture, "TerrainParts");
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            switch (_selectedTab)
            {
                case 0:
                    DrawPartsGUI();
                    break;
                case 1:
                    DrawTerrainGUI();
                    break;
                case 2:
                    DrawOthersGUI();
                    break;
            }
        }

        private void DrawPartsGUI()
        {
            const float LabelWidth = 150;
            const float ValueWidth = 40;

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Object", GUILayout.MaxWidth(LabelWidth));
                EditorGUILayout.LabelField("Layer", GUILayout.MaxWidth(ValueWidth));
                EditorGUILayout.LabelField("Order", GUILayout.MaxWidth(ValueWidth));
            }
            EditorGUILayout.Space();

            using (var scroll = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;

                var parts = FindObjectsOfInterface<ITerrainParts>();
                parts.Sort(TerrainPartsUtility.CompareOrderInLayer);
                var count = parts.Count;
                for (int i = 0; i < count; i++)
                {
                    var part = parts[i];
                    if (part is Component component)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(component.gameObject.name, GUILayout.MaxWidth(LabelWidth));
                            var basicData = part.GetBasicData();
                            EditorGUILayout.LabelField(basicData.Layer.ToString(), GUILayout.MaxWidth(ValueWidth));
                            EditorGUILayout.LabelField(basicData.OrderInLayer.ToString(), GUILayout.MaxWidth(ValueWidth));
                        }
                    }
                }
            }
        }

        private void DrawTerrainGUI()
        {
            var terrains = Terrain.activeTerrains;
            var terrainCount = terrains.Length;

            GUILayout.Label("Active Terrains");
            using (new EditorGUI.IndentLevelScope(1))
            {
                if (terrainCount == 0)
                {
                    GUILayout.Label("No active terrains");
                }
                else
                {
                    for (int i = 0; i < terrainCount; i++)
                    {
                        var terrain = terrains[i];
                        EditorGUILayout.LabelField(terrain.name);
                    }
                }
            }

            EditorGUILayout.Space();

            GUILayout.Label("Build Settings");
            using (new EditorGUI.IndentLevelScope(1))
            {
                _buildSettings = (TerrainPartsBuildSettings)EditorGUILayout.ObjectField(_buildSettings, typeof(TerrainPartsBuildSettings), false);
                if (_buildSettings == null)
                {
                    _buildSettings = _defaultBuildSettings;
                }
            }

            EditorGUILayout.Space();

            _toolCategory = (ToolCategory)EditorGUILayout.EnumFlagsField("Tool filter", _toolCategory);
            if (GUILayout.Button("Rebuild terrain"))
            {
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                var parts = FindObjectsOfInterface<ITerrainParts>().Where(p => p.GetBasicData().ToolCategory.HasFlagAny(_toolCategory)).ToList();
                parts.Sort(TerrainPartsUtility.CompareOrderInLayer);
                for (int i = 0; i < terrainCount; i++)
                {
                    var terrain = terrains[i];
                    var terrainData = terrain.terrainData;
                    foreach (var p in parts)
                    {
                        p.Setup(terrainData.size.x / terrainData.heightmapResolution);
                    }

                    if (_toolCategory.HasFlagAll(ToolCategory.Height))
                    {
                        var painter = new TerrainHeightPainter(terrain);
                        var partsForTool = parts.Where(p => p.GetBasicData().ToolCategory.HasFlagAll(ToolCategory.Height));
                        painter.Paint(partsForTool);
                    }
                    if (_toolCategory.HasFlagAll(ToolCategory.Texture))
                    {
                        var painter = new TerrainTexturePainter(terrain);
                        var partsForTool = parts.Where(p => p.GetBasicData().ToolCategory.HasFlagAll(ToolCategory.Texture));
                        painter.Paint(partsForTool);
                    }
                    if (_toolCategory.HasFlagAll(ToolCategory.Hole))
                    {
                        var painter = new TerrainHolePainter(terrain);
                        var partsForTool = parts.Where(p => p.GetBasicData().ToolCategory.HasFlagAll(ToolCategory.Hole));
                        painter.Paint(partsForTool);
                    }
                    if (_toolCategory.HasFlagAll(ToolCategory.Tree))
                    {
                        var painter = new TerrainTreePainter(terrain, _buildSettings.TreePainterSettings, new System.Random(_buildSettings.RandomSeed));
                        var partsForTool = parts.Where(p => p.GetBasicData().ToolCategory.HasFlagAll(ToolCategory.Tree));
                        painter.Paint(partsForTool);
                    }
                    if (_toolCategory.HasFlagAll(ToolCategory.Detail))
                    {
                        var painter = new TerrainDetailPainter(terrain);
                        var partsForTool = parts.Where(p => p.GetBasicData().ToolCategory.HasFlagAll(ToolCategory.Detail));
                        painter.Paint(partsForTool);
                    }
                }

                stopwatch.Stop();
                Debug.Log($"Rebuild terrain using parts: {stopwatch.ElapsedMilliseconds}ms");

                var buildSettingsGuidForSave = _buildSettings == _defaultBuildSettings ? string.Empty : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_buildSettings));
                EditorUserSettings.SetConfigValue(TerrainPartsEditorDefines.UserBuildSettingsKey, buildSettingsGuidForSave);
            }
        }

        private void DrawOthersGUI()
        {
            EditorGUILayout.LabelField("Gizmos");
            using (new EditorGUI.IndentLevelScope(1))
            {
                EditorGUILayout.HelpBox("Generate image to display the gizmo icon in \"Assets/Gizmos\".", MessageType.Info);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(EditorGUI.indentLevel * 15); // Button indent
                    if (GUILayout.Button("Generate gizmo icon texture"))
                    {
                        _gizmoIconReplicator.Replicate();
                    }
                }
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Layers");
            using (new EditorGUI.IndentLevelScope(1))
            {
                var layerSettingsGuid = EditorUserSettings.GetConfigValue(TerrainPartsEditorDefines.UserLayerSettingsKey);
                var path = AssetDatabase.GUIDToAssetPath(layerSettingsGuid);
                var layerSettings = AssetDatabase.LoadAssetAtPath<TerrainPartsLayerSettings>(path);
                var newLayerSettings = (TerrainPartsLayerSettings)EditorGUILayout.ObjectField(layerSettings, typeof(TerrainPartsLayerSettings), false);
                if (layerSettings != newLayerSettings)
                {
                    var guid = newLayerSettings == null ? string.Empty : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newLayerSettings));
                    EditorUserSettings.SetConfigValue(TerrainPartsEditorDefines.UserLayerSettingsKey, guid);
                }
            }
        }

        private static List<T> FindObjectsOfInterface<T>()
        {
            var founds = new List<T>();
            var objects = FindObjectsOfType<MonoBehaviour>();
            var count = objects.Length;
            for (int i = 0; i < count; i++)
            {
                var obj = objects[i];
                if (obj is T target)
                {
                    founds.Add(target);
                }
            }

            return founds;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (Terrain.activeTerrains.Length == 0)
            {
                return;
            }

            if (Selection.activeGameObject != null
                && Selection.activeGameObject.TryGetComponent<ITerrainParts>(out var parts))
            {
                parts.GetRect(out var x1, out var z1, out var x2, out var z2);
                var terrain = Terrain.activeTerrain;
                var p1 = new Vector3(x1, 0, z1);
                var p2 = new Vector3(x2, 0, z1);
                var p3 = new Vector3(x2, 0, z2);
                var p4 = new Vector3(x1, 0, z2);
                p1.y = terrain.SampleHeight(p1) + terrain.transform.position.y;
                p2.y = terrain.SampleHeight(p2) + terrain.transform.position.y;
                p3.y = terrain.SampleHeight(p3) + terrain.transform.position.y;
                p4.y = terrain.SampleHeight(p4) + terrain.transform.position.y;
                using (new Handles.DrawingScope(Color.blue))
                {
                    Handles.DrawLine(p1, p2);
                    Handles.DrawLine(p2, p3);
                    Handles.DrawLine(p3, p4);
                    Handles.DrawLine(p4, p1);
                }
            }
        }
    }
}
