using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Ground Check")]
    [SerializeField] private float gravity = -40f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;

    private CharacterController characterController;
    private Vector2 movementInput;
    private Vector3 velocity;
    private bool isSprinting;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        // Auto-find main camera if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMove;
            moveAction.action.canceled += OnMove;
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJump;
        }

        if (sprintAction != null)
        {
            sprintAction.action.Enable();
            sprintAction.action.performed += OnSprint;
            sprintAction.action.canceled += OnSprint;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= OnMove;
            moveAction.action.canceled -= OnMove;
            moveAction.action.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
            jumpAction.action.Disable();
        }

        if (sprintAction != null)
        {
            sprintAction.action.performed -= OnSprint;
            sprintAction.action.canceled -= OnSprint;
            sprintAction.action.Disable();
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (characterController.isGrounded)
        {
            // Calculate initial upward velocity using kinematic equation: v = √(2 * a * s)
            // This ensures the character reaches exactly 'jumpHeight' meters regardless of gravity strength
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleRotation();
    }

    private void HandleMovement()
    {
        // Get camera-relative movement direction
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Flatten camera directions (no vertical component)
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to camera
        Vector3 moveDirection = (cameraForward * movementInput.y + cameraRight * movementInput.x).normalized;

        // Apply speed
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        Vector3 move = moveDirection * currentSpeed;

        // Combine horizontal movement with vertical velocity
        Vector3 finalMovement = move + velocity;

        // Move the character
        characterController.Move(finalMovement * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -5f; // Stronger downward force to keep grounded
        }
        else
        {
            velocity.y += gravity * Time.deltaTime; // Apply gravity
        }
    }

    private void HandleRotation()
    {
        // Only rotate if moving
        if (movementInput.sqrMagnitude > 0.01f)
        {
            // Get movement direction relative to camera
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * movementInput.y + cameraRight * movementInput.x).normalized;

            // Smoothly rotate towards movement direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}