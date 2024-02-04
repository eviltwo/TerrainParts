using System.Collections.Generic;
using UnityEngine;

namespace TerrainParts.Editor
{
    public class TerrainHolePainter
    {
        private readonly Terrain _terrain;

        public TerrainHolePainter(Terrain terrain)
        {
            _terrain = terrain;
        }

        public void Paint(IEnumerable<ITerrainParts> parts)
        {
            var terrainData = _terrain.terrainData;
            var resolution = new Vector2Int(terrainData.holesResolution, terrainData.holesResolution);
            var boolMap = new bool[resolution.y, resolution.x];
            for (var x = 0; x < resolution.x; x++)
            {
                for (var y = 0; y < resolution.y; y++)
                {
                    boolMap[y, x] = true;
                }
            }

            foreach (var part in parts)
            {
                var basicData = part.GetBasicData();
                part.GetRect(out var minX, out var minZ, out var maxX, out var maxZ);
                PainterUtility.CalculatePixelRange(minX, minZ, maxX, maxZ, _terrain, resolution, out var pixelBase, out var pixelSize);
                for (var x = 0; x < pixelSize.x; x++)
                {
                    for (var y = 0; y < pixelSize.y; y++)
                    {
                        var pixelPos = new Vector2Int(pixelBase.x + x, pixelBase.y + y);
                        var worldPos = PainterUtility.PixelToWorld(pixelPos, resolution, _terrain);
                        if (part.TryGetAlpha(worldPos.x, worldPos.z, out var resultAlpha))
                        {
                            var digHole = resultAlpha > basicData.HoleThreshold;
                            boolMap[pixelPos.y, pixelPos.x] = !digHole;
                        }
                    }
                }
            }

            terrainData.SetHoles(0, 0, boolMap);
        }
    }
}
