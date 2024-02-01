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

        public ToolCategory GetToolCategory() => _basicData.ToolCategory;

        public int GetTextureLayerIndex() => _basicData.TextureLayerIndex;

        public int GetLayer() => _basicData.Layer;

        public int GetOrderInLayer() => _basicData.OrderInLayer;

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

        public float GetHeight(float worldX, float worldZ, float currentHeight)
        {
            var surface = FitToSurfaceWithCache(new Vector3(worldX, 0, worldZ));
            var localSurface = transform.InverseTransformPoint(surface);
            var isInside = localSurface.x >= -0.5f && localSurface.x <= 0.5f && localSurface.z >= -0.5f && localSurface.z <= 0.5f;
            if (!isInside)
            {
                return currentHeight;
            }
            var targetHeight = TerrainPartsUtility.MergeHeight(currentHeight, surface.y, _basicData.WriteCondition);
            var color = _copiedTexture.GetPixelBilinear(Mathf.Clamp01(localSurface.x + 0.5f), Mathf.Clamp01(localSurface.z + 0.5f));
            var alpha = Mathf.Clamp01(color.a);
            return Mathf.Lerp(currentHeight, targetHeight, alpha);
        }

        public float GetAlpha(float worldX, float worldZ, float currentAlpha)
        {
            var surface = FitToSurfaceWithCache(new Vector3(worldX, 0, worldZ));
            var localSurface = transform.InverseTransformPoint(surface);
            var isInside = localSurface.x >= -0.5f && localSurface.x <= 0.5f && localSurface.z >= -0.5f && localSurface.z <= 0.5f;
            if (!isInside)
            {
                return currentAlpha;
            }
            var color = _copiedTexture.GetPixelBilinear(Mathf.Clamp01(localSurface.x + 0.5f), Mathf.Clamp01(localSurface.z + 0.5f));
            var alpha = _basicData.Strength * Mathf.Clamp01(color.a);
            return Mathf.Clamp01(currentAlpha + alpha);
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
                Debug.LogError($"Failed to fit surface. {transform.position}, {worldPosition}");
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
