using System.Collections.Generic;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainDetailPainter
    {
        private readonly Terrain _terrain;

        public TerrainDetailPainter(Terrain terrain)
        {
            _terrain = terrain;
        }

        public void Paint(IEnumerable<ITerrainParts> parts)
        {
            var terrainData = _terrain.terrainData;
            var prototypeCount = terrainData.detailPrototypes.Length;
            var detailMapSize = new Vector2Int(terrainData.detailWidth, terrainData.detailHeight);
            var densityIntValueMaps = new int[detailMapSize.y, detailMapSize.x, prototypeCount];
            for (var x = 0; x < detailMapSize.x; x++)
            {
                for (var y = 0; y < detailMapSize.y; y++)
                {
                    for (var layer = 0; layer < prototypeCount; layer++)
                    {
                        densityIntValueMaps[y, x, layer] = 0;
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
                PainterUtility.CalculatePixelRange(minX, minZ, maxX, maxZ, _terrain, detailMapSize, out var pixelBase, out var pixelSize);
                for (var x = 0; x < pixelSize.x; x++)
                {
                    for (var y = 0; y < pixelSize.y; y++)
                    {
                        var pixelPos = new Vector2Int(pixelBase.x + x, pixelBase.y + y);
                        Vector3 worldPos = PainterUtility.PixelToWorld(pixelPos, detailMapSize, _terrain);
                        if (part.TryGetAlpha(worldPos.x, worldPos.z, out var resultAlpha))
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
                                var currentDensityIntValue = densityIntValueMaps[pixelPos.y, pixelPos.x, prototypeIndex];
                                densityIntValueMaps[pixelPos.y, pixelPos.x, prototypeIndex] = Mathf.Max(currentDensityIntValue, resultDensity);
                            }
                        }
                    }
                }
            }

            for (int prototypeIndex = 0; prototypeIndex < prototypeCount; prototypeIndex++)
            {
                var layerData = new int[detailMapSize.y, detailMapSize.x];
                for (int x = 0; x < detailMapSize.x; x++)
                {
                    for (int y = 0; y < detailMapSize.y; y++)
                    {
                        layerData[y, x] = densityIntValueMaps[y, x, prototypeIndex];
                    }
                }
                terrainData.SetDetailLayer(0, 0, prototypeIndex, layerData);
            }
        }
    }
}
