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

    public IdleState(DemoPlayer actor, float speed) 
    {
        this.actor = actor;
        this.speed = speed;
    }

    public override Type Tick()
    {
        return null;
    }
    
}
