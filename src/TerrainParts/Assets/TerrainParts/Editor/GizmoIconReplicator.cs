using System.IO;
using UnityEditor;
using UnityEngine;

namespace TerrainParts
{
    public class GizmoIconReplicator
    {
        private static readonly string GizmoDirectory = "Assets/Gizmos";
        private readonly string _sourceDirectoryPath;
        private readonly string _directoryName;

        public GizmoIconReplicator(string sourceDirectoryPath, string directoryName)
        {
            _sourceDirectoryPath = sourceDirectoryPath;
            _directoryName = directoryName;
        }

        public void Replicate()
        {
            var sourceGuids = AssetDatabase.FindAssets("t:Texture2D", new string[] { _sourceDirectoryPath });

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

                var assetCount = sourceGuids.Length;
                for (int i = 0; i < assetCount; i++)
                {
                    var sourcePath = AssetDatabase.GUIDToAssetPath(sourceGuids[i]);
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
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }
    }
}
