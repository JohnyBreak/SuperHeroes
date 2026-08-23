using Synty.AnimationBaseLocomotion.Samples.InputSystem;
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
    
    public readonly int MovementHash = Animator.StringToHash("Movement");
    public readonly int WallCrawlingHash = Animator.StringToHash("WallCrawling");
    public readonly int TPoseHash = Animator.StringToHash("TPose");
    public readonly int JumpHash = Animator.StringToHash("Jump");
    public readonly int FallHash = Animator.StringToHash("Fall");
    public readonly int MovementSpeedHash = Animator.StringToHash("MovementSpeed");
    
    //public readonly int MovementHash = Animator.StringToHash("MovementSpeed");
    //public readonly int CrouchHash = Animator.StringToHash("IsCrouching");
    //public readonly int YVelocityHash = Animator.StringToHash("YVelocity");
    //public readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    //public readonly int TPoseHash = Animator.StringToHash("TPose");
    //public readonly int JumpHash = Animator.StringToHash("Jump");
    //public readonly int IsFallingHash = Animator.StringToHash("IsFalling");
    
    public readonly MyCharacterController Controller;
    public readonly PlayerPivot Pivot;
    public readonly UnitVelocity Velocity;
    public readonly InputReader InputReader;
    public readonly Animator Animator;
    public readonly Transform ModelT;
    public readonly Transform PlayerT;
    public readonly Transform CameraTransform;
    public readonly int WallMask;
    public readonly int GroundMask;
    public readonly LineRenderer LineRenderer;
    public readonly Transform SwingRoot;

    public PlayerSharedData(UnitVelocity unitVelocity,
        MyCharacterController controller,
        PlayerPivot pivot,
        Transform player,
        Transform model,
        InputReader inputReader,
        Animator animator,
        Transform cameraTransform,
        int wallMask,
        int groundMask,
        LineRenderer lineRenderer, 
        Transform swingRoot)
    {
        Velocity = unitVelocity;
        Controller = controller;
        Pivot = pivot;
        PlayerT = player;
        ModelT = model;
        CameraTransform = cameraTransform;
        InputReader = inputReader;
        Animator = animator;
        WallMask = wallMask;
        GroundMask = groundMask;
        LineRenderer = lineRenderer;
        SwingRoot = swingRoot;

        SetupVariables();
    }
    
    private void SetupVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        JumpGravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        InitialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
    }
}
