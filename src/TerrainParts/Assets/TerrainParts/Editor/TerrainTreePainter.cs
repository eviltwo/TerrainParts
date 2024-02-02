using System.Collections.Generic;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainTreePainter
    {
        private const int DencityLimit = 100;
        private readonly Terrain _terrain;
        private readonly int _resolution;
        private readonly int _randomSeed;

        public TerrainTreePainter(Terrain terrain, int resolution, int randomSeed)
        {
            _terrain = terrain;
            _resolution = resolution;
            _randomSeed = randomSeed;
        }

        public void Paint(IEnumerable<ITerrainParts> parts)
        {
            var terrainData = _terrain.terrainData;
            var prototypeCount = terrainData.treePrototypes.Length;
            var densityMaps = new float[_resolution, _resolution, prototypeCount];
            for (var x = 0; x < _resolution; x++)
            {
                for (var y = 0; y < _resolution; y++)
                {
                    for (var layer = 0; layer < prototypeCount; layer++)
                    {
                        densityMaps[x, y, layer] = 0;
                    }
                }
            }

            foreach (var part in parts)
            {
                var basicData = part.GetBasicData();
                var prototypeDataCount = basicData.TreePrototypeDataList.Length;
                if (prototypeDataCount == 0)
                {
                    continue;
                }
                part.GetRect(out var minX, out var minZ, out var maxX, out var maxZ);
                minX = Mathf.Max(minX - _terrain.transform.position.x, 0);
                minZ = Mathf.Max(minZ - _terrain.transform.position.z, 0);
                maxX = Mathf.Min(maxX - _terrain.transform.position.x, _terrain.terrainData.size.x);
                maxZ = Mathf.Min(maxZ - _terrain.transform.position.z, _terrain.terrainData.size.z);
                var baseX = Mathf.CeilToInt(minX / terrainData.size.x * _resolution);
                var baseZ = Mathf.CeilToInt(minZ / terrainData.size.z * _resolution);
                var exX = Mathf.CeilToInt(maxX / terrainData.size.x * _resolution);
                var exZ = Mathf.CeilToInt(maxZ / terrainData.size.z * _resolution);
                var xResolution = exX - baseX;
                var zResolution = exZ - baseZ;
                for (var x = 0; x < xResolution; x++)
                {
                    for (var z = 0; z < zResolution; z++)
                    {
                        var pixelX = baseX + x;
                        var pixelZ = baseZ + z;
                        var worldX = _terrain.transform.position.x + (float)pixelX / _resolution * terrainData.size.x;
                        var worldZ = _terrain.transform.position.z + (float)pixelZ / _resolution * terrainData.size.z;
                        if (part.TryGetAlpha(worldX, worldZ, out var resultAlpha))
                        {
                            for (int prototypeIndex = 0; prototypeIndex < prototypeDataCount; prototypeIndex++)
                            {
                                if (prototypeIndex >= prototypeCount)
                                {
                                    break;
                                }
                                var prototypeData = basicData.TreePrototypeDataList[prototypeIndex];
                                var resultDensity = resultAlpha * prototypeData.Density;
                                var currentDensity = densityMaps[pixelZ, pixelX, prototypeIndex];
                                densityMaps[pixelZ, pixelX, prototypeIndex] = Mathf.Max(currentDensity, resultDensity);
                            }
                        }
                    }
                }
            }

            var pixelSizeX = _terrain.terrainData.size.x / _resolution;
            var pixelSizeZ = _terrain.terrainData.size.z / _resolution;
            var pixelArea = pixelSizeX * pixelSizeZ;
            var random = new System.Random(_randomSeed);
            var treeInstances = new List<TreeInstance>();
            for (var x = 0; x < _resolution; x++)
            {
                for (var z = 0; z < _resolution; z++)
                {
                    var localX = (float)x / _resolution;
                    var localZ = (float)z / _resolution;
                    for (int prototypeIndex = 0; prototypeIndex < prototypeCount; prototypeIndex++)
                    {
                        var density = Mathf.Min(densityMaps[z, x, prototypeIndex], DencityLimit);
                        if (density <= 0)
                        {
                            continue;
                        }
                        var localDensity = pixelArea * density;
                        var treeCount = Mathf.FloorToInt(localDensity) + (random.NextDouble() < localDensity % 1 ? 1 : 0);
                        for (var i = 0; i < treeCount && i < 10; i++)
                        {
                            var position = new Vector3(
                                localX + (float)random.NextDouble() / _resolution,
                                0,
                                localZ + (float)random.NextDouble() / _resolution);
                            var treeInstance = new TreeInstance
                            {
                                position = position,
                                prototypeIndex = prototypeIndex,
                                widthScale = 1,
                                heightScale = 1,
                                color = Color.white,
                                lightmapColor = Color.white,
                            };
                            treeInstances.Add(treeInstance);
                        }
                    }
                }
            }

            terrainData.SetTreeInstances(treeInstances.ToArray(), true);
            Debug.Log(terrainData.treeInstanceCount);
        }
    }
}
