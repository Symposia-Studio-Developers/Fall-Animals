using System;
using Fall_Friends.Controllers;
using UnityEngine;

namespace Fall_Friends.Colliders
{
    public class Stopper : MonoBehaviour
    {
        private bool _hasPlayerStanding = false;
        [SerializeField] private GameObject _crownPrefab;

        private void Update() {
            
        }

        private void OnCollisionEnter(Collision other) {
            if (other.gameObject.CompareTag("Player")) {
                other.gameObject.GetComponent<DemoPlayer>().Status = PlayerStatus.Defensing;
                _hasPlayerStanding = true;
            }
        }

        private void OnCollisionStay(Collision other) {
            if (other.gameObject.CompareTag("Player"))
            {
                Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
                rb.mass = rb.mass * 1.0001f;
                var trans = rb.transform;
                trans.localScale = trans.localScale * 1.0001f;
            }
        }

        private void OnCollisionExit(Collision other) {
            if (other.gameObject.CompareTag("Player"))
            {
                DemoPlayer dp = other.gameObject.GetComponent<DemoPlayer>();
                dp.Status = PlayerStatus.Idle;
                _hasPlayerStanding = false;
            }
        }
    }
}