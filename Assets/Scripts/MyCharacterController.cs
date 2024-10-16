using UnityEngine;

public class MyCharacterController : MonoBehaviour
{
    [SerializeField] private CapsuleCollider _playerCollider;
    [SerializeField] private LayerMask _discludePlayer;
    [SerializeField] private float _playerHeight;

    private Vector3 _velocity;
    private Vector3 _move;
    private bool _isGrounded;

    public bool IsGrounded { get => _isGrounded; set => _isGrounded = value; }

    public LayerMask DiscludePlayerMask => _discludePlayer;
    public Vector3 Velocity { get => _velocity; set => _velocity = value; }
    public float PlayerHeight { get => _playerHeight; }
    public Vector3 MoveDir { get => _move; set => _move = value; }
    public bool ShouldSnapToGround;

    //ground check
    
    [SerializeField] private bool _smooth;
    [SerializeField] private float _smoothSpeed;
    [SerializeField] private float _checkSphereRadius = 0.2f;
    [SerializeField] private float _confirmCollisionRadius = 0.4f;
    [SerializeField] private Vector3 _groundCheckPosition;
    private RaycastHit _groundHit;

    private void Update()
    {
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
        transform.position += _velocity * Time.deltaTime;

        _velocity = Vector3.zero;
    }

    private void CheckCollision()
    {
        Collider[] overlap = new Collider[4];

        Vector3 p0 = new Vector3(_playerCollider.center.x, (_playerCollider.center.y + (_playerCollider.height / 2)) - _playerCollider.radius, _playerCollider.center.z);
        Vector3 p1 = new Vector3(_playerCollider.center.x, (_playerCollider.center.y - (_playerCollider.height / 2)) + _playerCollider.radius, _playerCollider.center.z);
        int num = Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(p0), transform.TransformPoint(p1), _playerCollider.radius, overlap, _discludePlayer, QueryTriggerInteraction.UseGlobal);

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
                //dir.y = 0;
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
        Ray ray = new Ray(transform.position + transform.up, -transform.up);

        if (Physics.SphereCast(ray, _checkSphereRadius, out RaycastHit tempHit, 6, _discludePlayer))
        {
            ConfirmGround(tempHit);
        }
        else
        {
            IsGrounded = false;
        }
    }

    private void ConfirmGround(RaycastHit tempHit)
    {
        Collider[] collider = new Collider[3];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(_groundCheckPosition), _confirmCollisionRadius, collider, _discludePlayer);

        _isGrounded = false;
        for (int i = 0; i < num; i++)
        {
            if (collider[i].transform == tempHit.transform)
            {
                _groundHit = tempHit;
                if (ShouldSnapToGround)
                {
                    var newPos = new Vector3(transform.position.x, (_groundHit.point.y/* + _playerHeight / 2*/), transform.position.z);

                    if (!_smooth)
                    {
                        transform.position = newPos;
                    }
                    else
                    {
                        transform.position = Vector3.Lerp(transform.position, newPos, _smoothSpeed * Time.deltaTime);
                    }
                }
                _isGrounded = true;
                break;
            }
        }
    }


    #endregion

    [SerializeField] private bool _drawGizmos;

    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos) return;

        Gizmos.color = new Color(1, 1, 0, 0.75F);
        Gizmos.DrawWireSphere(transform.TransformPoint(_groundCheckPosition), _confirmCollisionRadius);

        Ray ray = new Ray(transform.position, -transform.up);

        if (Physics.SphereCast(ray, _checkSphereRadius, out RaycastHit tempHit, 6, _discludePlayer))
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere((tempHit.point + Vector3.up * _checkSphereRadius), _checkSphereRadius);
        }
    }
}
