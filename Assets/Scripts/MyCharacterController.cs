using UnityEngine;

public class MyCharacterController : MonoBehaviour
{
    [SerializeField] private CapsuleCollider _playerCollider;
    [SerializeField] private LayerMask _discludePlayer;
    [SerializeField] private float _playerHeight;

    private Vector3 _velocity;
    private Vector3 _move;
    private Vector3 _vel;
    private Vector3 direction;
    private bool _isGrounded;
    private bool _isFalling;

    public bool IsGrounded { get => _isGrounded; set => _isGrounded = value; }
    public bool IsFalling { get => _isFalling; set => _isFalling = value; }
    public LayerMask DiscludePlayerMask => _discludePlayer;
    public Vector3 Velocity { get => _velocity; set => _velocity = value; }
    public float PlayerHeight { get => _playerHeight; }
    public Vector3 MoveDir { get => _move; set => _move = value; }
    public bool ShouldSnapToGround;
    public bool GroundDiff;

    //ground check
    [SerializeField] private bool _smooth;
    [SerializeField] private float _smoothSpeed;
    [SerializeField] private float _checkSphereRadius = .23f;
    [SerializeField] private float _checkCollisionRadius = .4f;
    private RaycastHit _groundHit;
    private Vector3 _liftPoint;
    private Vector3 _groundCheckPoint;
    private float _groundHeightDifference;
    private float _currentSphereCastDistance = 0;


    private void Awake()
    {
        _groundCheckPoint = new Vector3(0, -.87f/*.12f*/, 0);
        _liftPoint = new Vector3(0, 1.2f, 0);
    }

    private void Update()
    {
        //if (GameStateManager.CurrentGameState != GameStateManager.GameState.GamePlay) return;
        CheckCollision();
        CheckGround();
        FinalMove();
        CheckCollision();
    }


    #region Controller

    public void Move(Vector3 moveVector)
    {
        _move = moveVector;

        _velocity += _move;
    }

    private void FinalMove()
    {
        _vel = new Vector3(_velocity.x, _velocity.y, _velocity.z);
        transform.position += _velocity * Time.deltaTime;

        _velocity = Vector3.zero;
    }

    private void CheckCollision()
    {
        Collider[] overlap = new Collider[4];

        Vector3 p0 = new Vector3(_playerCollider.center.x, (_playerCollider.center.y + (_playerCollider.height / 2)) - _playerCollider.radius, _playerCollider.center.z);
        Vector3 p1 = new Vector3(_playerCollider.center.x, (_playerCollider.center.y - (_playerCollider.height / 2)) + _playerCollider.radius, _playerCollider.center.z);
        int num = Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(p0), transform.TransformPoint(p1), _playerCollider.radius, overlap, _discludePlayer, QueryTriggerInteraction.UseGlobal);

        //int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(_playerCollider.center), _playerCollider.radius, overlap, _discludePlayer, QueryTriggerInteraction.UseGlobal);
        for (int i = 0; i < num; i++)
        {
            Transform t = overlap[i].transform;
            Vector3 dir;
            float distance;

            if (Physics.ComputePenetration(_playerCollider, transform.position, transform.rotation, overlap[i], t.position, t.rotation, out dir, out distance))
            {
                /*if (dir.y != 0)
                {
                    distance *= .5f;
                    dir.y = 0;
                }*/
                dir.y = 0;
                Vector3 penetrationVector = dir * distance;

                transform.position = transform.position + penetrationVector;
                //Debug.DrawRay(transform.position + Vector3.up, penetrationVector);
            }
        }
    }

    #endregion

    #region GroundCheck

    private void CheckGround()
    {
        Ray ray = new Ray(transform.TransformPoint(_liftPoint), -transform.up);
        RaycastHit tempHit = new RaycastHit();
        //if (Physics.BoxCast(transform.position, new Vector3(_checkSphereRadius, _checkSphereRadius, _checkSphereRadius), -transform.up, out tempHit, Quaternion.identity, 6, _discludePlayer))
        if (Physics.SphereCast(ray, _checkSphereRadius, out tempHit, 6, _discludePlayer))
        {
            _currentSphereCastDistance = tempHit.distance;
            ConfirmGround(tempHit);
        }
        else
        {
            IsGrounded = false;
        }
    }

    private void ConfirmGround(RaycastHit tempHit)
    {
        RaycastHit raycastHit;
        Physics.Raycast(transform.position, -transform.up, out raycastHit, 2f, _discludePlayer);
        _groundHeightDifference = raycastHit.distance;
        GroundDiff = (_groundHeightDifference > 1.56 || _groundHeightDifference == 0);

        Collider[] collider = new Collider[3];
        //int num = Physics.OverlapBoxNonAlloc(transform.TransformPoint(_groundCheckPoint), new Vector3(_checkSphereRadius + 0.2f, _checkSphereRadius + 0.2f, _checkSphereRadius + 0.2f)/*0.57f*/, collider, Quaternion.identity,_discludePlayer);
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(_groundCheckPoint), _checkCollisionRadius/*0.57f*/, collider, _discludePlayer);
        _isGrounded = false;
        for (int i = 0; i < num; i++)
        {
            if (collider[i].transform == tempHit.transform)
            {
                _groundHit = tempHit;
                if (ShouldSnapToGround)
                {
                    if (!_smooth)
                    {
                        transform.position = new Vector3(transform.position.x, ((_groundHit.point.y + _playerHeight / 2)/* + (_checkSphereRadius / 2)*/ /*y3*/), transform.position.z);
                    }
                    else
                    {
                        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, ((_groundHit.point.y + _playerHeight / 2)/* + (_checkSphereRadius / 2)*/ /*y3*/), transform.position.z), _smoothSpeed * Time.deltaTime);
                    }
                }
                _isGrounded = true;
                break;
            }
        }
    }


    #endregion

    [SerializeField] private bool _drawGizmos;

    //private void OnDrawGizmosSelected()
    //{
    //    if (!_drawGizmos) return;

    //    Gizmos.color = new Color(1, 1, 0, 0.75F);
    //    Gizmos.DrawWireSphere(transform.TransformPoint(_groundCheckPoint), _checkCollisionRadius);
    //    //Gizmos.DrawWireCube(transform.TransformPoint(_groundCheckPoint), new Vector3(_checkSphereRadius + 0.2f, _checkSphereRadius + 0.2f, _checkSphereRadius + 0.2f));
    //    Gizmos.color = Color.black;
    //    Gizmos.DrawWireSphere((transform.position + _liftPoint) + ((-transform.up) * _currentSphereCastDistance), _checkSphereRadius);
    //    // Gizmos.DrawWireCube(transform.TransformPoint(_groundCheckPoint), new Vector3(_checkSphereRadius, _checkSphereRadius, _checkSphereRadius));
    //    Debug.DrawRay(transform.position, -transform.up * 2f, Color.red);

    //    Gizmos.color = Color.blue;
    //    Vector3 p0 = new Vector3(_playerCollider.center.x, (_playerCollider.center.y + (_playerCollider.height / 2)) - _playerCollider.radius, _playerCollider.center.z);

    //    Vector3 p1 = new Vector3(_playerCollider.center.x, (_playerCollider.center.y - (_playerCollider.height / 2)) + _playerCollider.radius, _playerCollider.center.z);
    //    Gizmos.DrawWireSphere(transform.TransformPoint(p0), _playerCollider.radius);
    //    Gizmos.DrawWireSphere(transform.TransformPoint(p1), _playerCollider.radius);

    //}
}
