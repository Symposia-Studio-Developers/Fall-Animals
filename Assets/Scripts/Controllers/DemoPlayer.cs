using System;
using UnityEngine;
using Fall_Friends.States;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;
using Fall_Friends.Manager;

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
        public float glowEffectDuration = 10f;
        
        [Header("Skin")]
        public Material[] SkinColors;
        public Material[] SkinGlows;

        #region Variables for Bot
        [Header("Bot")]
        public bool IsBot = false;
        public float IdleToDashCoolDown = 3.0f;
        
        private float _idleToDashTimer = 0.0f;

        #endregion

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
        public bool OnMiddleGround { get; private set; }
        private float _likeElapsedTimer = 0.0f;
        private bool _isGlowing = false;
        private float _glowTimer = 0.0f;

        #endregion


        #region Unity Functions

        protected override void Start()
        {
            currentState = new IdleState(this, MaxIdleSpeed, IdleToDashCoolDown);

            availableStates = new Dictionary<Type, BaseState>()
            {
                {typeof(IdleState), currentState},
                {typeof(DashingState), new DashingState(this, PullCoolDown, PushPullRadius, MaxActiveSpeed)},
                {
                    typeof(DefendingState),
                    new DefendingState(this, PushCoolDown, PushForce, PushPullRadius, MaxActiveSpeed)
                },
                {typeof(FrozenState), new FrozenState(this, MaxFreezeTime)}
            };

            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody>();

            if (IsBot)
            {
                MaxLikeDuration = 120f;
                MaxFreezeTime = 10f;
            }

            if (IsActive)
                SwitchState(typeof(DashingState));
        }

        protected override void Update()
        {
            _likeElapsedTimer += Time.deltaTime;
            if (_likeElapsedTimer > MaxLikeDuration)
            {
                IsActive = false;
                SwitchState(typeof(IdleState));
            }

            if (_isGlowing)
            {
                _glowTimer += Time.deltaTime;
                if (_glowTimer > glowEffectDuration)
                    StopGlowEffect();
            }

            transform.eulerAngles = Vector3.Scale(transform.eulerAngles, Vector3.up);

            base.Update();
        }

        protected override void FixedUpdate()
        {
            if (_rb == null)
                Debug.LogError("Body is null");

            GroundCheck();

            FallingCheck();

            MiddleGroundCheck();

            base.FixedUpdate();
        }

        #region Collision Functions

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("DeathZone"))
            {
                GameManager.Instance.players.GetComponent<PlayerManager>().deletePlayer(_playerId);
            }
        }

        private void OnCollisionStay(Collision other)
        {
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

        public DemoPlayer GetNearestPlayer(float pushRadius)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, pushRadius);
            if (colliders.Length == 0) return null; // no nearby players

            DemoPlayer nearest = null; // otherwise, find the nearest
            float nearDist = float.PositiveInfinity;
            foreach (var collider in colliders)
            {
                DemoPlayer otherPlayer = collider.GetComponent<DemoPlayer>();
                if (otherPlayer != null && otherPlayer != this)
                {
                    Vector3 offset = transform.position - otherPlayer.transform.position;
                    float thisDist = offset.sqrMagnitude;
                    if (thisDist < nearDist)
                    {
                        nearDist = thisDist;
                        nearest = otherPlayer;
                    }
                }
            }

            return nearest;
        }

        public float GetTimerPartition()
        {
            return (MaxLikeDuration - _likeElapsedTimer) / MaxLikeDuration;
        }

        public DemoPlayer GetNearestPlayerFacingFront(float pullRadius)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, pullRadius);
            if (colliders.Length == 0) return null;

            Vector3 mgrPos = GameManager.Instance.transform.position;
            float playerManagerDistance = (transform.position - mgrPos).sqrMagnitude;

            DemoPlayer nearest = null;
            float nearDist = float.PositiveInfinity;
            foreach (var collider in colliders)
            {
                DemoPlayer otherPlayer = collider.GetComponent<DemoPlayer>();
                if (otherPlayer != null && otherPlayer != this)
                {
                    float otherPlayerManagerDistance = (otherPlayer.transform.position - mgrPos).sqrMagnitude;
                    if (otherPlayerManagerDistance < playerManagerDistance)
                    {
                        Vector3 offset = transform.position - otherPlayer.transform.position;
                        float thisDist = offset.sqrMagnitude;
                        if (thisDist < nearDist)
                        {
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

        IEnumerator PlayPulledAnimationHelper()
        {
            _animator.Play("PulledBack");
            _animator.SetBool("Pulled", true);
            yield return 0;
            _animator.SetBool("Pulled", false);
        }

        IEnumerator PlayFallingAnimation()
        {
            if (_isPlayingFallingAnimation) yield break;
            _isPlayingFallingAnimation = true;

            _animator.Play("StartFalling"); // NECESSARY: force the animation to start playing falling animation
            _animator.SetBool("Falling", true);
            yield return 0;
            _animator.SetBool("Falling", false);
        }

        IEnumerator PlayAnimationHelper(string name, bool onOff)
        {
            _animator.SetBool(name, onOff);
            yield return 0;
            _animator.SetBool(name, !onOff);
        }

        #endregion

        #region Helper Functions

        private void MiddleGroundCheck()
        {
            if (!Grounded) return;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit))
            {
                // Debug.Log($"Found an object {hit.collider.name}, with distanc {hit.distance}");
                Debug.DrawRay(transform.position, -Vector3.up * 100, Color.green);
                if (hit.collider.CompareTag("MiddleGround"))
                    OnMiddleGround = true;
                else OnMiddleGround = false;
            }
        }

        private void GroundCheck()
        {
            // check whether the player is on the ground
            bool wasGrounded = Grounded;
            Grounded = false;
            Collider[] colliders = Physics.OverlapSphere(_groundCheck.position, _groundCheckRadius, _whatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    Grounded = true;
                    if (!wasGrounded)
                    {
                        _animator.SetBool("Grounded", true);
                    }
                }
            }
        }

        private void FallingCheck()
        {
            if (_rb.velocity.y < -0.1 && !Grounded)
            {
                Falling = true;
                StartCoroutine(PlayFallingAnimation());
            }
            else
            {
                Falling = false;
                _isPlayingFallingAnimation = false;
            }
        }

        #endregion

        #region Utilities

        public string GetPlayerId()
        {
            return _playerId;
        }

        public void SetPlayerId(string playerId)
        {
            _playerId = playerId;
        }

        public void ResetTimer()
        {
            _likeElapsedTimer = 0f;
            IsActive = true;
            if (CurrentState == "IdleState")
            {
                SwitchState(typeof(DashingState));
            }
        }

        public int GetSkinColorIndex()
        {
            return SkinColorIndex;
        }

        public void SetSkinColorIndex(int skinColorIndex)
        {
            SkinColorIndex = skinColorIndex;
            Material skinMaterial = gameObject.transform.Find("pCylinder1").gameObject
                .GetComponent<SkinnedMeshRenderer>().material;
            if (_isGlowing)
                skinMaterial = SkinGlows[skinColorIndex];
            else
                skinMaterial = SkinColors[skinColorIndex];
        }

        public void StartGlowEffect()
        {
            gameObject.transform.Find("pCylinder1").gameObject.GetComponent<SkinnedMeshRenderer>().material =
                SkinGlows[SkinColorIndex];
            _isGlowing = true;
            _glowTimer = 0f;
        }

        public void StopGlowEffect()
        {
            gameObject.transform.Find("pCylinder1").gameObject.GetComponent<SkinnedMeshRenderer>().material =
                SkinColors[SkinColorIndex];
            _isGlowing = false;
        }

        public void GrowSize(float multiple)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.mass = rb.mass * multiple;
            var trans = rb.transform;
            trans.localScale = trans.localScale * multiple;
        }

        public float getScore()
        {
            // method to calculate score from player's status
            return GetComponent<Rigidbody>().mass;
        }

        #endregion
    }
}