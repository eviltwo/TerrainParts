using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TerrainParts
{
    [CreateAssetMenu(fileName = "TerrainPartsLayerSettings", menuName = "TerrainParts/LayerSettings")]
    public class TerrainPartsLayerSettings : ScriptableObject
    {
        [SerializeField]
        private string[] _layerNames = new string[] { "Base" };

        public int GetLayerNameCount()
        {
            return _layerNames.Length;
        }

        public string GetLayerName(int index)
        {
            if (index < 0 || index >= _layerNames.Length)
            {
                return string.Empty;
            }

            var layerName = _layerNames[index];
            if (string.IsNullOrEmpty(layerName))
            {
                return string.Empty;
            }

            return layerName;
        }
    }

    [CustomEditor(typeof(TerrainPartsLayerSettings))]
    public class TerrainPartsLayerSettingsEditor : UnityEditor.Editor
    {
        private ReorderableList _reorderableList;

        private void OnEnable()
        {
            _reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("_layerNames"), false, true, false, false);
            _reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Layer Names");
            _reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _reorderableList.DoLayoutList();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+"))
                {
                    var property = serializedObject.FindProperty("_layerNames");
                    property.InsertArrayElementAtIndex(property.arraySize);
                }
                if (GUILayout.Button("-"))
                {
                    var property = serializedObject.FindProperty("_layerNames");
                    if (property.arraySize > 0)
                    {
                        property.DeleteArrayElementAtIndex(property.arraySize - 1);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Layers for each object are saved as numbers. Editing the name does not change the layer number.", MessageType.Info);
        }
    }
}

