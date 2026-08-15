using UnityEngine;

public class CubeCityGenerator : MonoBehaviour
{
    private const string CityRootName = "City";
    private const string WallLayerName = "Wall";
    private const string GroundLayerName = "Ground";

    [Header("Grid")]
    [SerializeField] private int _blocksX = 8;
    [SerializeField] private int _blocksZ = 8;
    [SerializeField] private float _avenueWidth = 12f;
    [SerializeField] private float _streetWidth = 6f;
    [SerializeField] private float _alleyWidth = 2f;
    [SerializeField] private int _avenueEvery = 3;

    [Header("Buildings")]
    [SerializeField] private float _buildingWidthMin = 4f;
    [SerializeField] private float _buildingWidthMax = 10f;
    [SerializeField] private float _buildingDepthMin = 4f;
    [SerializeField] private float _buildingDepthMax = 10f;
    [SerializeField] private float _buildingHeightMin = 8f;
    [SerializeField] private float _buildingHeightMax = 60f;
    [SerializeField] private float _downtownFalloff = 2f;
    [SerializeField] private float _heightNoise = 0.15f;

    [Header("Look")]
    [SerializeField] private Material _buildingMaterial;
    [SerializeField] private Material _groundMaterial;
    [SerializeField] private float _groundThickness = 1f;

    [Header("Random")]
    [SerializeField] private int _seed = 1;

    private Transform _cityRoot;
    private System.Random _random;

    [ContextMenu("Generate City")]
    public void GenerateCity()
    {
        ClearCity();
        NormalizeParameters();
        _random = new System.Random(_seed);
        _cityRoot = new GameObject(CityRootName).transform;
        _cityRoot.SetParent(transform, false);
        BuildCity();
    }

    [ContextMenu("Clear City")]
    public void ClearCity()
    {
        for (int index = transform.childCount - 1; index >= 0; index--)
        {
            Transform child = transform.GetChild(index);
            if (child.name != CityRootName)
                continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        _cityRoot = null;
    }

    private void NormalizeParameters()
    {
        _blocksX = Mathf.Max(1, _blocksX);
        _blocksZ = Mathf.Max(1, _blocksZ);
        _avenueEvery = Mathf.Max(1, _avenueEvery);
        _avenueWidth = Mathf.Max(0.1f, _avenueWidth);
        _streetWidth = Mathf.Max(0.1f, _streetWidth);
        _alleyWidth = Mathf.Max(0f, _alleyWidth);
        _groundThickness = Mathf.Max(0.1f, _groundThickness);
        _downtownFalloff = Mathf.Max(1f, _downtownFalloff);
        _heightNoise = Mathf.Clamp01(_heightNoise);

        SwapIfNeeded(ref _buildingWidthMin, ref _buildingWidthMax);
        SwapIfNeeded(ref _buildingDepthMin, ref _buildingDepthMax);
        SwapIfNeeded(ref _buildingHeightMin, ref _buildingHeightMax);
    }

    private static void SwapIfNeeded(ref float minimum, ref float maximum)
    {
        if (minimum <= maximum)
            return;

        float temporary = minimum;
        minimum = maximum;
        maximum = temporary;
    }

    private void BuildCity()
    {
        ComputeBlockRects(out Rect[] blockRects, out float cityWidth, out float cityDepth);
        CreateGround(cityWidth, cityDepth);

        Vector2 cityCenter = new Vector2(cityWidth * 0.5f, cityDepth * 0.5f);
        float maxRadius = cityCenter.magnitude;

        for (int z = 0; z < _blocksZ; z++)
        {
            for (int x = 0; x < _blocksX; x++)
            {
                Rect blockRect = blockRects[x + z * _blocksX];
                PopulateBlock(x, z, blockRect, cityCenter, maxRadius);
            }
        }
    }

    private float GapWidth(int gapIndex)
    {
        bool isAvenue = (gapIndex + 1) % _avenueEvery == 0;
        return isAvenue ? _avenueWidth : _streetWidth;
    }

    private void ComputeBlockRects(out Rect[] blockRects, out float cityWidth, out float cityDepth)
    {
        blockRects = new Rect[_blocksX * _blocksZ];

        float[] blockWidths = new float[_blocksX];
        float[] blockDepths = new float[_blocksZ];

        for (int x = 0; x < _blocksX; x++)
            blockWidths[x] = EstimateBlockExtent(_buildingWidthMin, _buildingWidthMax);

        for (int z = 0; z < _blocksZ; z++)
            blockDepths[z] = EstimateBlockExtent(_buildingDepthMin, _buildingDepthMax);

        float[] originX = new float[_blocksX];
        float[] originZ = new float[_blocksZ];

        float cursorX = 0f;
        for (int x = 0; x < _blocksX; x++)
        {
            originX[x] = cursorX;
            cursorX += blockWidths[x];
            if (x < _blocksX - 1)
                cursorX += GapWidth(x);
        }

        float cursorZ = 0f;
        for (int z = 0; z < _blocksZ; z++)
        {
            originZ[z] = cursorZ;
            cursorZ += blockDepths[z];
            if (z < _blocksZ - 1)
                cursorZ += GapWidth(z);
        }

        cityWidth = cursorX;
        cityDepth = cursorZ;

        for (int z = 0; z < _blocksZ; z++)
        {
            for (int x = 0; x < _blocksX; x++)
            {
                int index = x + z * _blocksX;
                blockRects[index] = new Rect(originX[x], originZ[z], blockWidths[x], blockDepths[z]);
            }
        }
    }

    private float EstimateBlockExtent(float buildingMin, float buildingMax)
    {
        int buildingsPerSide = 2 + _random.Next(0, 3);
        float averageBuilding = (buildingMin + buildingMax) * 0.5f;
        float extent = buildingsPerSide * averageBuilding + Mathf.Max(0, buildingsPerSide - 1) * _alleyWidth;
        float jitter = (float)(_random.NextDouble() * 0.25 + 0.875);
        return Mathf.Max(buildingMin, extent * jitter);
    }

    private static int ResolveLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer >= 0)
            return layer;

