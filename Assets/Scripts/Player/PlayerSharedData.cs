using UnityEngine;

public class PlayerSharedData
{
    public float PreviousYVelocity;
    public  float MaxFallGravity { get; private set; } = -35f;

    private float _maxJumpHeight = 3.5f;
    private float _maxJumpTime = 1f;
    public float Gravity { get; private set; } = -20;
    public float JumpGravity { get; private set; } = -9.8f;
    public float InitialJumpVelocity { get; private set; }

    public void Init()
    {
        SetupVariables();
    }
    
    private void SetupVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        JumpGravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        InitialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
    }
}
