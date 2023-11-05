using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField] public float Speed { get; set; } = 5f;
    [field: SerializeField] public float JumpForce { get; set; } = 5f;
    [field: SerializeField] public float SubsequentJumpModifier { get; set; } = 1.2f;
    [field: SerializeField] public int AllowedJumps { get; set; } = 1;
    [field: SerializeField] public LayerMask GroundLayer { get; set; }
    [field: SerializeField] public InputActionReference MovementAction { get; set; }
    [field: SerializeField] public InputActionReference JumpAction { get; set; }
    [field: SerializeField] public InputActionReference AttackAction { get; set; }
    
    [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }

    private Vector2 movementInput;
    private int currentJumps = 0;
    private bool isGrounded;
    private bool isMoving;
    private bool isJumping;
    private bool isAttacking;
    private bool jumpButtonHeld;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        MovementAction.action.Enable();
        MovementAction.action.performed += OnMoveAction;
        MovementAction.action.canceled += OnMoveAction;
        JumpAction.action.Enable();
        JumpAction.action.performed += OnJumpAction;
        JumpAction.action.canceled += OnJumpAction;
        AttackAction.action.Enable();
        AttackAction.action.performed += OnAttackAction;
    }

    private void OnDisable()
    {
        MovementAction.action.Disable();
        MovementAction.action.performed -= OnMoveAction;
        MovementAction.action.canceled -= OnMoveAction;
        JumpAction.action.Disable();
        JumpAction.action.performed -= OnJumpAction;
        JumpAction.action.canceled -= OnJumpAction;
        AttackAction.action.Disable();
        AttackAction.action.performed -= OnAttackAction;
    }

    private void Update()
    {
        isGrounded = CheckIfGrounded();
        if (isGrounded && currentJumps > 0)
        {
            currentJumps = 0; // Reset jumps when grounded
        }
        
        if (isMoving)
        {
            MovePlayer();
        }
    }

    private void FixedUpdate()
    {
        if (isJumping)
        {
            ApplyJumpPhysics();
            isJumping = false; // Reset jump flag after applying physics
        }
    }

    private void MovePlayer()
    {
        transform.position += new Vector3(movementInput.x, 0, 0) * (Speed * Time.deltaTime);
        SpriteRenderer.flipX = movementInput.x < 0;
    }

    private void ApplyJumpPhysics()
    {
        if (Rigidbody.velocity.y > 0 && !jumpButtonHeld)
        {
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, Rigidbody.velocity.y * 0.5f);
        }
    }

    private bool CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, GroundLayer);
        return hit.collider != null;
    }

    private void OnMoveAction(InputAction.CallbackContext input)
    {
        movementInput.x = input.ReadValue<Vector2>().x;
        isMoving = movementInput.x != 0;
        Animator.SetBool("isMoving", isMoving);
    }

    private void OnJumpAction(InputAction.CallbackContext input)
    {
        if (input.phase == InputActionPhase.Performed)
        {
            jumpButtonHeld = true;
            // Allow a jump if on the ground, or if the number of jumps made is less than the allowed number
            if (isGrounded || currentJumps < AllowedJumps)
            {
                PerformJump();
            }
        }
        else if (input.phase == InputActionPhase.Canceled)
        {
            jumpButtonHeld = false;
        }
    }

    private void OnAttackAction(InputAction.CallbackContext context)
    {
        if (isAttacking || context.phase != InputActionPhase.Performed) return;
        
        Animator.SetTrigger("Attack");
        StartCoroutine(Attack());
    }

    private void PerformJump()
    {
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, 0);
        Animator.SetTrigger("Jump");
        float jumpModifier = (currentJumps == 0) ? 1f : SubsequentJumpModifier;
        Rigidbody.AddForce(Vector2.up * JumpForce * jumpModifier, ForceMode2D.Impulse);
        currentJumps++;
        isJumping = true;
    }

    private IEnumerator Attack()
    {
        isMoving = false;
        isAttacking = true;
        // Wait for the attack animation to complete
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false;
        isMoving = true;
    }
}
