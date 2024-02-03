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
        private int _treeDencityMapResolution = 500;

        public int TreeDencityMapResolution => _treeDencityMapResolution;
    }
}
