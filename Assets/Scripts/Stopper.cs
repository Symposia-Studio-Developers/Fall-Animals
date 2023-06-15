using System;
using UnityEngine;

namespace Fall_Friends.Colliders
{
    public class Stopper : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                rb.mass = rb.mass * 1.0001f;
                var trans = rb.transform;
                trans.localScale = trans.localScale * 1.0001f;
            }
        }
    }
}