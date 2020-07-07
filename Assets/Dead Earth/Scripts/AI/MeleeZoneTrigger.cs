using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeZoneTrigger : MonoBehaviour {

    public void OnTriggerEnter(Collider other) {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());
        if (machine) {
            machine.inMeleeRange = true;
        }
    }

    public void OnTriggerExit(Collider other) {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());
        if (machine) {
            machine.inMeleeRange = false;
        }
    }
}
