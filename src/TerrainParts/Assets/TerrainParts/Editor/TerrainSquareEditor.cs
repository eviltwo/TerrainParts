using UnityEditor;

namespace TerrainParts.Editor
{
    [CustomEditor(typeof(TerrainSquare))]
    public class TerrainSquareEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_alphaTexture"));
            TerrainPartsEditorUtility.DrawBasicDataProperty(serializedObject.FindProperty("_basicData"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
