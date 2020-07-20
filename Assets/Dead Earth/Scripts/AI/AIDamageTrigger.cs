using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScript.Steps;

public class AIDamageTrigger : MonoBehaviour {

    [SerializeField]
    private string _parameter = "";
    [SerializeField]
    private int _bloodParticlesBurstAmount = 10;
    [SerializeField]
    private float _damageAmount = 0.1f;

    private AIStateMachine _stateMachine;
    private Animator _animator;
    private int _parameterHash = -1;
    private GameSceneManager _gameSceneManager;

    public void Start() {
        _stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();
        if (_stateMachine != null)
            _animator = _stateMachine.animator;

        _parameterHash = Animator.StringToHash(_parameter);

        _gameSceneManager = GameSceneManager.instance;
    }

    public void OnTriggerStay(Collider other) {
        if (!_animator)
            return;

        if (other.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f) {
            if (GameSceneManager.instance && GameSceneManager.instance.bloodParticles) {
                ParticleSystem system = GameSceneManager.instance.bloodParticles;

                system.transform.position = transform.position;
                system.transform.rotation = Camera.main.transform.rotation;

                system.simulationSpace = ParticleSystemSimulationSpace.World;
                system.Emit(_bloodParticlesBurstAmount);
            }

            if (_gameSceneManager != null) {
                PlayerInfo info = _gameSceneManager.GetPlayerInfo(other.GetInstanceID());
                if (info != null && info.characterManager != null) {
                    info.characterManager.TakeDamage(_damageAmount);
                }
            }
        }
    }
}
