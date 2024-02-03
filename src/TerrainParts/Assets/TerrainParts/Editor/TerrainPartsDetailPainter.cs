using System.Collections.Generic;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainPartsDetailPainter
    {
        private readonly Terrain _terrain;

        public TerrainPartsDetailPainter(Terrain terrain)
        {
            _terrain = terrain;
        }

        public void Paint(IEnumerable<ITerrainParts> parts)
        {
            var terrainData = _terrain.terrainData;
            var prototypeCount = terrainData.detailPrototypes.Length;
            var detailWidth = terrainData.detailWidth;
            var detailHeight = terrainData.detailHeight;
            var densityIntValueMaps = new int[detailHeight, detailWidth, prototypeCount];
            for (var x = 0; x < detailWidth; x++)
            {
                for (var y = 0; y < detailHeight; y++)
                {
                    for (var layer = 0; layer < prototypeCount; layer++)
                    {
                        densityIntValueMaps[y, x, layer] = layer == 0 ? 1 : 0;
                    }
                }
            }

            foreach (var part in parts)
            {
                var basicData = part.GetBasicData();
                var prototypeDataCount = basicData.DetailPrototypeDataList.Length;
                if (prototypeDataCount == 0)
                {
                    continue;
                }
                part.GetRect(out var minX, out var minZ, out var maxX, out var maxZ);
                minX = Mathf.Max(minX - _terrain.transform.position.x, 0);
                minZ = Mathf.Max(minZ - _terrain.transform.position.z, 0);
                maxX = Mathf.Min(maxX - _terrain.transform.position.x, _terrain.terrainData.size.x);
                maxZ = Mathf.Min(maxZ - _terrain.transform.position.z, _terrain.terrainData.size.z);
                var baseX = Mathf.CeilToInt(minX / terrainData.size.x * detailWidth);
                var baseZ = Mathf.CeilToInt(minZ / terrainData.size.z * detailHeight);
                var exX = Mathf.CeilToInt(maxX / terrainData.size.x * detailWidth);
                var exZ = Mathf.CeilToInt(maxZ / terrainData.size.z * detailHeight);
                var xResolution = exX - baseX;
                var zResolution = exZ - baseZ;
                for (var x = 0; x < xResolution; x++)
                {
                    for (var z = 0; z < zResolution; z++)
                    {
                        var pixelX = baseX + x;
                        var pixelZ = baseZ + z;
                        var worldX = _terrain.transform.position.x + (float)pixelX / detailWidth * terrainData.size.x;
                        var worldZ = _terrain.transform.position.z + (float)pixelZ / detailHeight * terrainData.size.z;
                        if (part.TryGetAlpha(worldX, worldZ, out var resultAlpha))
                        {
                            for (int dataIndex = 0; dataIndex < prototypeDataCount; dataIndex++)
                            {
                                var prototypeData = basicData.DetailPrototypeDataList[dataIndex];
                                var prototypeIndex = prototypeData.Index;
                                if (prototypeIndex >= prototypeCount)
                                {
                                    break;
                                }
                                var resultDensity = Mathf.RoundToInt(resultAlpha * prototypeData.Density);
                                var currentDensityIntValue = densityIntValueMaps[pixelZ, pixelX, prototypeIndex];
                                densityIntValueMaps[pixelZ, pixelX, prototypeIndex] = Mathf.Max(currentDensityIntValue, resultDensity);
                            }
                        }
                    }
                }
            }

            for (int prototypeIndex = 0; prototypeIndex < prototypeCount; prototypeIndex++)
            {
                var layerData = new int[detailWidth, detailHeight];
                for (int x = 0; x < detailWidth; x++)
                {
                    for (int y = 0; y < detailHeight; y++)
                    {
                        layerData[y, x] = densityIntValueMaps[y, x, prototypeIndex];
                    }
                }
                terrainData.SetDetailLayer(0, 0, prototypeIndex, layerData);
            }
        }
    }
}
