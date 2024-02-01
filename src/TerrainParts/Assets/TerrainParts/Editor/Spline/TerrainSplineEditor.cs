#if SPLINES_SUPPORTED
using TerrainParts.Splines;
using UnityEditor;

namespace TerrainParts.Editor
{
    [CustomEditor(typeof(TerrainSpline))]
    public class TerrainSplineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_splineContainer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_width"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_offset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_alphaTexture"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_innerHeightMapWriteCondition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_innerAlphaMapWriteCondition"));
            TerrainPartsEditorUtility.DrawBasicDataProperty(serializedObject.FindProperty("_basicData"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
