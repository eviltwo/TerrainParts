using UnityEngine;

namespace TerrainParts.Editor
{
    [CreateAssetMenu(fileName = "TerrainPartsBuildSettings", menuName = "TerrainParts/BuildSettings")]
    public class TerrainPartsBuildSettings : ScriptableObject
    {
        [SerializeField]
        private int _randomSeed = 0;

        public int RandomSeed => _randomSeed;

        [SerializeField]
        private TreePainterSettings _treePainterSettings = default;

        public TreePainterSettings TreePainterSettings => _treePainterSettings;
    }

    [System.Serializable]
    public class TreePainterSettings
    {
        [SerializeField]
        private int _treeDencityMapResolution = 500;

        public int TreeDencityMapResolution => _treeDencityMapResolution;

        [System.Serializable]
        public class TreeDetail
        {
            [SerializeField]
            private bool _isHeightRandom = true;

            public bool IsHeightRandom => _isHeightRandom;

            [SerializeField]
            public float _heightRandomMin = 1.0f;

            public float HeightRandomMin => _heightRandomMin;

            [SerializeField]
            public float _heightRandomMax = 1.2f;

            public float HeightRandomMax => _heightRandomMax;

            [SerializeField]
            public bool _isLockWidthToHeight = true;

            public bool IsLockWidthToHeight => _isLockWidthToHeight;

            [SerializeField]
            public bool _isWidthRandom = true;

            public bool IsWidthRandom => _isWidthRandom;

            [SerializeField]
            public float _widthRandomMin = 1.0f;

            public float WidthRandomMin => _widthRandomMin;

            [SerializeField]
            public float _widthRandomMax = 1.2f;

            public float WidthRandomMax => _widthRandomMax;
        }

        [System.Serializable]
        public class TreePrototypeDetail
        {
            [SerializeField]
            public int _prototypeIndex = 0;

            public int PrototypeIndex => _prototypeIndex;

            [SerializeField]
            public TreeDetail _detail = default;

            public TreeDetail Detail => _detail;
        }

        [SerializeField]
        private TreeDetail _defaultTreeDetail = default;

        [SerializeField]
        private TreePrototypeDetail[] _overrideTreeDetails = new TreePrototypeDetail[0];

        public TreeDetail GetTreeDetail(int prototypeIndex)
        {
            var count = _overrideTreeDetails.Length;
            for (int i = 0; i < count; i++)
            {
                if (_overrideTreeDetails[i].PrototypeIndex == prototypeIndex)
                {
                    return _overrideTreeDetails[i].Detail;
                }
            }

            return _defaultTreeDetail;
        }
    }
}
