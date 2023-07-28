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

    private float elapsedTime; // like elapsed time
    private float pullElapsedTime;
    private Vector3 moveDir;

    private const float rotationSpeed = 1.5f;

    public DashingState(DemoPlayer actor, float likeTimer, float pullTimer, float speed) 
    {
        this.actor = actor;
        this.likeTimer = likeTimer;
        this.pullTimer = pullTimer;
        this.speed = speed;
        this.pullElapsedTime = pullTimer;
    }

    public override void OnEnter() 
    {
        actor.GetComponent<Animator>().SetBool("Running", true);
        elapsedTime = 0.0f;
    }

    public override Type Tick() 
    {
        // switch to Idle if the player is not liking
        elapsedTime += Time.deltaTime;
        if (elapsedTime > likeTimer) 
            return typeof(IdleState);

        // perform pulling action
        pullElapsedTime += Time.deltaTime;

        // if not grounded, do nothing
        if (!actor.Grounded) return null;

        Move();
        if (pullElapsedTime > pullTimer) 
        {
            pullElapsedTime = 0.0f;
            var nearestPlayer = actor.GetNearestPlayerFacingFront();
            if (nearestPlayer != null)
                Pull(nearestPlayer);
        }
        
        return null;
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

    private void Pull(DemoPlayer otherPlayer)
    {
        Debug.Log($"{actor.name} is pulling the other player {otherPlayer.name}");
        actor.PlayPullAnimation();

        // TODO: make sure the current player is facing the other player's direction
        Vector3 facingDir = otherPlayer.transform.position - actor.transform.position;
        actor.transform.rotation = Quaternion.Slerp(
                actor.transform.rotation,
                Quaternion.LookRotation(facingDir.normalized),
                Time.deltaTime * rotationSpeed // May need to tune
            );

        // TODO: the other player gets pulled here
        otherPlayer.SwitchState(typeof(FrozenState));
        otherPlayer.PlayPulledAnimation();

        // Move up and then in the direction of player
        Vector3 forceDirection = actor.transform.position - otherPlayer.transform.position;

        otherPlayer.transform.DOMoveY(otherPlayer.transform.position.y + 4.0f, 0.1f).OnComplete(() => otherPlayer.transform.DOLocalMove(forceDirection.normalized + otherPlayer.transform.position, 0.2f));

    }

    public override void FrameUpdate()
    {
        if (actor.Grounded) {
            actor.transform.position += moveDir * speed * Time.fixedDeltaTime;
        }
    }

    public override void OnExit() 
    {
        actor.GetComponent<Animator>().SetBool("Running", false);
    }
}
