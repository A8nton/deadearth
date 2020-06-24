using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Pursuit1 : AIZombieState {

    [SerializeField]
    [Range(0, 10)]
    private float _speed = 1;
    [SerializeField]
    private float _slerpSpeed = 5;
    [SerializeField]
    private float _repathDistanceMultiplier = 0.035f;
    [SerializeField]
    private float _repathVisualMinDuration = 0.05f;
    [SerializeField]
    private float _repathVisualMaxDuration = 5;
    [SerializeField]
    private float _repathAudioMinDuration = 0.25f;
    [SerializeField]
    private float _repathAudioMaxDuration = 5;
    [SerializeField]
    private float _maxDuration = 40;

    private float _timer = 0;
    private float _repathTimer = 0;
    private bool _targetReached;

    public override AIStateType GetStateType() {
        return AIStateType.Pursuit;
    }

    public override void OnEnterState() {
        base.OnEnterState();
        if (_zombieStateMachine == null)
            return;

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = _speed;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        _timer = 0;
        _repathTimer = 0;
        _targetReached = false;

        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.targetPosition);
        _zombieStateMachine.navAgent.isStopped = false;
    }

    public override AIStateType OnUpdate() {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        if (_timer > _maxDuration)
            return AIStateType.Patrol;

        if (_stateMachine.targetType == AITargetType.VisualPlayer && _zombieStateMachine.inMeleeRange) {
            return AIStateType.Attack;
        }

        if (_targetReached) {
            switch (_stateMachine.targetType) {
                case AITargetType.Audio:
                case AITargetType.VisualLight:
                    _stateMachine.ClearTarget();
                    return AIStateType.Alerted;

                case AITargetType.VisualFood:
                    return AIStateType.Feeding;
            }
        }

        if (_zombieStateMachine.navAgent.isPathStale ||
            !_zombieStateMachine.navAgent.hasPath ||
            _zombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete) {
            return AIStateType.Alerted;
        }

        if (!_zombieStateMachine.useRootRotation && _zombieStateMachine.targetType == AITargetType.VisualPlayer &&
            _zombieStateMachine.VisualThreat.type == AITargetType.VisualPlayer && _targetReached) {
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = newRot;

        } else if (!_stateMachine.useRootRotation && !_targetReached) {
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);

        } else if (_targetReached) {
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.VisualPlayer) {
            if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position) {
                if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer) {
                    _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.VisualThreat.position);
                    _repathTimer = 0;
                }
            }

            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);

            return AIStateType.Pursuit;
        }

        if (_zombieStateMachine.targetType == AITargetType.VisualPlayer)
            return AIStateType.Pursuit;

        if (_zombieStateMachine.VisualThreat.type == AITargetType.VisualLight) {
            if (_zombieStateMachine.targetType == AITargetType.Audio || _zombieStateMachine.targetType == AITargetType.VisualFood) {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                return AIStateType.Alerted;
            }
        }

        return AIStateType.Pursuit;
    }

    public override void OnDestinationReached(bool isReached) {
        if (_stateMachine == null)
            return;
    }
}
