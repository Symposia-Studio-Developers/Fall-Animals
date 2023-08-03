using UnityEngine;
using Fall_Friends.States;
using Fall_Friends.Controllers;
using System;
using Fall_Friends.Manager;
using DG.Tweening;

public class DashingState : BaseState
{
    private readonly DemoPlayer actor;
    private readonly float likeTimer;
    private readonly float pullTimer;
    private readonly float speed;
    private readonly float pullRadius;

    private float pullElapsedTime;
    private Vector3 moveDir;
    private Animator animator;

    private const float rotationSpeed = 1.5f;

    public DashingState(DemoPlayer actor, float pullTimer, float pullRadius, float speed) 
    {
        this.actor = actor;
        this.pullTimer = pullTimer;
        this.speed = speed;
        this.pullElapsedTime = pullTimer;
        this.pullRadius = pullRadius;
        this.animator = actor.GetComponent<Animator>();
    }

    public override void OnEnter() 
    {
        // if (this.animator.GetBool("Grounded"))
        //     this.animator.SetBool("Grounded", false);
        this.animator.SetBool("Running", true);
        this.animator.SetFloat("RunningSpeed", speed);
    }

    public override Type Tick() 
    {
        // if not grounded, do nothing
        if (!actor.Grounded) return null;

        if (actor.OnMiddleGround) 
            return typeof(DefendingState);

        Move();

        Pull();

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
        this.animator.SetBool("Running", false);
        this.animator.SetFloat("RunningSpeed", 1);
    }

    private void Move () {
        // get the moving direction
        Vector3 centerPos = GameManager.Instance.CenterPosition;
        moveDir = (centerPos - new Vector3(actor.transform.position.x, centerPos.y, actor.transform.position.z)).normalized;
        moveDir = Vector3.ClampMagnitude(moveDir, 1);

        // make sure the player is always facing the moving direction
        if (moveDir != Vector3.zero) 
        { 
            actor.transform.rotation = Quaternion.Slerp(
                actor.transform.rotation,
                Quaternion.LookRotation(moveDir),
                Time.deltaTime * rotationSpeed
            );
        }
    }

    private void Pull () {
        // perform pulling action
        pullElapsedTime += Time.deltaTime;
        if (pullElapsedTime > pullTimer) 
        {
            pullElapsedTime = 0.0f;
            var nearestPlayer = actor.GetNearestPlayerFacingFront(pullRadius);
            if (nearestPlayer != null) 
                PullHelper(nearestPlayer);
        }
    }

    private void PullHelper(DemoPlayer otherPlayer)
    {
        Debug.Log($"{actor.GetPlayerId()} is pulling the other player {otherPlayer.GetPlayerId()}");
        actor.PlayPullAnimation();

        // TODO: make sure the current player is facing the other player's direction
        actor.transform.LookAt(otherPlayer.transform);

        otherPlayer.SwitchState(typeof(FrozenState));
        otherPlayer.PlayPulledAnimation();

        // Move up and then in the direction of player
        Vector3 forceDirection = actor.transform.position - otherPlayer.transform.position;

        otherPlayer.transform.DOMoveY(otherPlayer.transform.position.y + 4.0f, 0.2f).OnComplete(() => otherPlayer.transform.DOLocalMove(forceDirection.normalized + otherPlayer.transform.position + new Vector3(0, 0, -1), 0.2f));

    }
}
