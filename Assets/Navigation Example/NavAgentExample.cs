using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour {

    [SerializeField]
    private int _currentIndex;
    [SerializeField]
    private AIWaypointsNetwork _waypointsNetwork;
    [SerializeField]
    private bool _hasPath;
    [SerializeField]
    private bool _pathPending;
    [SerializeField]
    private NavMeshPathStatus _pathStatus;
    [SerializeField]
    private AnimationCurve _jumpCurve = new AnimationCurve();
    private NavMeshAgent _navAgent;

    void Start() {
        _navAgent = GetComponent<NavMeshAgent>();

        if (_waypointsNetwork == null) return;

        Transform waypoint = _waypointsNetwork.Waypoints[_currentIndex];

        SetNextDestination(false);
    }

    void SetNextDestination(bool increment) {
        if (!_waypointsNetwork) return;

        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform;

        int nextWaypoint = (_currentIndex + incStep >= _waypointsNetwork.Waypoints.Count) ? 0 : _currentIndex + incStep;
        nextWaypointTransform = _waypointsNetwork.Waypoints[nextWaypoint];

        if (nextWaypointTransform != null) {
            _currentIndex = nextWaypoint;
            _navAgent.destination = nextWaypointTransform.position;
            return;
        }

        _currentIndex++;
    }

    void Update() {

        _hasPath = _navAgent.hasPath;
        _pathPending = _navAgent.pathPending;
        _pathStatus = _navAgent.pathStatus;

        if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !_pathPending) || _pathStatus == NavMeshPathStatus.PathInvalid) {
            SetNextDestination(true);
        } else if (_navAgent.isPathStale) {
            SetNextDestination(false);
        }
        if (_navAgent.isOnOffMeshLink) {
            StartCoroutine(JumpCoroutine(1.0f));
            return;
        }
    }

    IEnumerator JumpCoroutine(float duration) {
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);
        float time = 0;

        while(time <= duration) {
            float t = time / duration;
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (_jumpCurve.Evaluate(t) * Vector3.up);
            time += Time.deltaTime;
            yield return null;
        }
        _navAgent.CompleteOffMeshLink();
    }
}