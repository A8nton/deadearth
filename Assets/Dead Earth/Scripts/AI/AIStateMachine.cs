using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType { None, Waypoint, VisualPlayer, VisualLight, VisualFood, Audio }
public enum AITriggerEventType { Enter, Stay, Exit }

public struct AITarget {
    private AITargetType _type;
    private Collider _collider;
    private Vector3 _position;
    private float _distance;
    private float _time;

    public AITargetType type { get { return _type; } }
    public Collider collider { get { return _collider; } }
    public Vector3 position { get { return _position; } }
    public float distance { get { return _distance; } set { _distance = value; } }
    public float time { get { return _time; } }

    public void Set(AITargetType t, Collider c, Vector3 p, float d) {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _time = Time.time;
    }

    public void Clear() {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _time = 0.0f;
        _distance = Mathf.Infinity;
    }
}

public abstract class AIStateMachine : MonoBehaviour {

    public AITarget VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();

    protected AIState _currentState;
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();

    [SerializeField]
    protected SphereCollider _targetTrigger;

    [SerializeField]
    protected SphereCollider _sensorTrigger;

    [SerializeField]
    [Range(0, 15)]
    protected float _stoppingDistance = 1;

    [SerializeField]
    protected AIStateType _currentStateType = AIStateType.Idle;

    protected Animator _animator;
    protected NavMeshAgent _navAgent;
    protected Collider _collider;
    protected Transform _transform;

    public Animator animator { get { return _animator; } }
    public NavMeshAgent navAgent { get { return _navAgent; } }

    protected virtual void Awake() {
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
    }

    protected virtual void Start() {
        AIState[] states = GetComponents<AIState>();

        foreach (AIState state in states)
            if (state != null && !_states.ContainsKey(state.GetStateType())) {
                _states[state.GetStateType()] = state;
                state.SetStateMachine(this);
            }

        if (_states.ContainsKey(_currentStateType)) {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        } else {
            _currentState = null;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d) {
        _target.Set(t, c, p, d);

        if (_targetTrigger != null) {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s) {
        _target.Set(t, c, p, d);

        if (_targetTrigger != null) {
            _targetTrigger.radius = s;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITarget t) {
        _target = t;

        if (_targetTrigger != null) {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void ClearTarget() {
        _target.Clear();

        if (_targetTrigger != null)
            _targetTrigger.enabled = false;
    }

    protected virtual void FixedUpdate() {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if (_target.type != AITargetType.None)
            _target.distance = Vector3.Distance(_transform.position, _target.position);
    }

    protected virtual void Update() {
        if (_currentState == null)
            return;

        AIStateType newStateType = _currentState.OnUpdate();
        if (newStateType != _currentStateType) {

            AIState newState = null;

            if (_states.TryGetValue(newStateType, out newState)) {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            } else
            if (_states.TryGetValue(AIStateType.Idle, out newState)) {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
        }
    }
}
