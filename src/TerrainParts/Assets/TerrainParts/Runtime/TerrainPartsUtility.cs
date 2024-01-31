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
    }
}
