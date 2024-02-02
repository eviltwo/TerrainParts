using System.Collections.Generic;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainTreePainter
    {
        private const float MaxDencityPerUnit = 2;
        private readonly Terrain _terrain;
        private readonly int _resolution;
        private readonly System.Random _random;

        public TerrainTreePainter(Terrain terrain, int resolution, System.Random random)
        {
            _terrain = terrain;
            _resolution = resolution;
            _random = random;
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
                            for (int dataIndex = 0; dataIndex < prototypeDataCount; dataIndex++)
                            {
                                var prototypeData = basicData.TreePrototypeDataList[dataIndex];
                                var prototypeIndex = prototypeData.Index;
                                if (prototypeIndex >= prototypeCount)
                                {
                                    break;
                                }
                                var resultDensity = resultAlpha * prototypeData.DensityPerUnit;
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
            var treeInstances = new List<TreeInstance>();
            for (var x = 0; x < _resolution; x++)
            {
                for (var z = 0; z < _resolution; z++)
                {
                    var localX = (float)x / _resolution;
                    var localZ = (float)z / _resolution;
                    for (int prototypeIndex = 0; prototypeIndex < prototypeCount; prototypeIndex++)
                    {
                        var densityPerUnit = Mathf.Min(densityMaps[z, x, prototypeIndex], MaxDencityPerUnit);
                        if (densityPerUnit <= 0)
                        {
                            continue;
                        }
                        var densityPerPixel = pixelArea * densityPerUnit;
                        var treeCount = Mathf.FloorToInt(densityPerPixel) + (_random.NextDouble() < densityPerPixel % 1 ? 1 : 0);
                        for (var i = 0; i < treeCount; i++)
                        {
                            var position = new Vector3(
                                localX + (float)_random.NextDouble() / _resolution,
                                0,
                                localZ + (float)_random.NextDouble() / _resolution);
                            var rotation = (float)_random.NextDouble() * 2 * Mathf.PI;
                            var treeInstance = new TreeInstance
                            {
                                position = position,
                                rotation = rotation,
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
        }
    }
}
