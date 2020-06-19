using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Patrol1 : AIZombieState {

    [SerializeField]
    private AIWaypointsNetwork _waypointNetwork;
    [SerializeField]
    private bool _randomPatrol;
    [SerializeField]
    private int _currentWaypoint;
    [SerializeField]
    private float _turnOnSpotThreshold = 80.0f;
    [SerializeField]
    private float _slerpSpeed = 5.0f;

    [SerializeField]
    [Range(0, 3)]
    private float _speed = 1.0f;

    public override AIStateType GetStateType() {
        return AIStateType.Patrol;
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

        if (_zombieStateMachine.targetType != AITargetType.Waypoint) {
            _zombieStateMachine.ClearTarget();

            if (_waypointNetwork != null && _waypointNetwork.Waypoints.Count > 0) {
                if (_randomPatrol) {
                    _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
                }

                if (_currentWaypoint < _waypointNetwork.Waypoints.Count) {
                    Transform waypoint = _waypointNetwork.Waypoints[_currentWaypoint];
                    if (waypoint != null) {
                        _zombieStateMachine.SetTarget(AITargetType.Waypoint, null, waypoint.position,
                            Vector3.Distance(_zombieStateMachine.transform.position, waypoint.position));

                        _zombieStateMachine.navAgent.SetDestination(waypoint.position);
                    }
                }
            }
        }
        _zombieStateMachine.navAgent.Resume();
    }
    public override AIStateType OnUpdate() {

        if (_zombieStateMachine.VisualThreat.type == AITargetType.VisualPlayer) {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }
        if (_zombieStateMachine.VisualThreat.type == AITargetType.VisualLight) {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }
        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio) {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }
        if (_zombieStateMachine.VisualThreat.type == AITargetType.VisualFood) {
            if ((1.0f - _zombieStateMachine.satisfaction) > (_zombieStateMachine.VisualThreat.distance / _zombieStateMachine.sensorRadius)) {
                _stateMachine.SetTarget(_stateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }
        }

        float angle = Vector3.Angle(_zombieStateMachine.transform.forward, (_zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position));
        if (angle > _turnOnSpotThreshold) {
            return AIStateType.Alerted;
        }
        if (!_zombieStateMachine.useRootRotation) {
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }
        if (_zombieStateMachine.navAgent.isPathStale ||
            !_zombieStateMachine.navAgent.hasPath ||
            _zombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete) {
            NextWaypoint();
        }

        return AIStateType.Patrol;
    }

    private void NextWaypoint() {

        if (_randomPatrol && _waypointNetwork.Waypoints.Count > 1) {
            int oldWaypoint = _currentWaypoint;
            while (_currentWaypoint == oldWaypoint) {
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
            }
        } else {
            _currentWaypoint = _currentWaypoint == _waypointNetwork.Waypoints.Count - 1 ? 0 : _currentWaypoint + 1;
        } if (_waypointNetwork.Waypoints[_currentWaypoint] != null) {
            Transform newWaypoint = _waypointNetwork.Waypoints[_currentWaypoint];

            _zombieStateMachine.SetTarget(AITargetType.Waypoint, null, newWaypoint.position,
                Vector3.Distance(newWaypoint.position, _zombieStateMachine.transform.position));

            _zombieStateMachine.navAgent.SetDestination(newWaypoint.position);

        }
    }
    public override void OnDestinationReached(bool isReached) {
        if (_zombieStateMachine == null || !isReached)
            return;

        if (_zombieStateMachine.targetType == AITargetType.Waypoint)
            NextWaypoint();
    }
}
