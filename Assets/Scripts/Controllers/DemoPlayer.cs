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
    }

    public class DemoPlayer : MonoBehaviour
    {

        public string playerId;// unique playerId parsed from tiktok
        //note, exposing playerId as a public variable now to set it in Unity's inspector for testing and initial setup, 
        //encapsulate it and provide methods to access and modify it later in production for safety. make playerId private and provide a public getter and setter.

        public PlayerStatus Status
        {
            get => _status;
            set
            {
                if (value == PlayerStatus.Idle)
                {
                    _rollingBall.SetActive(false);
                    _idleToDashTimer = 0.0f;
                }
                else if (value == PlayerStatus.DashingToCenter)
                {
                    _idleToDashCooldown = 0.0f + Random.Range(0.0f, 4.0f);
                    _idleToDashTimer = 0.0f;
                }
                else if (value == PlayerStatus.Defensing)
                {
                    _rollingBall.SetActive(true);
                }

                _status = value;
            }
        }

        [SerializeField] private GameObject _rollingBall;

        private PlayerStatus _status;
        private Rigidbody _body;
        private float _idleToDashTimer = 0.0f;
        private float _idleToDashCooldown = 2.0f;

        private void Start()
        {
            _body = GetComponent<Rigidbody>();
            Status = PlayerStatus.DashingToCenter;
        }

        private void Update()
        {
            //Debug.Log(Status);
            if (Status == PlayerStatus.Idle)
            {
                _idleToDashTimer += Time.deltaTime;
                if (_idleToDashTimer > _idleToDashCooldown)
                {
                    Status = PlayerStatus.DashingToCenter;
                }
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (Status != PlayerStatus.Defensing)
                {
                    _body.velocity = -_body.velocity;
                    Status = PlayerStatus.Idle;
                }
            }
        }

        private void FixedUpdate()
        {
            // Give Players Velocities here
            if (Status == PlayerStatus.DashingToCenter)
            {
                Vector3 centerPosition = GameManager.Instance.CenterPosition;
                Vector3 direction = (centerPosition - transform.position).normalized;
                direction.y = 0;
                float originalYVelocity = _body.velocity.y;
                _body.velocity = direction * 5.0f + Vector3.up * originalYVelocity; // Temp Value, supposed to be changed later
            }
        }
    }
}