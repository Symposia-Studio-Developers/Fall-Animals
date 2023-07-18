using System;
using Fall_Friends.Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fall_Friends.Controllers
{
    public enum PlayerStatus
    {
        Idle = 0,
        DashingToCenter = 1,
        Defensing = 2,
        Freeze = 3,
    }

    public class DemoPlayer : MonoBehaviour
    {
        [Header("Push and Pull")]
        public float PushRadius = 1.5f;
        public float PushForce = 20f;
        public float ThrowArcHeight = 2f;
        public float ThrowForce = 2f;

        [Header("Move Speed")]
        public float MaxIdleSpeed = 3f;
        public float MaxActiveSpeed = 5f;

        [Header("Timer")]
        public float MaxLikeDuration = 10f;
        public float MaxFreezeTime = 3f;
        public float PushCoolDown = 2f;
        public float PullCoolDown = 2f;

        [Header("Debug")]
        public bool AlwaysActive = false; // testing purpose
        public Material WithCrownMat;
        [SerializeField] private string _playerId;// unique playerId parsed from tiktok
        //note, exposing playerId as a public variable now to set it in Unity's inspector for testing and initial setup, 
        //encapsulate it and provide methods to access and modify it later in production for safety. make playerId private and provide a public getter and setter.

        public int getScore()
        {
            // method to calculate score from player's status
            return 0;
        }

        [SerializeField] private GameObject _crown;

        public PlayerStatus Status
        {
            get => _status;
            set
            {
                if (value == PlayerStatus.Defensing) {
                    // GetComponent<Renderer>().material = WithCrownMat; // TODO: DEBUG PURPOSE
                } else {
                    // GetComponent<Renderer>().material = originalMat; // TODO: DEBUG PURPOSE
                    if (value == PlayerStatus.Idle) {
                    _speed = MaxIdleSpeed;
                    } else if (value == PlayerStatus.DashingToCenter) {
                        _speed = MaxActiveSpeed;
                    }
                }
                
                _status = value;
            }
        }
        
        #region Timer Variables
        private float _pushTimer = 0.0f;
        private float _pullTimer = 0.0f;
        private float _freezeTimer = 0.0f;
        private float _likeTimer = 0.0f;
        #endregion

        #region Unity Components Variables
        private Rigidbody _body;
        #endregion

        private PlayerStatus _status;
        
        #region Move Variables
        private float _speed;
        private Vector3 _moveDir;
        #endregion

        
        // for debug purpose
        private Vector3 collision = Vector3.zero; 
        private Vector3 pushedObject = Vector3.zero;
        private Material originalMat;

        #region Unity Functions
        private void Start() {
            _body = GetComponent<Rigidbody>();
            Status = PlayerStatus.Idle;
            // originalMat = GetComponent<Renderer>().material;
        }

        private void Update() {
            // Debug.Log(gameObject.name + ": " + Status + "; " + transform.parent);

            if (!AlwaysActive && Status != PlayerStatus.Idle) {
                _likeTimer += Time.deltaTime;
                if (_likeTimer > MaxLikeDuration) {
                    Status = PlayerStatus.Idle;
                }
            }

            if (Status == PlayerStatus.Freeze) {
                _freezeTimer += Time.deltaTime;
                if (_freezeTimer >= MaxFreezeTime) {
                    _freezeTimer = 0.0f;
                    Status = PlayerStatus.Idle;
                }
                _moveDir = Vector3.zero;
            } else {
                if (Status == PlayerStatus.Defensing) {
                    if (_pushTimer < PushCoolDown) {
                        _pushTimer += Time.deltaTime;
                    } else {
                        _pushTimer = 0.0f;
                        PushOtherPlayer();
                    }
                } else if (Status == PlayerStatus.DashingToCenter) {
                    if (_pullTimer < PullCoolDown) {
                        _pullTimer += Time.deltaTime;
                    } else {
                        _pullTimer = 0.0f;
                        ThrowOtherPlayer();
                    }
                }

                Vector3 _center_position = GameManager.Instance.CenterPosition;
                _moveDir = (_center_position - new Vector3(transform.position.x, _center_position.y, transform.position.z));
                _moveDir = Vector3.ClampMagnitude(_moveDir, 1);
            } 
        }

        private void FixedUpdate() {
            transform.position += _moveDir * _speed * Time.fixedDeltaTime;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, PushRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(collision, 1.0f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pushedObject, 1.0f);
        }
        #endregion

        #region Push and Pull
        private void PushOtherPlayer () {
            Collider[] colliders = Physics.OverlapSphere(transform.position, PushRadius);
            DemoPlayer nearest = null;
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
            
            if (nearest != null) {
                pushedObject = nearest.transform.position;
                Debug.Log("NEARBY PLAYER NAME: " + nearest.gameObject.name);
                
                Vector3 forceDirection = nearest.transform.position - transform.position;
                nearest.GetComponent<Rigidbody>().AddForce(forceDirection.normalized * PushForce, ForceMode.Impulse);

                nearest.Status = PlayerStatus.Freeze;
            } else {
                Debug.Log("Dancing~!!");
            }
        }

        private void ThrowOtherPlayer () {
            DemoPlayer otherPlayer = GetClosestPlayer(); 

            if (otherPlayer != null) {
                Vector3 throwDir = transform.position - otherPlayer.transform.position;
                Vector3 throwForceVec = throwDir.normalized + Vector3.up*4;
                Rigidbody otherPlayerRB = otherPlayer.GetComponent<Rigidbody>();
                otherPlayerRB.AddForce(throwForceVec * 5,  ForceMode.Impulse);
                otherPlayer.Status = PlayerStatus.Freeze;
            }
        }

        private DemoPlayer GetClosestPlayer() {
            // Find the direction of the ray
            Vector3 centerPosition = GameManager.Instance.CenterPosition;
            Vector3 direction = centerPosition - transform.position;
            direction.y = transform.position.y; 
            direction = direction.normalized;

            var ray = new Ray(transform.position, direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 3)) { // can use layermask
                GameObject hitObject = hit.transform.gameObject;
                DemoPlayer otherPlayer = hitObject.GetComponent<DemoPlayer>();
                if (hitObject.CompareTag("Player") && otherPlayer != this) { 
                    Debug.Log("Pulling Another Player");
                    collision = hit.point;
                    Debug.DrawLine(this.transform.position, hit.point, Color.red);
                    return otherPlayer;
                }
            }
            return null;
        }

        #endregion

        #region Utilities
        public string GetPlayerId() {
            return _playerId;
        }

        public void SetPlayerId(string playerId) {
            _playerId = playerId;
        }

        public void ResetTimer () {
            _likeTimer = 0.0f;
            Status = PlayerStatus.DashingToCenter;
        }
        #endregion
    }
}