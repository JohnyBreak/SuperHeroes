using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private float _groundedGravity = -0.6f;
    [SerializeField] private float _airGravity = -9.8f;

    [SerializeField] private float _gravityClamp = -20;

    [SerializeField] private MyCharacterController m_CharacterController;
    private float _totalGravity = 0;

    void Update()
    {
        if (m_CharacterController.IsGrounded == false)
        {
            if(_totalGravity > _airGravity)
            {
                _totalGravity = _airGravity;
            }
            _totalGravity += _airGravity * Time.deltaTime;
        }
        else 
        {
            _totalGravity = _groundedGravity;
        }

        if (_totalGravity < _gravityClamp) 
        {
            _totalGravity = _gravityClamp;
        }

        var gravityDir = Vector3.down;

        gravityDir.y = _totalGravity;

        m_CharacterController.Move(gravityDir);
    }
}
