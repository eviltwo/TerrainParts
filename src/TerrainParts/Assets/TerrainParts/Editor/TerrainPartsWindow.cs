using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainPartsWindow : EditorWindow
    {
        [MenuItem("Window/TerrainParts")]
        public static void Open()
        {
            var window = GetWindow<TerrainPartsWindow>();
            window.titleContent = new GUIContent("Terrain Parts");
            window.Show();
        }

        private static readonly string[] _tabNames = new string[]
        {
            "Parts",
            "Layer",
            "Terrain",
        };
        private int _selectedTab = 0;

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
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
                    DrawLayerGUI();
                    break;
                case 2:
                    DrawTerrainGUI();
                    break;
            }
        }

        private void DrawPartsGUI()
        {
            var parts = FindObjectsOfInterface<ITerrainParts>();
            var count = parts.Count;
            for (int i = 0; i < count; i++)
            {
                var part = parts[i];
                if (part is Component component)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(component.gameObject.name, GUILayout.MaxWidth(150));
                        EditorGUILayout.LabelField(part.GetLayer().ToString(), GUILayout.MaxWidth(100));
                        EditorGUILayout.LabelField(part.GetOrderInLayer().ToString(), GUILayout.MaxWidth(100));
                    }
                }
            }
        }

        private void DrawLayerGUI()
        {
            EditorGUILayout.HelpBox("Sorry, Not implemented yet. It will probably be possible to rename layers.", MessageType.Info);
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

            if (GUILayout.Button("Rebuild terrain using parts"))
            {
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                var parts = FindObjectsOfInterface<ITerrainParts>();
                parts.Sort(TerrainPartsUtility.CompareOrderInLayer);
                for (int i = 0; i < terrainCount; i++)
                {
                    var terrain = terrains[i];
                    var builder = new TerrainBuilder(terrain);
                    builder.Build(parts);
                }
                stopwatch.Stop();
                Debug.Log($"Rebuild terrain using parts: {stopwatch.ElapsedMilliseconds}ms");
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
