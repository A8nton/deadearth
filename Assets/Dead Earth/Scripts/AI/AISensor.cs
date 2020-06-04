using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISensor : MonoBehaviour {

    private AIStateMachine _parentStateMachine;

    public AIStateMachine parentStateMachine { set { _parentStateMachine = value; } }

    public void OnTriggerEnter(Collider other) {
        if (_parentStateMachine != null)
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Enter, other);
    }
    public void OnTriggerStay(Collider other) {
        if (_parentStateMachine != null)
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, other);
    }
    public void OnTriggerExit(Collider other) {
        if (_parentStateMachine != null)
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, other);
    }
}
