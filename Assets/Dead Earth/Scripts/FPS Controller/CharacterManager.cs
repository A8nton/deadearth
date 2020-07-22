using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {

    [SerializeField]
    private CapsuleCollider _meleeTrigger;
    [SerializeField]
    private CameraBloodEffect _cameraBloodEffect;
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private float _health = 100.0f;

    private Collider _collider;
    private FPSController _fpsController;
    private CharacterController _characterController;
    private GameSceneManager _gameSceneManager;
    private int _aiBodyPartLayer = -1;

    void Start() {
        _collider = GetComponent<Collider>();
        _fpsController = GetComponent<FPSController>();
        _characterController = GetComponent<CharacterController>();
        _gameSceneManager = GameSceneManager.instance;

        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        if (_gameSceneManager != null) {
            PlayerInfo info = new PlayerInfo();
            info.camera = _camera;
            info.characterManager = this;
            info.collider = _collider;
            info.meleeTrigger = _meleeTrigger;

            _gameSceneManager.RegisterPlayerInfo(_collider.GetInstanceID(), info);
        }
    }

    public void TakeDamage(float amount) {
        _health = Mathf.Max(_health - (amount * Time.deltaTime), 0.0f);
        if (_cameraBloodEffect != null) {
            _cameraBloodEffect.minBloodAmount = (1.0f - (_health / 100.0f)) / 3.0f;
            _cameraBloodEffect.bloodAmount = Mathf.Min(_cameraBloodEffect.minBloodAmount + 3.0f, 1.0f);
        }
    }

    public void DoDamage(int hitDirection = 0) {

        if (_camera == null || _gameSceneManager == null)
            return;

        Ray ray;
        RaycastHit hit;
        bool isSomethingHit;

        ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        isSomethingHit = Physics.Raycast(ray, out hit, 1000.0f, 1 << _aiBodyPartLayer);

        if (isSomethingHit) {
            AIStateMachine stateMachine = _gameSceneManager.GetAIStateMachine(hit.rigidbody.GetInstanceID());
            if (stateMachine) {
                stateMachine.TakeDamage(hit.point, ray.direction * 1.0f, 25, hit.rigidbody, this, 0);
            }
        }
    }

    public void Update() {
        if (Input.GetMouseButtonDown(0)) {
            DoDamage();
        }
    }
}
