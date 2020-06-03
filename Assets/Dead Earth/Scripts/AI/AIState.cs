﻿using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public abstract class AIState : MonoBehaviour {

    public void SetStateMachine(AIStateMachine stateMachine) {
        _stateMachine = stateMachine;
    }

    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorUpdated() { }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationReached (bool isReached) { }

    public abstract AIStateType OnUpdate();
    public abstract AIStateType GetStateType();

    protected AIStateMachine _stateMachine;
}
