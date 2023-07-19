using System;
using Fall_Friends.Controllers;
using UnityEngine;

namespace Fall_Friends.Colliders
{
    public class Crown : MonoBehaviour
    {
        private Vector3 _initialPosition;

        public void Start() {
            _initialPosition = transform.position;
        }

        public void ResetToOriginalPosition() {
            transform.position = _initialPosition;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.CompareTag("Player")) {
                
            }
        }

    }
}