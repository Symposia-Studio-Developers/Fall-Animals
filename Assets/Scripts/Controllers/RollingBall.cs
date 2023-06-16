using UnityEngine;

namespace Fall_Friends.Controllers
{
    public class RollingBall : MonoBehaviour
    {
        public float DegreePerSecond = 20.0f;
        private void Update()
        {
            transform.RotateAround(transform.parent.position, Vector3.up, Time.deltaTime * DegreePerSecond);
        }
    }
}