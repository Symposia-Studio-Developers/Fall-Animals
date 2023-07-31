using UnityEngine;
using Fall_Friends.States;
using Fall_Friends.Controllers;
using System;

public class DefendingState : BaseState
{   
    private readonly DemoPlayer actor;
    private readonly float likeTimer;
    private readonly float pushTimer;
    private readonly float pushForce;
    private readonly float pushRadius;

    private const float rotationSpeed = 1.5f;

    private float elapsedTime;
    private float pushElapsedTime;

    public DefendingState(DemoPlayer actor, float likeTimer, float pushTimer, float pushForce, float pushRadius) 
    {
        this.actor = actor;
        this.likeTimer = likeTimer;
        this.pushTimer = pushTimer;
        this.pushForce = pushForce;
        this.pushRadius = pushRadius;
    }

    public override void OnEnter() 
    {
        elapsedTime = 0.0f;
    }

    public override Type Tick()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > likeTimer)
            return typeof(IdleState);
        
        if (!actor.Grounded) return null;

        RaycastHit hit;
        if (Physics.Raycast(actor.transform.position, -Vector3.up, out hit)) {
            // Debug.Log($"Found an object {hit.collider.name}, with distanc {hit.distance}");

            if (!hit.collider.CompareTag("MiddleGround"))
                return typeof(IdleState);
        }
            
        Push();
        
        return null;
    }

    private void Push() 
    {
        pushElapsedTime += Time.deltaTime;
        var nearestPlayer = actor.GetNearestPlayer();
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
        // actor.transform.rotation = Quaternion.Slerp(
        //         actor.transform.rotation,
        //         Quaternion.LookRotation(forceDirection.normalized),
        //         Time.deltaTime * rotationSpeed // May need to tune
        //     );
        actor.transform.LookAt(otherPlayer.transform);

        otherPlayer.SwitchState(typeof(FrozenState));

        otherPlayer.GetComponent<Rigidbody>().AddForce(forceDirection.normalized * pushForce, ForceMode.Impulse);
    }

    public override void OnExit()
    {
        actor.GetComponent<Animator>().SetBool("Dancing", false);
    }

    
}
