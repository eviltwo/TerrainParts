using System.Collections.Generic;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainTreePainter
    {
        private const float MaxDencityPerUnit = 2;
        private readonly Terrain _terrain;
        private readonly TreePainterSettings _settings;
        private readonly System.Random _random;

        public TerrainTreePainter(Terrain terrain, TreePainterSettings settings, System.Random random)
        {
            _terrain = terrain;
            _settings = settings;
            _random = random;
        }

        public void Paint(IEnumerable<ITerrainParts> parts)
        {
            var terrainData = _terrain.terrainData;
            var prototypeCount = terrainData.treePrototypes.Length;
            var densityMaps = new float[_settings.TreeDencityMapResolution, _settings.TreeDencityMapResolution, prototypeCount];
            for (var x = 0; x < _settings.TreeDencityMapResolution; x++)
            {
                for (var y = 0; y < _settings.TreeDencityMapResolution; y++)
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
                var baseX = Mathf.CeilToInt(minX / terrainData.size.x * _settings.TreeDencityMapResolution);
                var baseZ = Mathf.CeilToInt(minZ / terrainData.size.z * _settings.TreeDencityMapResolution);
                var exX = Mathf.CeilToInt(maxX / terrainData.size.x * _settings.TreeDencityMapResolution);
                var exZ = Mathf.CeilToInt(maxZ / terrainData.size.z * _settings.TreeDencityMapResolution);
                var xResolution = exX - baseX;
                var zResolution = exZ - baseZ;
                for (var x = 0; x < xResolution; x++)
                {
                    for (var z = 0; z < zResolution; z++)
                    {
                        var pixelX = baseX + x;
                        var pixelZ = baseZ + z;
                        var worldX = _terrain.transform.position.x + (float)pixelX / _settings.TreeDencityMapResolution * terrainData.size.x;
                        var worldZ = _terrain.transform.position.z + (float)pixelZ / _settings.TreeDencityMapResolution * terrainData.size.z;
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

            var pixelSizeX = _terrain.terrainData.size.x / _settings.TreeDencityMapResolution;
            var pixelSizeZ = _terrain.terrainData.size.z / _settings.TreeDencityMapResolution;
            var pixelArea = pixelSizeX * pixelSizeZ;
            var treeInstances = new List<TreeInstance>();
            for (var x = 0; x < _settings.TreeDencityMapResolution; x++)
            {
                for (var z = 0; z < _settings.TreeDencityMapResolution; z++)
                {
                    var localX = (float)x / _settings.TreeDencityMapResolution;
                    var localZ = (float)z / _settings.TreeDencityMapResolution;
                    for (int prototypeIndex = 0; prototypeIndex < prototypeCount; prototypeIndex++)
                    {
                        var treeDetail = _settings.GetTreeDetail(prototypeIndex);
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
                                localX + (float)_random.NextDouble() / _settings.TreeDencityMapResolution,
                                0,
                                localZ + (float)_random.NextDouble() / _settings.TreeDencityMapResolution);
                            var rotation = (float)_random.NextDouble() * 2 * Mathf.PI;
                            CalculateTreeScales(treeDetail, _random, out var width, out var height);
                            var treeInstance = new TreeInstance
                            {
                                position = position,
                                rotation = rotation,
                                prototypeIndex = prototypeIndex,
                                widthScale = width,
                                heightScale = height,
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

        private static void CalculateTreeScales(TreePainterSettings.TreeDetail detail, System.Random random, out float width, out float height)
        {
            if (detail.IsHeightRandom)
            {
                height = Mathf.Lerp(detail.HeightRandomMin, detail.HeightRandomMax, (float)random.NextDouble());
            }
            else
            {
                height = detail.HeightRandomMin;
            }

            if (detail.IsLockWidthToHeight)
            {
                width = height;
            }
            else if (detail.IsWidthRandom)
            {
                width = Mathf.Lerp(detail.WidthRandomMin, detail.WidthRandomMax, (float)random.NextDouble());
            }
            else
            {
                width = detail.WidthRandomMin;
            }
        }
    }
}
