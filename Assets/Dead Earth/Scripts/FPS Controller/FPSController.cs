using UnityEngine;
using System.Collections;

public enum PlayerMoveStatus { NotMoving, Walking, Running, NotGrounded, Landing }

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour {

    [SerializeField]
    private float _walkSpeed = 1;
    [SerializeField]
    private float _runSpeed = 4.5f;
    [SerializeField]
    private float _jumpSpeed = 7.5f;
    [SerializeField]
    private float _stickToGroundForce = 5;
    [SerializeField]
    private float _gravityMultiplyer = 2.5f;

    [SerializeField]
    private UnityStandardAssets.Characters.FirstPerson.MouseLook _mouseLook;

    private Camera _camera;
    private Vector2 _inputVector = Vector2.zero;
    private Vector3 _moveDirection = Vector3.zero;
    private CharacterController _characterController;
    private PlayerMoveStatus _movementStatus = PlayerMoveStatus.NotMoving;

    private bool _jumpButtonPressed;
    private bool _previouslyGrounded;
    private bool _isWalking = true;
    private bool _isJumping;
    private float _fallingTimer;

    public PlayerMoveStatus movementStatus { get => _movementStatus; }
    public float walkSpeed { get => _walkSpeed; }

    protected void Start() {
        _characterController = GetComponent<CharacterController>();
        _camera = Camera.main;
        _movementStatus = PlayerMoveStatus.NotMoving;
        _fallingTimer = 0;
        _mouseLook.Init(transform, _camera.transform);
    }

    protected void Update() {
        if (_characterController.isGrounded)
            _fallingTimer = 0;
        else
            _fallingTimer += Time.deltaTime;

        if (Time.timeScale > Mathf.Epsilon)
            _mouseLook.LookRotation(transform, _camera.transform);

        if (!_jumpButtonPressed)
            _jumpButtonPressed = Input.GetButtonDown("Jump");

        if (!_previouslyGrounded && _characterController.isGrounded) {
            if (_fallingTimer > 0.5f) {

            }

            _moveDirection.y = 0;
            _isJumping = false;
            _movementStatus = PlayerMoveStatus.Landing;
        } else if (!_characterController.isGrounded)
            _movementStatus = PlayerMoveStatus.NotGrounded;
        else if (_characterController.velocity.sqrMagnitude < 0.01f)
            _movementStatus = PlayerMoveStatus.NotMoving;
        else if (_isWalking)
            _movementStatus = PlayerMoveStatus.Walking;
        else
            _movementStatus = PlayerMoveStatus.Running;

        _previouslyGrounded = _characterController.isGrounded;
    }

    protected void FixedUpdate() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool wasWalking = _isWalking;
        _isWalking = !Input.GetKey(KeyCode.LeftShift);

        float speed = _isWalking ? _walkSpeed : _runSpeed;
        _inputVector = new Vector2(horizontal, vertical);

        if (_inputVector.sqrMagnitude > 1)
            _inputVector.Normalize();

        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right *_inputVector.x;

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height / 2, 1))
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        _moveDirection.x = desiredMove.x * speed;
        _moveDirection.z = desiredMove.z * speed;

        if (_characterController.isGrounded) {
            _moveDirection.y = -_stickToGroundForce;
            if (_jumpButtonPressed) {
                _moveDirection.y = _jumpSpeed;
                _jumpButtonPressed = false;
                _isJumping = true;
            }
        } else {
            _moveDirection += Physics.gravity * _gravityMultiplyer * Time.fixedDeltaTime;
        }

        _characterController.Move(_moveDirection * Time.fixedDeltaTime);
    }
}
