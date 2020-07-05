using UnityEngine;
using System.Collections.Generic;
using System;

public enum PlayerMoveStatus { NotMoving, Walking, Running, NotGrounded, Landing }
public enum CurveControlledBobCallbackType { Horizontal, Vertical }

public delegate void CurveControlledBobCallback();

[System.Serializable]
public class CurveControlledBobEvent {
    public float Time = 0;
    public CurveControlledBobCallback Function;
    public CurveControlledBobCallbackType Type = CurveControlledBobCallbackType.Vertical;
}

[System.Serializable]
public class CurveControlledBob {
    [SerializeField]
    AnimationCurve _bobcurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f), new Keyframe(1.5f, -1),
        new Keyframe(2f, 0f));

    [SerializeField]
    private float _horizontalMultiplier = 0.01f;
    [SerializeField]
    private float _verticalMultiplier = 0.02f;
    [SerializeField]
    private float _verticalToHorizontalSpeedRatio = 2.0f;
    [SerializeField]
    private float _baseInterval;

    private float _prevXPlayHead;
    private float _prevYPlayHead;
    private float _xPlayHead;
    private float _yPlayHead;
    private float _curveEndTime;
    private List<CurveControlledBobEvent> _events = new List<CurveControlledBobEvent>();

    public void Initialize() {
        _curveEndTime = _bobcurve[_bobcurve.length - 1].time;
        _xPlayHead = 0;
        _yPlayHead = 0;
        _prevXPlayHead = 0;
        _prevYPlayHead = 0;
    }

    public void RegisterEventCallback(float time, CurveControlledBobCallback function, CurveControlledBobCallbackType type) {
        CurveControlledBobEvent ccbeEvent = new CurveControlledBobEvent();

        ccbeEvent.Time = time;
        ccbeEvent.Function = function;
        ccbeEvent.Type = type;
        _events.Add(ccbeEvent);
        _events.Sort((t1, t2) => t1.Time.CompareTo(t2.Time));
    }

    public Vector3 GetVectorOffset(float speed) {
        _xPlayHead += (speed * Time.deltaTime) / _baseInterval;
        _yPlayHead += ((speed * Time.deltaTime) / _baseInterval) * _verticalToHorizontalSpeedRatio;

        if (_xPlayHead > _curveEndTime)
            _xPlayHead -= _curveEndTime;

        if (_yPlayHead > _curveEndTime)
            _yPlayHead -= _curveEndTime;

        for (int i = 0; i < _events.Count; i++) {
            CurveControlledBobEvent ev = _events[i];
            if (ev != null) {
                if (ev.Type == CurveControlledBobCallbackType.Vertical) {
                    if ((_prevYPlayHead < ev.Time && _yPlayHead >= ev.Time) ||
                        (_prevYPlayHead > _yPlayHead && (ev.Time > _prevYPlayHead || ev.Time <= _prevYPlayHead))) {

                        ev.Function();
                    }
                }
                else {
                    if ((_prevXPlayHead < ev.Time && _xPlayHead >= ev.Time) ||
                        (_prevXPlayHead > _prevXPlayHead && (ev.Time > _prevXPlayHead || ev.Time <= _xPlayHead))) {

                        ev.Function();
                    }
                }
            }
        }

        float xPos = _bobcurve.Evaluate(_xPlayHead) * _horizontalMultiplier;
        float yPos = _bobcurve.Evaluate(_yPlayHead) * _verticalMultiplier;

        _prevXPlayHead = _xPlayHead;
        _prevYPlayHead = _yPlayHead;

        return new Vector3(xPos, yPos, 0);
    }
}

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
    private float _runStepLengthen = 0.75f;
    [SerializeField]
    private CurveControlledBob _headBob = new CurveControlledBob();

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
    private Vector3 _localSpaceCameraPos = Vector3.zero;

    private float _fallingTimer;

    public PlayerMoveStatus movementStatus { get => _movementStatus; }
    public float walkSpeed { get => _walkSpeed; }

    protected void Start() {
        _characterController = GetComponent<CharacterController>();

        _camera = Camera.main;
        _localSpaceCameraPos = _camera.transform.localPosition;

        _movementStatus = PlayerMoveStatus.NotMoving;

        _fallingTimer = 0;

        _mouseLook.Init(transform, _camera.transform);

        _headBob.Initialize();
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

        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right * _inputVector.x;

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

        if (_characterController.velocity.magnitude > 1.0f)
            _camera.transform.localPosition = _localSpaceCameraPos + _headBob.GetVectorOffset(_characterController.velocity.magnitude * (_isWalking ? 1.0f : _runStepLengthen));
        else
            _camera.transform.localPosition = _localSpaceCameraPos;
    }
}