        Debug.LogWarning($"CubeCityGenerator: layer '{layerName}' not found. Using Default.");
        return 0;
    }

    private void CreateGround(float cityWidth, float cityDepth)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.layer = ResolveLayer(GroundLayerName);
        ground.transform.SetParent(_cityRoot, false);
        ground.transform.localScale = new Vector3(cityWidth, _groundThickness, cityDepth);
        ground.transform.localPosition = new Vector3(cityWidth * 0.5f, -_groundThickness * 0.5f, cityDepth * 0.5f);

        if (_groundMaterial != null)
        {
            MeshRenderer meshRenderer = ground.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _groundMaterial;
        }
    }

    private void PopulateBlock(int blockX, int blockZ, Rect blockRect, Vector2 cityCenter, float maxRadius)
    {
        Transform blockRoot = new GameObject($"Block_{blockX}_{blockZ}").transform;
        blockRoot.SetParent(_cityRoot, false);
        blockRoot.localPosition = Vector3.zero;

        int columns = CountBuildingsAlong(blockRect.width, _buildingWidthMin, _buildingWidthMax);
        int rows = CountBuildingsAlong(blockRect.height, _buildingDepthMin, _buildingDepthMax);

        float totalAlleyX = Mathf.Max(0, columns - 1) * _alleyWidth;
        float totalAlleyZ = Mathf.Max(0, rows - 1) * _alleyWidth;
        float cellWidth = (blockRect.width - totalAlleyX) / columns;
        float cellDepth = (blockRect.height - totalAlleyZ) / rows;

        int buildingIndex = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                float width = RandomRange(_buildingWidthMin, Mathf.Min(_buildingWidthMax, cellWidth));
                float depth = RandomRange(_buildingDepthMin, Mathf.Min(_buildingDepthMax, cellDepth));

                float cellMinX = blockRect.x + column * (cellWidth + _alleyWidth);
                float cellMinZ = blockRect.y + row * (cellDepth + _alleyWidth);
                float centerX = cellMinX + cellWidth * 0.5f;
                float centerZ = cellMinZ + cellDepth * 0.5f;

                float height = SampleBuildingHeight(new Vector2(centerX, centerZ), cityCenter, maxRadius);
                CreateBuilding(blockRoot, buildingIndex++, centerX, centerZ, width, depth, height);
            }
        }
    }

    private int CountBuildingsAlong(float extent, float buildingMin, float buildingMax)
    {
        float average = (buildingMin + buildingMax) * 0.5f;
        int count = Mathf.FloorToInt((extent + _alleyWidth) / (average + _alleyWidth));
        return Mathf.Max(1, count);
    }

    private float SampleBuildingHeight(Vector2 position, Vector2 cityCenter, float maxRadius)
    {
        float distance = Vector2.Distance(position, cityCenter);
        float normalized = maxRadius <= 0.001f ? 0f : Mathf.Clamp01(distance / maxRadius);
        float t = 1f - Mathf.Pow(normalized, _downtownFalloff);
        float height = Mathf.Lerp(_buildingHeightMin, _buildingHeightMax, t);
        float noiseAmplitude = (_buildingHeightMax - _buildingHeightMin) * _heightNoise;
        height += RandomRange(-noiseAmplitude, noiseAmplitude);
        return Mathf.Clamp(height, _buildingHeightMin, _buildingHeightMax);
    }

    private void CreateBuilding(Transform parent, int index, float centerX, float centerZ, float width, float depth, float height)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = $"Building_{index}";
        building.layer = ResolveLayer(WallLayerName);
        building.transform.SetParent(parent, false);
        building.transform.localScale = new Vector3(width, height, depth);
        building.transform.localPosition = new Vector3(centerX, height * 0.5f, centerZ);

        if (_buildingMaterial != null)
        {
            MeshRenderer meshRenderer = building.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _buildingMaterial;
        }
    }

    private float RandomRange(float minimum, float maximum)
    {
        return minimum + (float)_random.NextDouble() * (maximum - minimum);
    }
}
