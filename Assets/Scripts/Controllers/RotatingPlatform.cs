using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fall_Friends.Controllers
{
    public class RotatingPlatform : MonoBehaviour
    {

        [SerializeField] private float _speed = 2.0f;

        // Update is called once per frame
        private void Update()
        {
            transform.Rotate(new Vector3(0, _speed * Time.deltaTime, 0));
        }

        private void OnCollisionEnter(Collision other) {
            if (other.gameObject.CompareTag("Player")) {
                other.gameObject.transform.SetParent(transform);
            }
        }

        private void OnCollisionStay(Collision other) {
            if (other.gameObject.CompareTag("Player")) {
                other.gameObject.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit(Collision other) {
            if (other.gameObject.CompareTag("Player")) {
                other.gameObject.transform.SetParent(null);
            }
        }

    }
}

