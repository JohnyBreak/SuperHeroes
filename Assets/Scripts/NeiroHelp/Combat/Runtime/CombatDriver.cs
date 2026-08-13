using UnityEngine;

namespace NeiroHelp.Combat
{
    /// <summary>
    /// Self-contained combat runner (Combat root analogue).
    /// Owns attack playback, motion, hit probe and combo resolve while attacking.
    /// </summary>
    public class CombatDriver : MonoBehaviour
    {
        [SerializeField] private Transform _modelFacing;
        [SerializeField] private Transform _hitSocket;
        [SerializeField] private LayerMask _hitMask = ~0;
        [SerializeField] private bool _useExampleGraph = true;
        [SerializeField] private ComboGraphDefinition _comboGraph;
        [SerializeField] private bool _isGrounded = true;
        [SerializeField] private KeyCode _lightKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode _heavyKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode _specialKey = KeyCode.F;

        private CombatInputSampler _inputSampler;
        private CombatTargeting _targeting;
        private CombatInputResolver _resolver;
        private AttackExecutor _executor;
        private CombatEffectRunner _effectRunner;
        private bool _inCombat;

        public bool InCombat => _inCombat;
        public AttackExecutor Executor => _executor;
        public ComboGraphDefinition Graph => _comboGraph;

        private void Awake()
        {
            if (_modelFacing == null)
            {
                _modelFacing = transform;
            }

            if (_hitSocket == null)
            {
                _hitSocket = CreateDefaultHitSocket();
            }

            if (_useExampleGraph || _comboGraph == null)
            {
                _comboGraph = CombatExampleFactory.CreateExampleGraph(_hitMask);
            }

            _inputSampler = new CombatInputSampler(_lightKey, _heavyKey, _specialKey);
            _targeting = new CombatTargeting(_modelFacing);
            _resolver = new CombatInputResolver(_comboGraph, _inputSampler, _targeting);
            _executor = new AttackExecutor(_modelFacing, _hitSocket);
            _effectRunner = new CombatEffectRunner(_modelFacing, _targeting);
            _executor.EffectFired += OnEffectFired;
        }

        private void OnDestroy()
        {
            if (_executor != null)
            {
                _executor.EffectFired -= OnEffectFired;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            _resolver.Tick(deltaTime, moveInput);

            AttackContext context = _isGrounded ? AttackContext.Ground : AttackContext.Air;

            if (!_inCombat)
            {
                if (_resolver.TryResolveStarter(context, out AttackDefinition starter))
                {
                    EnterCombat(starter);
                }

                return;
            }

            _executor.Tick(deltaTime);
            ApplyMotion(_executor.LastMotionDelta);

            if (_executor.IsPlaying && _executor.IsComboWindowOpen())
            {
                if (_resolver.TryResolveNext(
                        _executor.CurrentAttack.Id,
                        context,
                        _executor.HadHitConfirm,
                        out AttackDefinition next))
                {
                    _executor.Start(next);
                }
            }

            if (!_executor.IsPlaying)
            {
                ExitCombat();
            }
        }

        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
        }

        private void EnterCombat(AttackDefinition attack)
        {
            _inCombat = true;
            _executor.Start(attack);
            Debug.Log($"[Combat] Start {attack.Id}");
        }

        private void ExitCombat()
        {
            _inCombat = false;
            _resolver.ClearBuffer();
            Debug.Log("[Combat] Exit");
        }

        private void ApplyMotion(Vector3 worldDelta)
        {
            if (worldDelta.sqrMagnitude <= 0f)
            {
                return;
            }

            transform.position += worldDelta;
        }

        private void OnEffectFired(AttackEffectEntry entry, AttackDefinition attack)
        {
            _effectRunner.Run(entry, attack);
        }

        private Transform CreateDefaultHitSocket()
        {
            GameObject socketObject = new GameObject("HitSocket_RightHand");
            socketObject.transform.SetParent(_modelFacing, false);
            socketObject.transform.localPosition = new Vector3(0.25f, 1.2f, 0.45f);
            return socketObject.transform;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_hitSocket == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(_hitSocket.position, Vector3.one * 0.3f);
        }
#endif
    }
}
