using System;
using Fall_Friends.Controllers;
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
                other.GetComponent<DemoPlayer>().Status = PlayerStatus.Defensing;
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

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<DemoPlayer>().Status = PlayerStatus.Idle;
            }
        }
    }
}