using System.Collections;
using Fall_Friends.Controllers;
using UnityEngine;

namespace Fall_Friends.Colliders
{
    public class Stopper : MonoBehaviour
    {

        private void OnCollisionEnter(Collision other) 
        {
            if (other.gameObject.CompareTag("Player")) 
            {
                Debug.Log($"Switching {other.gameObject.name}'s state");
                // StartCoroutine(SwitchStateCoroutine(other));
                other.gameObject.GetComponent<DemoPlayer>().SwitchState(typeof(DefendingState));
            }
        }

        private void OnCollisionStay(Collision other) 
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
                rb.mass = rb.mass * 1.0001f;
                var trans = rb.transform;
                trans.localScale = trans.localScale * 1.0001f;
            }
        }

        private void OnCollisionExit(Collision other) 
        {
            if (other.gameObject.CompareTag("Player"))
            {
                DemoPlayer dp = other.gameObject.GetComponent<DemoPlayer>();
                dp.SwitchState(typeof(IdleState));
            }
        }

        IEnumerator SwitchStateCoroutine(Collision other) 
        {
            yield return new WaitForSeconds(1.5f); // let the player move forward a little bit more
            other.gameObject.GetComponent<DemoPlayer>().SwitchState(typeof(DefendingState));
        }
    }
}