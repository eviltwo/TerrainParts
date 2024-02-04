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
            var densityMapSize = new Vector2Int(_settings.TreeDensityMapResolution, _settings.TreeDensityMapResolution);
            var densityMaps = new float[densityMapSize.y, densityMapSize.x, prototypeCount];
            for (var x = 0; x < densityMapSize.x; x++)
            {
                for (var y = 0; y < densityMapSize.y; y++)
                {
                    for (var layer = 0; layer < prototypeCount; layer++)
                    {
                        densityMaps[y, x, layer] = 0;
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
                PainterUtility.CalculatePixelRange(minX, minZ, maxX, maxZ, _terrain, densityMapSize, out var basePixel, out var pixelSize);
                for (var x = 0; x < pixelSize.x; x++)
                {
                    for (var y = 0; y < pixelSize.y; y++)
                    {
                        var pixelPos = new Vector2Int(basePixel.x + x, basePixel.y + y);
                        var worldPos = PainterUtility.PixelToWorld(pixelPos, densityMapSize, _terrain);
                        if (part.TryGetAlpha(worldPos.x, worldPos.z, out var resultAlpha))
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
                                var currentDensity = densityMaps[pixelPos.y, pixelPos.x, prototypeIndex];
                                densityMaps[pixelPos.y, pixelPos.x, prototypeIndex] = Mathf.Max(currentDensity, resultDensity);
                            }
                        }
                    }
                }
            }

            var pixelSizeX = _terrain.terrainData.size.x / densityMapSize.x;
            var pixelSizeZ = _terrain.terrainData.size.z / densityMapSize.y;
            var pixelArea = pixelSizeX * pixelSizeZ;
            var treeInstances = new List<TreeInstance>();
            for (var x = 0; x < densityMapSize.x; x++)
            {
                for (var y = 0; y < densityMapSize.y; y++)
                {
                    var localX = (float)x / densityMapSize.x;
                    var localZ = (float)y / densityMapSize.y;
                    for (int prototypeIndex = 0; prototypeIndex < prototypeCount; prototypeIndex++)
                    {
                        var treeDetail = _settings.GetTreeDetail(prototypeIndex);
                        var densityPerUnit = Mathf.Min(densityMaps[y, x, prototypeIndex], MaxDencityPerUnit);
                        if (densityPerUnit <= 0)
                        {
                            continue;
                        }
                        var densityPerPixel = pixelArea * densityPerUnit;
                        var treeCount = Mathf.FloorToInt(densityPerPixel) + (_random.NextDouble() < densityPerPixel % 1 ? 1 : 0);
                        for (var i = 0; i < treeCount; i++)
                        {
                            var position = new Vector3(
                                localX + (float)_random.NextDouble() / densityMapSize.x,
                                0,
                                localZ + (float)_random.NextDouble() / densityMapSize.y);
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
