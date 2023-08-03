using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fall_Friends.States;
using Fall_Friends.Controllers;
using System;

public class IdleState : BaseState
{
    private readonly DemoPlayer actor;
    private readonly float speed;
    private readonly float _idleToDashCoolDown;

    private float _idleToDashTimer;

    public IdleState(DemoPlayer actor, float speed, float IdleToDashCoolDown) 
    {
        this.actor = actor;
        this.speed = speed;
    }

    public override Type Tick()
    {       
        if (actor.IsBot) {
            if (actor.Grounded) {
                _idleToDashTimer += Time.deltaTime;
                if (_idleToDashTimer > _idleToDashCoolDown) {
                    return typeof(DashingState);
                }
            }
        }

        if (actor.OnMiddleGround)
            return typeof(DefendingState);
        return null;
    }
    
}
