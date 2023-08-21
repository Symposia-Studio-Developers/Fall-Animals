using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fall_Friends.States;
using Fall_Friends.Controllers;
using System;

public class FrozenState : BaseState
{
    private readonly DemoPlayer actor;
    private readonly float freezeTimer;
    
    private float elapsedFreezeTime;

    public FrozenState(DemoPlayer actor, float freezeTimer) 
    {
        this.actor = actor;
        this.freezeTimer = freezeTimer;
    }

    public override void OnEnter()
    {
        elapsedFreezeTime = 0.0f;
    }

    public override Type Tick()
    {
        elapsedFreezeTime += Time.deltaTime;
        if (elapsedFreezeTime > freezeTimer)
        {
            return typeof(IdleState); // if maximum frozen time achieved, switch back to Idle State
        }
        return null;
    }
}
