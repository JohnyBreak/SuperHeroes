using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private float _groundedGravity = -0.6f;
    [SerializeField] private float _airGravity = -9.8f;
    [SerializeField] private MyCharacterController m_CharacterController;
    

    void Update()
    {
        float gravity = (m_CharacterController.IsGrounded) ? _groundedGravity : _airGravity;

        var gravityDir = Vector3.down;

        gravityDir.y = gravity;

        m_CharacterController.Move(gravityDir);
    }
}
