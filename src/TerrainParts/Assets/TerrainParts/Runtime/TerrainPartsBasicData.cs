using UnityEngine;
using UnityEngine.Serialization;

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

        [System.Serializable]
        public class DetailPrototypeData
        {
            [SerializeField]
            public int Index = 0;

            [SerializeField]
            public int Density = 1;
        }

        [SerializeField]
        public ToolCategory ToolCategory = ToolCategory.Height;

        [SerializeField, FormerlySerializedAs("WriteCondition")]
        public WriteCondition HeightWriteCondition = default;

        [SerializeField]
        public int TextureLayerIndex = 1;

        [SerializeField]
        public float TextureLayerStrength = 1.0f;

        [SerializeField]
        public float HoleThreshold = 0.0f;

        [SerializeField]
        public TreePrototypeData[] TreePrototypeDataList = new TreePrototypeData[0];

        [SerializeField]
        public DetailPrototypeData[] DetailPrototypeDataList = new DetailPrototypeData[0];

        [SerializeField]
        public int Layer = 0;

        [SerializeField]
        public int OrderInLayer = 0;
    }
}
