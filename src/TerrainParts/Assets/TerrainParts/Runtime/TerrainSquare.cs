using UnityEngine;

namespace TerrainParts
{
    public class TerrainSquare : MonoBehaviour, ITerrainParts
    {
        [SerializeField]
        private Texture2D _alphaTexture = null;

        [SerializeField]
        private TerrainPartsBasicData _basicData = default;

        private static readonly Vector3[] _corners = new Vector3[]
        {
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, -0.5f),
        };

        private Texture2D _copiedTexture = null;
        private float _cachedOriginY;
        private float _cachedRightY;
        private float _cachedForwardY;

        public TerrainPartsBasicData GetBasicData()
        {
            return _basicData;
        }

        public void GetRect(out float minX, out float minZ, out float maxX, out float maxZ)
        {
            minX = float.MaxValue;
            minZ = float.MaxValue;
            maxX = float.MinValue;
            maxZ = float.MinValue;
            var cornersCount = _corners.Length;
            for (int i = 0; i < cornersCount; i++)
            {
                var position = transform.TransformPoint(_corners[i]);
                if (position.x < minX) minX = position.x;
                if (position.z < minZ) minZ = position.z;
                if (position.x > maxX) maxX = position.x;
                if (position.z > maxZ) maxZ = position.z;
            }
        }

        public void Setup(float unitPerPixel)
        {
            var texture = _alphaTexture == null ? Texture2D.whiteTexture : _alphaTexture;
            _copiedTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount, true);
            Graphics.CopyTexture(texture, _copiedTexture);

            _cachedOriginY = FitToSurface(transform.position).y;
            _cachedRightY = FitToSurface(transform.position + Vector3.right).y;
            _cachedForwardY = FitToSurface(transform.position + Vector3.forward).y;
        }

        public bool TryGetHeight(float worldX, float worldZ, out float resultHeight, out float resultAlpha)
        {
            var surface = FitToSurfaceWithCache(new Vector3(worldX, 0, worldZ));
            var localSurface = transform.InverseTransformPoint(surface);
            var isInside = localSurface.x >= -0.5f && localSurface.x <= 0.5f && localSurface.z >= -0.5f && localSurface.z <= 0.5f;
            if (!isInside)
            {
                resultHeight = 0;
                resultAlpha = 0;
                return false;
            }
            resultHeight = surface.y;
            var color = _copiedTexture.GetPixelBilinear(Mathf.Clamp01(localSurface.x + 0.5f), Mathf.Clamp01(localSurface.z + 0.5f));
            resultAlpha = Mathf.Clamp01(color.a);
            return true;
        }

        public bool TryGetAlpha(float worldX, float worldZ, out float resultAlpha)
        {
            var surface = FitToSurfaceWithCache(new Vector3(worldX, 0, worldZ));
            var localSurface = transform.InverseTransformPoint(surface);
            var isInside = localSurface.x >= -0.5f && localSurface.x <= 0.5f && localSurface.z >= -0.5f && localSurface.z <= 0.5f;
            if (!isInside)
            {
                resultAlpha = 0;
                return false;
            }
            var color = _copiedTexture.GetPixelBilinear(Mathf.Clamp01(localSurface.x + 0.5f), Mathf.Clamp01(localSurface.z + 0.5f));
            resultAlpha = _basicData.Strength * Mathf.Clamp01(color.a);
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            var cornersCount = _corners.Length;
            for (int i = 0; i < cornersCount; i++)
            {
                var p1 = transform.TransformPoint(_corners[i]);
                var p2 = transform.TransformPoint(_corners[(i + 1) % cornersCount]);
                Gizmos.DrawLine(p1, p2);
            }
        }

        private Vector3 FitToSurface(Vector3 worldPosition)
        {
            worldPosition.y = 0;
            var plane = new Plane(transform.up, transform.position);
            if (plane.Raycast(new Ray(worldPosition, Vector3.up), out var distance))
            {
                return new Vector3(worldPosition.x, worldPosition.y + distance, worldPosition.z);
            }
            else if (plane.Raycast(new Ray(worldPosition, Vector3.down), out distance))
            {
                return new Vector3(worldPosition.x, worldPosition.y - distance, worldPosition.z);
            }
            else
            {
                return worldPosition;
            }
        }

        private Vector3 FitToSurfaceWithCache(Vector3 worldPosition)
        {
            var diff = worldPosition - transform.position;
            var xy = (_cachedRightY - _cachedOriginY) * diff.x;
            var zy = (_cachedForwardY - _cachedOriginY) * diff.z;
            return new Vector3(worldPosition.x, xy + zy + _cachedOriginY, worldPosition.z);
        }
    }
}
