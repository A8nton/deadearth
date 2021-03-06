﻿using UnityEngine;
using System.Collections;

public class AISensor : MonoBehaviour {
	private AIStateMachine _parentStateMachine = null;
	public AIStateMachine parentStateMachine { set { _parentStateMachine = value; } }

	void OnTriggerEnter(Collider col) {
		if (_parentStateMachine != null)
			_parentStateMachine.OnTriggerEvent(AITriggerEventType.Enter, col);
	}

	void OnTriggerStay(Collider col) {
		if (_parentStateMachine != null)
			_parentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, col);
	}

	void OnTriggerExit(Collider col) {
		if (_parentStateMachine != null)
			_parentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, col);
	}

}
