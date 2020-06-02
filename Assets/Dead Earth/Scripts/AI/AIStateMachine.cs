using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public enum AIStateType { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType { None, Waypoint, VisualPlayer, VisualLight, VisualFood, Audio }

public struct AITarget {
    private AITargetType _type;
    private Collider _collider;
    private Vector3 _position;
    private float _distance;
    private float _time;

    private AITargetType type { get { return _type; } }
    private Collider collider { get { return _collider; } }
    private Vector3 position { get { return _position; } }
    private float distance { get { return _distance; } set { _distance = value; } }
    private float time { get { return _time; } }

    public void Set(AITargetType t, Collider c, Vector3 p, float d) {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _time = Time.time;
    }
}

public abstract class AIStateMachine : MonoBehaviour {

    private Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();

    protected virtual void Start() {
        AIState[] states = GetComponents<AIState>();

        foreach (AIState state in states)
            if (state != null && !_states.ContainsKey(state.GetStateType())) {
                _states[state.GetStateType()] = state;
            }
    }
}
