using UnityEditor;

namespace TerrainParts.Editor
{
    public static class TerrainPartsEditorUtility
    {
        public static void DrawBasicDataProperty(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("ToolCategory"));
            var toolCategory = (ToolCategory)property.FindPropertyRelative("ToolCategory").enumValueFlag;
            if (toolCategory.HasFlagAny(ToolCategory.Texture | ToolCategory.Hole))
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("TextureLayerIndex"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("TextureLayerStrength"));
                }
            }
            EditorGUILayout.PropertyField(property.FindPropertyRelative("WriteCondition"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Layer"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("OrderInLayer"));
        }
    }
}
