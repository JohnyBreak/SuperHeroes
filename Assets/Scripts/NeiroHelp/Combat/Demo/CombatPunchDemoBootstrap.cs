using UnityEngine;

namespace NeiroHelp.Combat
{
    /// <summary>
    /// Quick scene setup: player combat driver + dummy enemy in front.
    /// Add this to an empty GameObject and press Play, or use menu context on component.
    /// </summary>
    public class CombatPunchDemoBootstrap : MonoBehaviour
    {
        [SerializeField] private bool _spawnOnAwake = true;
        [SerializeField] private float _dummyDistance = 1.1f;
        [SerializeField] private LayerMask _hitMask = ~0;

        private void Awake()
        {
            if (_spawnOnAwake)
            {
                Build();
            }
        }

        [ContextMenu("Build Punch Demo")]
        public void Build()
        {
            CombatDriver existingDriver = FindObjectOfType<CombatDriver>();
            if (existingDriver != null)
            {
                EnsureDummy(existingDriver.transform);
                return;
            }

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "CombatDemoPlayer";
            player.transform.position = transform.position;
            Object.Destroy(player.GetComponent<CapsuleCollider>());

            player.AddComponent<CombatDriver>();
            EnsureDummy(player.transform);
            Debug.Log(
                "Combat punch demo ready. LMB tap = LightPunch1 (curves + swept hit + knockback). " +
                "LMB hold = Uppercut. RMB = Heavy. F = Special (aim with W/S).");
        }

        private void EnsureDummy(Transform player)
        {
            CombatDummy dummy = FindObjectOfType<CombatDummy>();
            if (dummy != null)
            {
                return;
            }

            GameObject dummyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dummyObject.name = "CombatDummy";
            dummyObject.transform.position = player.position + player.forward * _dummyDistance + Vector3.up * 0.5f;
            dummyObject.transform.localScale = new Vector3(0.8f, 1.6f, 0.8f);
            dummyObject.AddComponent<CombatDummy>();

            Rigidbody rigidbody = dummyObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }
    }
}
