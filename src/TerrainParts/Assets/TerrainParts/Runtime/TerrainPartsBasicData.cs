using UnityEngine;

namespace TerrainParts
{
    [System.Serializable]
    public class TerrainPartsBasicData
    {
        [System.Serializable]
        public class TreePrototypeData
        {
            [SerializeField]
            public int Index = 0;

            [SerializeField]
            public float DensityPerUnit = 0.1f;
        }

        [SerializeField]
        public ToolCategory ToolCategory = ToolCategory.Height;

        [SerializeField]
        public int TextureLayerIndex = 1;

        [SerializeField]
        public float TextureLayerStrength = 1.0f;

        [SerializeField]
        public float HoleThreshold = 0.0f;

        [SerializeField]
        public TreePrototypeData[] TreePrototypeDataList = new TreePrototypeData[0];

        [SerializeField]
        public WriteCondition WriteCondition = default;

        [SerializeField]
        public int Layer = 0;

        [SerializeField]
        public int OrderInLayer = 0;
    }
}
