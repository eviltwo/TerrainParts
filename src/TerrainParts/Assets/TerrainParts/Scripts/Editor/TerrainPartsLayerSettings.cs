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
                return $"Layer {index}";
            }

            var layerName = _layerNames[index];
            if (string.IsNullOrEmpty(layerName))
            {
                return $"Layer {index}";
            }

            return layerName;
        }
    }
}

