using System.IO;
using UnityEditor;
using UnityEngine;

namespace TerrainParts
{
    public class GizmoIconReplicator
    {
        private static readonly string GizmoDirectory = "Assets/Gizmos";
        private readonly Texture2D _sourceTexture;
        private readonly string _directoryName;

        public GizmoIconReplicator(Texture2D sourceTexture, string directoryName)
        {
            _sourceTexture = sourceTexture;
            _directoryName = directoryName;
        }

        public void Replicate()
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                if (!AssetDatabase.IsValidFolder(GizmoDirectory))
                {
                    AssetDatabase.CreateFolder(Path.GetDirectoryName(GizmoDirectory), Path.GetFileName(GizmoDirectory));
                }

                var terrainPartsGizmoDirectory = Path.Combine(GizmoDirectory, _directoryName).Replace("\\", "/");
                if (!AssetDatabase.IsValidFolder(terrainPartsGizmoDirectory))
                {
                    AssetDatabase.CreateFolder(GizmoDirectory, _directoryName);
                }

                var sourcePath = AssetDatabase.GetAssetPath(_sourceTexture);
                if (sourcePath == null)
                {
                    Debug.LogError($"Source texture is not found. {_sourceTexture.name}");
                    return;
                }

                var assetName = Path.GetFileName(sourcePath);
                var copyPath = Path.Combine(GizmoDirectory, _directoryName, assetName).Replace("\\", "/");
                var result = AssetDatabase.CopyAsset(sourcePath, copyPath);
                if (result)
                {
                    Debug.Log($"Replecate {sourcePath} to {copyPath}");
                }
                else
                {
                    Debug.LogError($"Replecate failed. {sourcePath} to {copyPath}");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }
    }
}
