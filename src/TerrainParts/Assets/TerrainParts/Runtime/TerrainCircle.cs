using UnityEngine;

namespace TerrainParts
{
    public class TerrainCircle : MonoBehaviour, ITerrainParts
    {
        [SerializeField]
        private Texture2D _alphaTexture = null;

        [SerializeField]
        private WriteCondition _writeCondition = default;

        [SerializeField]
        private int _layer = 0;

        [SerializeField]
        private int _orderInLayer = 0;

        public int Layer
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public int OrderInLayer
        {
            get { return _orderInLayer; }
            set { _orderInLayer = value; }
        }

        public int GetLayer() => _layer;

        public int GetOrderInLayer() => _orderInLayer;

        private Texture2D _copiedTexture = null;

        public void GetRect(out float minX, out float minZ, out float maxX, out float maxZ)
        {
            const int AngleResolution = 32;
            minX = float.MaxValue;
            minZ = float.MaxValue;
            maxX = float.MinValue;
            maxZ = float.MinValue;
            for (int i = 0; i < AngleResolution; i++)
            {
                var angle = 360f / AngleResolution * i;
                var position = GetEdgePosition(angle);
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
        }

        public float GetHeight(float worldX, float worldZ, float currentHeight)
        {
            var surface = FitSurface(new Vector3(worldX, 0, worldZ));
            var localSurface = transform.InverseTransformPoint(surface);
            var isInside = localSurface.x * localSurface.x + localSurface.z * localSurface.z < 0.5f * 0.5f;
            if (!isInside)
            {
                return currentHeight;
            }
            var targetHeight = TerrainPartsUtility.MergeHeight(currentHeight, surface.y, _writeCondition);
            var color = _copiedTexture.GetPixelBilinear(localSurface.x + 0.5f, localSurface.z + 0.5f);
            var alpha = Mathf.Clamp01(color.r);
            return Mathf.Lerp(currentHeight, targetHeight, alpha);
        }

        private void OnDrawGizmosSelected()
        {
            GetRect(out var minX, out var minZ, out var maxX, out var maxZ);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(new Vector3((minX + maxX) / 2, 0, (minZ + maxZ) / 2), new Vector3(maxX - minX, 0, maxZ - minZ));
        }

        private void OnDrawGizmos()
        {
            const int AngleResolution = 32;
            Gizmos.color = Color.blue;
            for (int i = 0; i < AngleResolution; i++)
            {
                var angle = 360f / AngleResolution * i;
                var nextAngle = 360f / AngleResolution * (i + 1);
                Gizmos.DrawLine(GetEdgePosition(angle), GetEdgePosition(nextAngle));
            }
        }

        private Vector3 GetEdgePosition(float angle)
        {
            var radian = Mathf.Deg2Rad * angle;
            var localX = Mathf.Cos(radian) * 0.5f;
            var localZ = Mathf.Sin(radian) * 0.5f;
            return transform.TransformPoint(new Vector3(localX, 0, localZ));
        }

        private Vector3 FitSurface(Vector3 worldPosition)
        {
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
                Debug.LogError("Failed to fit surface");
                return worldPosition;
            }
        }
    }
}
