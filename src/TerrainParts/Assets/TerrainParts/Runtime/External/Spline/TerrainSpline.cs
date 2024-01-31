#if SPLINES_SUPPORTED
using UnityEngine;
using UnityEngine.Splines;

namespace TerrainParts.Splines
{
    [RequireComponent(typeof(SplineContainer))]
    public class TerrainSpline : MonoBehaviour, ITerrainParts
    {
        [SerializeField]
        private SplineContainer _splineContainer = null;

        [SerializeField]
        private float _width = 1f;

        [SerializeField]
        private Vector3 _offset = Vector3.zero;

        [SerializeField]
        private Texture2D _alphaTexture = null;

        [SerializeField]
        private WriteCondition _writeCondition = default;

        [SerializeField]
        private WriteCondition _innerMapWriteCondition = default;

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

        private Vector2 _mapMin;
        private Vector2 _mapMax;
        private Vector2Int _mapResolution;
        private float[,] _innerHeightMap;

        private void Reset()
        {
            _splineContainer = GetComponent<SplineContainer>();
        }

        public int GetLayer() => _layer;

        public int GetOrderInLayer() => _orderInLayer;

        public void Setup(float unitPerPixel)
        {
            GetRect(out var minX, out var minZ, out var maxX, out var maxZ);
            _mapMin = new Vector2(minX, minZ);
            _mapMax = new Vector2(maxX, maxZ);
            var width = maxX - minX;
            var height = maxZ - minZ;
            _mapResolution = new Vector2Int(Mathf.CeilToInt(width / unitPerPixel), Mathf.CeilToInt(height / unitPerPixel));
            _innerHeightMap = new float[_mapResolution.y, _mapResolution.x];
            for (int x = 0; x < _mapResolution.x; x++)
            {
                for (int y = 0; y < _mapResolution.y; y++)
                {
                    _innerHeightMap[y, x] = -1;
                }
            }

            var horizontalResolution = Mathf.CeilToInt(width / unitPerPixel);

            var splines = _splineContainer.Splines;
            var splineCount = splines.Count;
            for (int i = 0; i < splineCount; i++)
            {
                var spline = splines[i];
                var splineLength = spline.GetLength();
                var splineResolution = Mathf.CeilToInt(splineLength / unitPerPixel);
                for (int j = 0; j < splineResolution; j++)
                {
                    var t = (float)j / (splineResolution - 1);
                    if (spline.Evaluate(t, out var position, out var tangent, out var up))
                    {
                        var worldPosition = transform.TransformPoint(position);
                        var worldTangent = transform.TransformDirection(tangent);
                        var worldUp = transform.TransformDirection(up);
                        var worldRight = Vector3.Cross(worldTangent, worldUp).normalized;
                        for (int x = 0; x < horizontalResolution; x++)
                        {
                            var hrizonT = (float)x / (horizontalResolution - 1);
                            var horizonPosition = worldPosition + worldRight * _width * (hrizonT - 0.5f);
                            var horizonHeight = horizonPosition.y;
                            var innerHeightMapPosition = horizonPosition - new Vector3(minX, 0, minZ);
                            var innerHeightMapX = (int)(innerHeightMapPosition.x / unitPerPixel);
                            var innerHeightMapZ = (int)(innerHeightMapPosition.z / unitPerPixel);
                            var existingHeight = _innerHeightMap[innerHeightMapZ, innerHeightMapX];
                            if (existingHeight < 0)
                            {
                                _innerHeightMap[innerHeightMapZ, innerHeightMapX] = horizonHeight;
                            }
                            else
                            {
                                _innerHeightMap[innerHeightMapZ, innerHeightMapX] = TerrainPartsUtility.MergeHeight(existingHeight, horizonHeight, _innerMapWriteCondition);
                            }
                        }
                    }
                }
            }
        }

        public void GetRect(out float minX, out float minZ, out float maxX, out float maxZ)
        {
            const int Resolution = 10;
            minX = float.MaxValue;
            minZ = float.MaxValue;
            maxX = float.MinValue;
            maxZ = float.MinValue;
            var splines = _splineContainer.Splines;
            var splineCount = splines.Count;
            for (int i = 0; i < splineCount; i++)
            {
                var spline = splines[i];
                for (int j = 0; j < Resolution; j++)
                {
                    var t = (float)j / (Resolution - 1);
                    if (spline.Evaluate(t, out var position, out var tangent, out var up))
                    {
                        var worldPosition = transform.TransformPoint(position);
                        var worldTangent = transform.TransformDirection(tangent);
                        var worldUp = transform.TransformDirection(up);
                        var worldRight = Vector3.Cross(worldTangent, worldUp).normalized;
                        var rightPosition = worldPosition + worldRight * _width * 0.5f;
                        var leftPosition = worldPosition - worldRight * _width * 0.5f;
                        minX = Mathf.Min(minX, rightPosition.x, leftPosition.x);
                        minZ = Mathf.Min(minZ, rightPosition.z, leftPosition.z);
                        maxX = Mathf.Max(maxX, rightPosition.x, leftPosition.x);
                        maxZ = Mathf.Max(maxZ, rightPosition.z, leftPosition.z);
                    }
                }
            }
        }

        public float GetHeight(float worldX, float worldZ, float currentHeight)
        {
            if (_innerHeightMap == null)
            {
                return currentHeight;
            }

            var mapPosition = new Vector2(
                (worldX - _mapMin.x) / (_mapMax.x - _mapMin.x) * _mapResolution.x,
                (worldZ - _mapMin.y) / (_mapMax.y - _mapMin.y) * _mapResolution.y);
            var innerMapHeight = GetInnerHeight(mapPosition);
            if (innerMapHeight < 0)
            {
                return currentHeight;
            }

            return TerrainPartsUtility.MergeHeight(currentHeight, innerMapHeight, _writeCondition);
        }

        private float GetInnerHeight(Vector2 mapPosition)
        {
            var CheckPositions = new Vector2Int[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
            };

            var basePosition = new Vector2Int(Mathf.FloorToInt(mapPosition.x), Mathf.FloorToInt(mapPosition.y));
            var totalWeight = 0f;
            var totalHeight = 0f;
            for (int i = 0; i < CheckPositions.Length; i++)
            {
                var checkPosition = CheckPositions[i];
                var position = basePosition + checkPosition;
                if (position.x < 0 || position.x >= _mapResolution.x || position.y < 0 || position.y >= _mapResolution.y)
                {
                    continue;
                }

                var height = _innerHeightMap[position.y, position.x];
                if (height < 0)
                {
                    continue;
                }

                var d = Vector2.Distance(new Vector2(mapPosition.x % 1, mapPosition.y % 1), checkPosition);
                var weight = 1 - Mathf.Clamp01(d);
                totalWeight += weight;
                totalHeight += height * weight;
            }

            if (totalWeight == 0)
            {
                return -1;
            }

            return totalHeight / totalWeight;
        }
    }
}
#endif
