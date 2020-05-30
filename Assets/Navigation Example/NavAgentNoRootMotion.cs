using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour {

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
    private Animator _animator;
    private float _originalMaxSpeed;

    void Start() {
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();

        if (_navAgent)
            _originalMaxSpeed = _navAgent.speed;

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

        int _turnOnSpot = 0;

        _hasPath = _navAgent.hasPath;
        _pathPending = _navAgent.pathPending;
        _pathStatus = _navAgent.pathStatus;

        Vector3 cross = Vector3.Cross(transform.forward, _navAgent.desiredVelocity.normalized);
        float horizontal = cross.y < 0 ? -cross.magnitude : cross.magnitude;
        horizontal = Mathf.Clamp(horizontal * 2.32f, -2.32f, 2.32f);

        if (_navAgent.desiredVelocity.magnitude < 1)
            Debug.Log("magnitude: " + _navAgent.desiredVelocity.magnitude);

        if (!_hasPath)
            _navAgent.speed = 0.1f;

        if (_navAgent.desiredVelocity.magnitude < 1.0f && Vector3.Angle(transform.forward, _navAgent.desiredVelocity) > 10.0f) {

            _navAgent.speed = 0.1f;
            _turnOnSpot = (int)Mathf.Sign(horizontal);
        } else if (_hasPath) {

            _navAgent.speed = _originalMaxSpeed;
            _turnOnSpot = 0;
        }

        _animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        _animator.SetFloat("Vertical", _navAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        _animator.SetInteger("TurnOnSpot", _turnOnSpot);

        if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !_pathPending) || _pathStatus == NavMeshPathStatus.PathInvalid) {
            SetNextDestination(true);
        } else if (_navAgent.isPathStale) {
            SetNextDestination(false);
        }
        /*if (_navAgent.isOnOffMeshLink) {
            StartCoroutine(JumpCoroutine(1.0f));
            return;
        } */
    }

    IEnumerator JumpCoroutine(float duration) {
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);
        float time = 0;

        while (time <= duration) {
            float t = time / duration;
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (_jumpCurve.Evaluate(t) * Vector3.up);
            time += Time.deltaTime;
            yield return null;
        }
        _navAgent.CompleteOffMeshLink();
    }
}