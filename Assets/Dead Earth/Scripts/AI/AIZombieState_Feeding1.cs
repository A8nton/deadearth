using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Feeding1 : AIZombieState {

    [SerializeField]
    private float _slerpSpeed = 5;
    [SerializeField]
    private Transform _bloodParticlesMount;
    [SerializeField]
    [Range(0.01f, 1)]
    private float _bloodParticlesBurstTime = 0.1f;
    [SerializeField]
    [Range(1, 100)]
    private int _bloodParticlesBurstAmount = 10;

    private int _eatingStateHash = Animator.StringToHash("Feeding State");
    private int _crawlEatingStateHash = Animator.StringToHash("Crawl Feeding State");
    private int _eatingLayerIndex = -1;
    private float _timer;

    public override AIStateType GetStateType() {
        return AIStateType.Feeding;
    }

    public override void OnEnterState() {
        Debug.Log("Entering feeding state");

        base.OnEnterState();

        if (_zombieStateMachine == null)
            return;

        if (_eatingLayerIndex == -1)
            _eatingLayerIndex = _zombieStateMachine.animator.GetLayerIndex("Cinematic");

        _timer = 0;

        _zombieStateMachine.feeding = true;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.attackType = 0;

        _stateMachine.NavAgentControl(true, false);
    }

    public override void OnExitState() {
        base.OnExitState();

        if (_zombieStateMachine != null)
            _zombieStateMachine.feeding = false;
    }

    public override AIStateType OnUpdate() {
        _timer += Time.deltaTime;

        if (_zombieStateMachine.satisfaction > 0.9f) {
            _zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.VisualThreat.type != AITargetType.None && _zombieStateMachine.VisualThreat.type != AITargetType.Visual_Food) {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio) {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        int currentHash = _zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash;
        if (currentHash == _eatingStateHash || currentHash == _crawlEatingStateHash) {
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + ((Time.deltaTime * _zombieStateMachine.replenishRate) / 100), 1);
            if (GameSceneManager.instance && GameSceneManager.instance.bloodParticles && _bloodParticlesMount) {

                if (_timer > _bloodParticlesBurstTime) {

                    ParticleSystem system = GameSceneManager.instance.bloodParticles;
                    system.transform.position = _bloodParticlesMount.transform.position;
                    system.transform.rotation = _bloodParticlesMount.transform.rotation;
                    var settings = system.main;
                    settings.simulationSpace = ParticleSystemSimulationSpace.World;
                    system.Emit(_bloodParticlesBurstAmount);
                    _timer = 0;
                }
            }
        }

        if (!_zombieStateMachine.useRootRotation) {
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        Vector3 headToTarget = _zombieStateMachine.targetPosition - _zombieStateMachine.animator.GetBoneTransform(HumanBodyBones.Head).position;
        _zombieStateMachine.transform.position = Vector3.Lerp(_zombieStateMachine.transform.position,
            _zombieStateMachine.transform.position + headToTarget,
            Time.deltaTime);

        return AIStateType.Feeding;
    }
}
