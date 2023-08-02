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
        [SerializeField] private LayerMask _whatIsGround;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.3f;

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
        
        [Header("Skin")]
        public Material[] SkinColors;

        [Header("Debug")]
        [SerializeField] private string _playerId;// unique playerId parsed from tiktok
        //note, exposing playerId as a public variable now to set it in Unity's inspector for testing and initial setup, 
        //encapsulate it and provide methods to access and modify it later in production for safety. make playerId private and provide a public getter and setter.
        public bool IsActive = false;
        #endregion

        #region Variables
        private Animator _animator;
        private Rigidbody _rb;
        private int SkinColorIndex;

        private bool _isPlayingFallingAnimation = false;
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

        protected override void FixedUpdate()
        {
            if (_rb == null) 
                Debug.LogError("Body is null");
            

            base.FixedUpdate();

            GroundCheck();

            FallingCheck();
        }

        #region Collision Functions
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
            if (other.gameObject.CompareTag("Ring")) 
            {
                transform.SetParent(null);
            }
        }
        #endregion

        #endregion

        #region Push and Pull

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
            StartCoroutine(PlayPulledAnimationHelper());
        }

        IEnumerator PlayPulledAnimationHelper() {
            _animator.Play("PulledBack");
            _animator.SetBool("Pulled", true);
            yield return 0;
            _animator.SetBool("Pulled", false);
        }
        #endregion

        #region Helper Functions

        private void GroundCheck() {
            // check whether the player is on the ground
            bool wasGrounded = Grounded;
		    Grounded = false;
            Collider[] colliders = Physics.OverlapSphere(_groundCheck.position, _groundCheckRadius, _whatIsGround);
            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i].gameObject != gameObject) {
                    Grounded = true;
                    if (!wasGrounded) {
                        _animator.SetBool("Grounded", true);
                    }
                }
            }

            if (!Grounded) {
                Debug.Log("The player is not grounded");
            }
        }

        private void FallingCheck() {
            if (_rb.velocity.y < -0.1) {
                Falling = true;
                StartCoroutine(PlayFallingAnimation());
            }
            else {
                Falling = false;
                _isPlayingFallingAnimation = false;
            }
        }

        IEnumerator PlayFallingAnimation() {
            if (_isPlayingFallingAnimation) yield break;
            _isPlayingFallingAnimation = true;

            _animator.Play("StartFalling"); // NECESSARY: force the animation to start playing falling animation
            _animator.SetBool("Falling", true);
            yield return 0;
            _animator.SetBool("Falling", false);
        }

        IEnumerator PlayAnimationHelper(string name, bool onOff) {
            _animator.SetBool(name, onOff);
            yield return 0;
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
        
        public int GetSkinColorIndex() {
            return SkinColorIndex;
        }
        
        public void SetSkinColorIndex(int skinColorIndex) {
            SkinColorIndex = skinColorIndex;
            gameObject.transform.Find("pCylinder1").gameObject.GetComponent<SkinnedMeshRenderer>().material = SkinColors[skinColorIndex];
        }

        public int getScore() {
            // method to calculate score from player's status
            return 0;
        }
        #endregion
    }
}