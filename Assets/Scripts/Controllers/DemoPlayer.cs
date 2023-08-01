﻿using System;
using UnityEngine;
using Fall_Friends.States;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;
using Fall_Friends.Manager;
using UnityEngine.Events;

namespace Fall_Friends.Controllers
{
    public class DemoPlayer : BaseAI
    {
        #region Public/Editable Variables

        [Header("Ground Check")]
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.3f;

        [Header("Push and Pull")]
        public float PushPullRadius = 2.0f;
        public float PushForce = 25f;

        [Header("Movement")]
        public float MaxIdleSpeed = 1f;
        public float MaxActiveSpeed = 3f;

        [Header("Timer")]
        public float MaxLikeDuration = 10f;
        public float MaxFreezeTime = 3f;
        public float PushCoolDown = 4f;
        public float PullCoolDown = 4f;

        [Header("Events")]
        [Space]

        public UnityEvent OnLandEvent;

        [Header("Debug")]
        [SerializeField] private string _playerId;// unique playerId parsed from tiktok
        //note, exposing playerId as a public variable now to set it in Unity's inspector for testing and initial setup, 
        //encapsulate it and provide methods to access and modify it later in production for safety. make playerId private and provide a public getter and setter.
        public bool IsActive = false;
        #endregion

        #region Variables
        private Animator _animator;
        private Rigidbody _rb;
        #endregion


        #region Unity Functions
        protected override void Start() {
            currentState = new IdleState(this, MaxIdleSpeed);

            availableStates = new Dictionary<Type, BaseState>() {
                {typeof(IdleState), currentState}, 
                {typeof(DashingState), new DashingState(this, MaxLikeDuration, PullCoolDown, MaxActiveSpeed)}, 
                {typeof(DefendingState), new DefendingState(this, MaxLikeDuration, PushCoolDown, PushForce, PushPullRadius)},
                {typeof(FrozenState), new FrozenState(this, MaxFreezeTime)}
            };

            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody>();

            if (IsActive) SwitchState(typeof(DashingState)); 
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void FixedUpdate()
        {
            if (_rb == null) 
                Debug.LogError("Body is null");
            if (GameManager.Instance == null) 
                Debug.LogError("GameManager instance is null");

            base.FixedUpdate();

            // check whether the player is on the ground
            bool wasGrounded = Grounded;
		    Grounded = false;
            Collider[] colliders = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, whatIsGround);
            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i].gameObject != gameObject) {
                    Grounded = true;
                    // if (!wasGrounded) {
                    //     // TODO: ADD FALL/STOP FALLING ANIMATIONS
                    //     _animator.SetBool("Falling", false);
                    // }
                }
            }

            //if (!Grounded) Debug.Log("The player is not grounded");

            if (_rb.velocity.y < -0.1)
                Falling = true;
            else Falling = false;
        }

        private IEnumerator OnCollisionEnter(Collision other) 
        {
            if (other.gameObject.CompareTag("MiddleGround")) 
            {
                float randomWaitTime = Random.Range(1.0f, 2.0f);
                yield return new WaitForSeconds(randomWaitTime);
                SwitchState(typeof(DefendingState));
            }
        }

        private void OnCollisionStay(Collision other)
        {
            if (other.gameObject.CompareTag("MiddleGround"))
            {
                _rb.mass = _rb.mass * 1.0001f;
                var trans = _rb.transform;
                trans.localScale = trans.localScale * 1.0001f;
            }
            
            if (other.gameObject.CompareTag("Ring")) 
            {
                transform.SetParent(other.gameObject.transform);
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.CompareTag("MiddleGround"))
            {
                // SwitchState(typeof(IdleState));
            }
            
            if (other.gameObject.CompareTag("Ring")) 
            {
                transform.SetParent(null);
            }
        }

        #endregion

        #region Push and Pull
        private void Rotate() {
            // transform.rotation = Quaternion.Slerp(
            //     transform.rotation,
            //     Quaternion.LookRotation(facingDir.normalized),
            //     Time.deltaTime * rotationSpeed // May need to tune
            // );
        }

        public DemoPlayer GetNearestPlayer() {
            Collider[] colliders = Physics.OverlapSphere(transform.position, PushPullRadius);
            if (colliders.Length == 0) return null; // no nearby players

            DemoPlayer nearest = null; // otherwise, find the nearest
            float nearDist = float.PositiveInfinity;
            foreach (var collider in colliders)
            {
                DemoPlayer otherPlayer = collider.GetComponent<DemoPlayer>();
                if (otherPlayer != null && otherPlayer != this) {
                    Vector3 offset = transform.position - otherPlayer.transform.position;
                    float thisDist = offset.sqrMagnitude;
                    if (thisDist < nearDist) {
                        nearDist = thisDist;
                        nearest = otherPlayer;
                    }
                }
            }
            return nearest;
        }

        public DemoPlayer GetNearestPlayerFacingFront() {
            Collider[] colliders = Physics.OverlapSphere(transform.position, PushPullRadius);
            if (colliders.Length == 0) return null;

            Vector3 mgrPos = GameManager.Instance.transform.position;
            float playerManagerDistance = (transform.position - mgrPos).sqrMagnitude;

            DemoPlayer nearest = null;
            float nearDist = float.PositiveInfinity;
            foreach (var collider in colliders) {
                DemoPlayer otherPlayer = collider.GetComponent<DemoPlayer>();
                if (otherPlayer != null && otherPlayer != this) {
                    float otherPlayerManagerDistance = (otherPlayer.transform.position - mgrPos).sqrMagnitude;
                    if (otherPlayerManagerDistance < playerManagerDistance) {
                        Vector3 offset = transform.position - otherPlayer.transform.position;
                        float thisDist = offset.sqrMagnitude;
                        if (thisDist < nearDist) {
                            nearDist = thisDist;
                            nearest = otherPlayer;
                        }
                    }
                }
            }
            return nearest;
        }
        #endregion

        #region Animations
        public void PlayPushAnimation() 
        {
            StartCoroutine(PlayAnimationHelper("Pushing", true));
        }

        public void PlayPullAnimation() 
        {
            StartCoroutine(PlayAnimationHelper("Pulling", true));
        }

        public void PlayPulledAnimation()
        {
            StartCoroutine(PlayAnimationHelper("Pulled", true, 1));
        }

        public void PlayFallingAnimation()
        {
            StartCoroutine(PlayAnimationHelper("Falling", true));
        }
        #endregion

        #region Helper Functions
        IEnumerator SwitchToDefendingStateHelper ()
        {   
            Debug.Log($"SwitchToDefend Helper, player is gounded? {Grounded}");
            // for temporary debug purpose
            float randomWaitTime = Random.Range(1.0f, 2.0f);
            yield return new WaitForSeconds(20f);
            SwitchState(typeof(DefendingState));
        }

        IEnumerator PlayAnimationHelper(string name, bool onOff, float waitTime = 2.0f) {
            _animator.SetBool(name, onOff);
            yield return new WaitForSeconds(Time.deltaTime * waitTime); // pause for one frame
            _animator.SetBool(name, !onOff);
        }
        #endregion

        #region Utilities
        public string GetPlayerId() {
            return _playerId;
        }

        public void SetPlayerId(string playerId) {
            _playerId = playerId;
        }

        public void ResetTimer() {
            SwitchState(typeof(DashingState));
        }

        public int getScore() {
            // method to calculate score from player's status
            return 0;
        }
        #endregion
    }
}