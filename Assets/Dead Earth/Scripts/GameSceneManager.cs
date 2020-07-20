using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo {
	public Collider collider;
	public CharacterManager characterManager;
	public Camera camera;
	public CapsuleCollider meleeTrigger;
}

public class GameSceneManager : MonoBehaviour {

	private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();
	private Dictionary<int, PlayerInfo> _playerInfos = new Dictionary<int, PlayerInfo>();

	[SerializeField]
	private ParticleSystem _bloodParticles;

	private static GameSceneManager _instance = null;
	public static GameSceneManager instance {
		get {
			if (_instance == null)
				_instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));
			return _instance;
		}
	}

	public ParticleSystem bloodParticles { get => _bloodParticles; }

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

	public void RegisterPlayerInfo(int key, PlayerInfo playerInfo) {
		if (!_playerInfos.ContainsKey(key)) {
			_playerInfos[key] = playerInfo;
		}
	}

	public PlayerInfo GetPlayerInfo(int key) {
		PlayerInfo info = null;
		if (_playerInfos.TryGetValue(key, out info)) {
			return info;
		}
		return null;
	}
}
