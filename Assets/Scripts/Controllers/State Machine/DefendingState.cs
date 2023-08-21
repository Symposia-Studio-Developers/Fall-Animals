using UnityEngine;
using Fall_Friends.States;
using Fall_Friends.Controllers;
using System;
using Fall_Friends.Manager;
using Random = UnityEngine.Random;


public class DefendingState : BaseState
{   
    private readonly DemoPlayer actor;
    private readonly float pushTimer;
    private readonly float pushForce;
    private readonly float pushRadius;
    private readonly float speed;

    private const float rotationSpeed = 1.5f;

    private float pushElapsedTime;
    private Rigidbody _rb;
    private float _distToCenter;
    private Vector3 moveDir;

    public DefendingState(DemoPlayer actor, float pushTimer, float pushForce, float pushRadius, float speed) 
    {
        this.actor = actor;
        this.pushTimer = pushTimer;
        this.pushForce = pushForce;
        this.pushRadius = pushRadius;
        this._rb = actor.GetComponent<Rigidbody>();
        this.speed = speed;
        if (actor.IsBot)
        {
            this.pushTimer *= 2;
        }
    }

    public override void OnEnter()
    {
        _distToCenter = Random.Range(0.5f, 3.0f);
    }

    public override Type Tick()
    {
        if (!actor.OnMiddleGround) return typeof(IdleState);
        if (!actor.Grounded) return null;
        
        MoveToCenter();

        if (!actor.IsBot)
            GainMass();

        Push();
        
        return null;
    }

    public override void FrameUpdate()
    {
        if (actor.Grounded) {
            actor.transform.position += moveDir * speed * Time.fixedDeltaTime;
        }
    }

    public override void OnExit()
    {
        if (actor.GetComponent<Animator>().GetBool("Dancing"))
            actor.GetComponent<Animator>().SetBool("Dancing", false);
    }

    private void Push() 
    {
        pushElapsedTime += Time.deltaTime;
        var nearestPlayer = actor.GetNearestPlayer(pushRadius * (_rb.mass/10 + 1));
        if (nearestPlayer == null) 
        {
            actor.GetComponent<Animator>().SetBool("Dancing", true);
        } 
        else 
        {
            if (pushElapsedTime > pushTimer) 
            {
                PushHelper(nearestPlayer);
                pushElapsedTime = 0.0f;
            }
        }
    }

    private void PushHelper(DemoPlayer otherPlayer) 
    {
        Debug.Log($"{actor.name} is pushing the other player {otherPlayer.name}");
        actor.PlayPushAnimation();
        Vector3 forceDirection = otherPlayer.transform.position - actor.transform.position;
        forceDirection.y = 0;
        actor.transform.LookAt(otherPlayer.transform);

        otherPlayer.SwitchState(typeof(FrozenState));

        otherPlayer.GetComponent<Rigidbody>().AddForce(forceDirection.normalized * pushForce, ForceMode.Impulse);
    }

    private void GainMass() {
        _rb.mass = _rb.mass * 1.00005f;
        var trans = _rb.transform;
        trans.localScale = trans.localScale * 1.00005f;
        if (GameManager.Instance.CurrentLeader == null || trans.localScale.x > GameManager.Instance.CurrentLeader.transform.localScale.x)
        {
            GameManager.Instance.ChangeLeader(_rb.GetComponent<DemoPlayer>());
        }
    }

    private void MoveToCenter() {
        Vector3 centerPos = GameManager.Instance.CenterPosition;
        Vector3 distVec = centerPos - new Vector3(actor.transform.position.x, centerPos.y, actor.transform.position.z);
        float dist = distVec.magnitude;

        if (dist > _distToCenter) {
            moveDir = distVec.normalized;
            moveDir = Vector3.ClampMagnitude(moveDir, 1);
        } else {
            moveDir = Vector3.zero;
        }
    }

    
}
