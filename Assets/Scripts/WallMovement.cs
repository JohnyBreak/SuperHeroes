using UnityEngine;

public class WallMovement : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private int _crouchtHash = Animator.StringToHash("IsCrouching");

    private void OnEnable()
    {
        _animator.SetBool(_crouchtHash, true);
    }

    private void OnDisable()
    {
        _animator.SetBool(_crouchtHash, false);
    }

    void Update()
    {
        
    }
}
