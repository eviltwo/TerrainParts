using UnityEngine;

namespace TerrainParts
{
    public static class TerrainPartsUtility
    {
        public static float MergeHeight(float sourceHeight, float targetHeight, WriteCondition condition)
        {
            if (condition == WriteCondition.Always)
            {
                return targetHeight;
            }
            if (condition == WriteCondition.IfHigher)
            {
                return Mathf.Max(sourceHeight, targetHeight);
            }
            else if (condition == WriteCondition.IfLower)
            {
                return Mathf.Min(sourceHeight, targetHeight);
            }
            else
            {
                return sourceHeight;
            }
        }

        public static int CompareOrderInLayer(ITerrainParts a, ITerrainParts b)
        {
            var basicDataA = a.GetBasicData();
            var basicDataB = b.GetBasicData();
            var layer = basicDataA.Layer.CompareTo(basicDataB.Layer);
            if (layer != 0)
            {
                return layer;
            }
            return basicDataA.OrderInLayer.CompareTo(basicDataB.OrderInLayer);
        }
    }
}
