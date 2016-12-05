using System.Collections;
using UnityEngine;
using System.Linq;

/// <summary>
/// Receives & handles input information and sends them to RayController.
/// <para>Used to move character and update sprites.</para>
/// </summary>
[RequireComponent(typeof(RayController2D))]
public class RayMovement : MonoBehaviour
{
    [Tooltip("Is movement allowed.")]
    public bool allowMovement = true;
    [Tooltip("Normal walking speed.")]
    public float movementSpeed = 6;
    [Tooltip("When running, multiplies the movement speed with this number.")]
    public float runMultiplier = 2;

    [Space]

    public bool allowJumping = false;
    [Tooltip("Jump height in units.")]
    public float jumpHeight = 4;
    [Tooltip("Time it takes to reach jump height after pressing jump button.")]
    public float timeToJumpApex = .4f;

    [Space]

    public bool disableGravity = false;

    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = 0; // 0 for instant turning/stopping, .1f for smooth turning/stopping
    
    float gravity;
    float jumpVelocity;
    float velocityXSmoothing;
    public Vector3 velocity;

    float verticalSlowMoveSpeed = 1.5f;
    float verticalFastMoveSpeed = 3.5f;

    [HideInInspector] public Vector2 CharacterInput;
    [HideInInspector] public bool Jump;
    [HideInInspector] public bool Run;

    [HideInInspector]
    public float facingDirection = 1; // 1 = right, -1 = left (used by CharacterInteraction)

    RayController2D controller;
    Animator animator;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        controller = GetComponent<RayController2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void Update()
    {
        if (!allowMovement)
        {
            CharacterInput = Vector2.zero;
            Run = false;
        }

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        if (Jump && controller.collisions.below && allowJumping)
        {
            velocity.y = jumpVelocity;
        }

        float targetVelocityX = CharacterInput.x * movementSpeed * (Run ? runMultiplier : 1);

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        if (!disableGravity) velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        if ((velocity.x < -0.1 || velocity.x > 0.1) && allowMovement)
        {
            facingDirection = Mathf.Sign(velocity.x);
            spriteRenderer.flipX = (facingDirection > 0) ? false : true;
        }

        animator.SetBool("Run", Run);
        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
    }

    /// <summary>
    /// Turns the character to face a target character.
    /// </summary>
    /// <param name="target">Transform of the target.</param>
    public void FaceTarget(Transform target)
    {
        spriteRenderer.flipX = !target.GetComponent<SpriteRenderer>().flipX;
        facingDirection = (!spriteRenderer.flipX) ? 1 : -1;
    }

    /// <summary>
    /// Turns the character to face a target character.
    /// <para>Requires sprite renderer on the target.</para>
    /// </summary>
    /// <param name="target">SpriteRenderer of the target.</param>
    public void FaceTarget(SpriteRenderer target)
    {
        spriteRenderer.flipX = !target.flipX;
        facingDirection = (!target.flipX) ? 1 : -1;
    }

    /// <summary>
    /// Turns the character to face a target character.
    /// <para>Requires 2D collider on the target.</para>
    /// </summary>
    /// <param name="target">Target's collider.</param>
    public void FaceTarget(Collider2D target)
    {
        spriteRenderer.flipX = target.bounds.center.x < transform.position.x;
        facingDirection = spriteRenderer.flipX ? 1 : -1;
    }
}