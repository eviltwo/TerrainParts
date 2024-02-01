using UnityEngine;

namespace TerrainParts
{
    [System.Serializable]
    public class TerrainPartsBasicData
    {
        [SerializeField]
        public ToolCategory ToolCategory = ToolCategory.Height;

        [SerializeField]
        public int TextureLayerIndex = 0;

        [SerializeField]
        public float TextureLayerAlpha = 1.0f;

        [SerializeField]
        public WriteCondition WriteCondition = default;

        [SerializeField]
        public int Layer = 0;

        [SerializeField]
        public int OrderInLayer = 0;
    }
}
