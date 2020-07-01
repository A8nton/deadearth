using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -------------------------------------------------------------------------
// CLASS	:	GameSceneManager
// Desc		:	Singleton class that acts as the scene database
// -------------------------------------------------------------------------
public class GameSceneManager : MonoBehaviour {
	private static GameSceneManager _instance = null;
	public static GameSceneManager instance {
		get {
			if (_instance == null)
				_instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));
			return _instance;
		}
	}

	private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();

	// --------------------------------------------------------------------
	// Name	:	RegisterAIStateMachine
	// Desc	:	Stores the passed state machine in the dictionary with
	//			the supplied key
	// --------------------------------------------------------------------
	public void RegisterAIStateMachine(int key, AIStateMachine stateMachine) {
		if (!_stateMachines.ContainsKey(key)) {
			_stateMachines[key] = stateMachine;
		}
	}

	// --------------------------------------------------------------------
	// Name	:	GetAIStateMachine
	// Desc	:	Returns an AI State Machine reference searched on by the
	//			instance ID of an object
	// --------------------------------------------------------------------
	public AIStateMachine GetAIStateMachine(int key) {
		AIStateMachine machine = null;
		if (_stateMachines.TryGetValue(key, out machine)) {
			return machine;
		}

		return null;
	}
}
