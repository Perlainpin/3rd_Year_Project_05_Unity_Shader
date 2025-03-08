using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] InputActionReference _movement;
    [SerializeField] InputActionReference _sprintAction;
    [SerializeField] Rigidbody _rb;
    [SerializeField] float _speed;
    [SerializeField] float _maxSpeed = 5f;
    [SerializeField] float _sprintMultiplier = 1.5f;
    private bool isSprinting => _sprintAction.action.IsPressed();

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 120f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] InputActionReference _jumpAction;
    private bool isGrounded;

    [Header("Animator")]
    [SerializeField] Animator _animator;
    
    
    private Vector3 _lastValidAimPos;

    private Vector2 currentMovementInput;
    private Vector3 currentCameraForward;
    private Vector3 currentCameraRight;

    private void Start()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        
        _lastValidAimPos = transform.position + transform.forward;
    }

    void OnEnable()
    {
        _movement.action.Enable();
        _jumpAction.action.Enable();
        _sprintAction.action.Enable();
    }

    void OnDisable()
    {
        _movement.action.Disable();
        _jumpAction.action.Disable();
        _sprintAction.action.Disable();
    }
   
    void Update()
    {
        //---------------CAMERA---------------//
        Vector3 aimPos = GetAimPos();
        aimPos.y = transform.position.y;
        transform.LookAt(aimPos);

        // Mouvement relatif   la cam ra
        currentMovementInput = _movement.action.ReadValue<Vector2>();
        currentCameraForward = Camera.main.transform.forward;
        currentCameraRight = Camera.main.transform.right;

        // Projeter les vecteurs sur le plan horizontal
        currentCameraForward.y = 0;
        currentCameraRight.y = 0;
        currentCameraForward.Normalize();
        currentCameraRight.Normalize();

        //---------------JUMP---------------//
        CheckGrounded();
        if (isGrounded && _jumpAction.action.WasPressedThisFrame())
        {
            Jump();
        }
        _animator.SetBool("isJumping", !isGrounded);

        //---------------SHADER---------------//
        Shader.SetGlobalVector("_PlayerPosition", transform.position);
    }

    void FixedUpdate()
    {
        //---------------MOVEMENTS---------------//
        
        Vector3 moveDirection = (currentCameraRight * currentMovementInput.x + currentCameraForward * currentMovementInput.y).normalized;
        float currentSpeed = isSprinting ? _speed * _sprintMultiplier : _speed;
        _rb.AddForce(moveDirection * currentSpeed);

        // Limiter la vitesse
        Vector3 horizontalVelocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        float speedLimit = isSprinting ? _maxSpeed * _sprintMultiplier : _maxSpeed;

        if (horizontalVelocity.magnitude > speedLimit)
        {
            horizontalVelocity = horizontalVelocity.normalized * speedLimit;
            _rb.velocity = new Vector3(horizontalVelocity.x, _rb.velocity.y, horizontalVelocity.z);
        }

        _animator.SetBool("isWalking", currentMovementInput.magnitude > 0.1f);
        _animator.SetBool("isSprinting", isSprinting && currentMovementInput.magnitude > 0.1f);

    }

    private Vector3 GetAimPos()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            _lastValidAimPos = hit.point;
        }
        return _lastValidAimPos;
    }

    private void CheckGrounded()
    {
        RaycastHit hit;
        // Effectuer un raycast vers le bas   partir de la position du joueur
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        // Met   jour le param tre 'isGrounded' dans l'Animator
        _animator.SetBool("isJumping", isGrounded);

        // Ray de debug pour visualiser le ground check
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    private void Jump()
    {
        // R initialiser la v locit  verticale avant de sauter
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z); // Enlever toute v locit  verticale
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Appliquer une impulsion de saut
        _animator.SetBool("isJumping", true); // D clencher l'animation de saut
    }
}