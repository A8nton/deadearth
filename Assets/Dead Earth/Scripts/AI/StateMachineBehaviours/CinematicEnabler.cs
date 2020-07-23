﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicEnabler : AIStateMachineLink {

    public bool OnEnter;
    public bool OnExit;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (_stateMachine)
            _stateMachine.cinematicEnabled = OnEnter;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (_stateMachine)
            _stateMachine.cinematicEnabled = OnExit;
    }
}
