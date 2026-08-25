using UnityEngine;

namespace Wolverine.Wounds
{
    /// <summary>
    /// Builds a two-layer capsule (meat inside, clipable skin outside) and wires Method C painting.
    /// Drop on an empty GameObject in a scene, enter Play Mode, then left-click the capsule.
    /// </summary>
    public sealed class WoundCapsuleDemo : MonoBehaviour
    {
        [Header("Layers")]
        [SerializeField] private float _innerScale = 0.92f;
        [SerializeField] private Color _skinColor = new Color(0.82f, 0.62f, 0.48f, 1f);
        [SerializeField] private Color _skinInteriorColor = new Color(0.45f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color _meatColor = new Color(0.55f, 0.12f, 0.12f, 1f);
        [SerializeField] private Color _boneColor = new Color(0.9f, 0.88f, 0.78f, 1f);

        [Header("Wound")]
        [SerializeField] private float _clipThreshold = 0.35f;
        [SerializeField] private float _stampRadius = 0.14f;
        [SerializeField] private float _regenPerSecond = 0.08f;
        [SerializeField] private bool _regenWhileHoldingR = true;

        [Header("Shaders (auto-loaded by name if empty)")]
        [SerializeField] private Shader _skinShader;
        [SerializeField] private Shader _skinInteriorShader;
        [SerializeField] private Shader _meatShader;
        [SerializeField] private Shader _stampShader;
        [SerializeField] private Shader _fadeShader;

        private WoundMaskBuffer _woundMaskBuffer;
        private WoundDecalPainter _woundDecalPainter;
        private Camera _camera;
        private Material _skinMaterial;
        private Material _skinInteriorMaterial;
        private Material _meatMaterial;
        private Material _stampMaterial;
        private bool _isBuilt;

        private void Start()
        {
            BuildIfNeeded();
        }

        [ContextMenu("Build Wound Capsule Demo")]
        public void BuildIfNeeded()
        {
            if (_isBuilt)
            {
                return;
            }

            ResolveShaders();
            ClearChildren();

            GameObject innerObject = CreateCapsuleChild("InnerMeat", _innerScale);
            GameObject outerObject = CreateCapsuleChild("OuterSkin", 1f);

            _meatMaterial = new Material(_meatShader)
            {
                name = "MeatMaterial"
            };
            _meatMaterial.SetColor("_Color", _meatColor);
            _meatMaterial.SetColor("_BoneColor", _boneColor);
            innerObject.GetComponent<MeshRenderer>().sharedMaterial = _meatMaterial;

            _skinMaterial = new Material(_skinShader)
            {
                name = "SkinMaterial"
            };
            _skinMaterial.SetColor("_Color", _skinColor);
            _skinMaterial.SetFloat("_ClipThreshold", _clipThreshold);
            outerObject.GetComponent<MeshRenderer>().sharedMaterial = _skinMaterial;

            _skinInteriorMaterial = new Material(_skinInteriorShader)
            {
                name = "SkinInteriorMaterial"
            };
            _skinInteriorMaterial.SetColor("_Color", _skinInteriorColor);
            _skinInteriorMaterial.SetFloat("_ClipThreshold", _clipThreshold);

            // Same mesh, separate material, Cull Front — opaque inside of the outer shell.
            GameObject outerInteriorObject = new GameObject("OuterSkinInterior");
            outerInteriorObject.transform.SetParent(outerObject.transform, false);
            MeshFilter outerInteriorFilter = outerInteriorObject.AddComponent<MeshFilter>();
            outerInteriorFilter.sharedMesh = outerObject.GetComponent<MeshFilter>().sharedMesh;
            MeshRenderer outerInteriorRenderer = outerInteriorObject.AddComponent<MeshRenderer>();
            outerInteriorRenderer.sharedMaterial = _skinInteriorMaterial;

            CapsuleCollider collider = outerObject.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;

            _woundMaskBuffer = gameObject.AddComponent<WoundMaskBuffer>();
            _woundMaskBuffer.Initialize(
                new[] { _skinMaterial, _skinInteriorMaterial },
                _fadeShader);

            _stampMaterial = new Material(_stampShader)
            {
                name = "WoundStampMaterial"
            };

            _woundDecalPainter = gameObject.AddComponent<WoundDecalPainter>();
            _woundDecalPainter.Configure(
                _woundMaskBuffer,
                outerObject.GetComponent<MeshFilter>(),
                outerObject.transform,
                _stampMaterial);
            _woundDecalPainter.SetStampShape(_stampRadius, 0.25f, 1f, 0.35f);

            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject cameraObject = new GameObject("WoundDemoCamera");
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.transform.position = new Vector3(0f, 1f, -3.5f);
                cameraObject.transform.LookAt(transform.position + Vector3.up);
            }

            if (Object.FindObjectOfType<Light>() == null)
            {
                GameObject lightObject = new GameObject("Directional Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            _isBuilt = true;
        }

        private void Update()
        {
            if (!_isBuilt || _camera == null)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    _woundDecalPainter.StampAtHit(hit);
                }
            }

            if (_regenWhileHoldingR && Input.GetKey(KeyCode.R))
            {
                _woundMaskBuffer.Fade(_regenPerSecond * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                _woundMaskBuffer.Clear();
            }
        }

        private void OnDestroy()
        {
            DestroyIfNotNull(_skinMaterial);
            DestroyIfNotNull(_skinInteriorMaterial);
            DestroyIfNotNull(_meatMaterial);
            DestroyIfNotNull(_stampMaterial);
        }

        private void ResolveShaders()
        {
            if (_skinShader == null)
            {
                _skinShader = Shader.Find("Wolverine/SkinClipMask");
            }

            if (_skinInteriorShader == null)
            {
                _skinInteriorShader = Shader.Find("Wolverine/SkinInteriorClip");
            }

            if (_meatShader == null)
            {
                _meatShader = Shader.Find("Wolverine/MeatSolid");
            }

            if (_stampShader == null)
            {
                _stampShader = Shader.Find("Wolverine/WoundDecalStamp");
            }

            if (_fadeShader == null)
            {
                _fadeShader = Shader.Find("Wolverine/WoundMaskFade");
            }

            if (_skinShader == null
                || _skinInteriorShader == null
                || _meatShader == null
                || _stampShader == null
                || _fadeShader == null)
            {
                Debug.LogError(
                    "Wolverine wound shaders not found. Copy the Shaders folder under Assets and let Unity import them.");
            }
        }

        private GameObject CreateCapsuleChild(string objectName, float scale)
        {
            GameObject child = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            child.name = objectName;
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.up;
            child.transform.localScale = Vector3.one * scale;

            Collider primitiveCollider = child.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Destroy(primitiveCollider);
            }

            return child;
        }

        private void ClearChildren()
        {
            for (int index = transform.childCount - 1; index >= 0; index--)
            {
                Destroy(transform.GetChild(index).gameObject);
            }

            WoundMaskBuffer existingBuffer = GetComponent<WoundMaskBuffer>();
            if (existingBuffer != null)
            {
                Destroy(existingBuffer);
            }

            WoundDecalPainter existingPainter = GetComponent<WoundDecalPainter>();
            if (existingPainter != null)
            {
                Destroy(existingPainter);
            }
        }

        private static void DestroyIfNotNull(Object resource)
        {
            if (resource != null)
            {
                Destroy(resource);
            }
        }
    }
}
