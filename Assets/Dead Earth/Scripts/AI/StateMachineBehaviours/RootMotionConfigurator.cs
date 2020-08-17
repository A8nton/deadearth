using System.Collections;
using UnityEngine;

// ------------------------------------------------------------
// CLASS	:	RootMotionConfigurator
// DESC		:	A State Machine Behaviour that communicates
//				with an AIStateMachine derived class to
//				allow for enabling/disabling root motion on
//				a per animation state basis.
// ------------------------------------------------------------
public class RootMotionConfigurator : AIStateMachineLink {
	[SerializeField] private int _rootPosition = 0;
	[SerializeField] private int _rootRotation = 0;

	private bool _rootMotionProcessed;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex) {
		if (_stateMachine) {
			_stateMachine.AddRootMotionRequest(_rootPosition, _rootRotation);
			_rootMotionProcessed = true;
		}
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex) {
		// Inform the AI State Machine that we wish to relinquish our root motion request.
		if (_stateMachine && _rootMotionProcessed) {
			_stateMachine.AddRootMotionRequest(-_rootPosition, -_rootRotation);
			_rootMotionProcessed = false;
		}
	}
}
