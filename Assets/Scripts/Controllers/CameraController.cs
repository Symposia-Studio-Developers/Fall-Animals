using Fall_Friends.Manager;
using UnityEngine;

namespace Fall_Friends.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _degreePerSecond = 30.0f;
        void Update()
        {
            transform.RotateAround(GameManager.Instance.CenterPosition, Vector3.up, Time.deltaTime * _degreePerSecond);
        }
    }
}