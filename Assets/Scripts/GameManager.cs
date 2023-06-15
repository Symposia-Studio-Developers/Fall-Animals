using UnityEngine;

namespace Fall_Friends.Manager
{
    public class GameManager : MonoBehaviour
    {
        public Rigidbody Player1;
        public Rigidbody Player2;
        public Rigidbody Player3;

        public Vector3 CenterPosition;
        public float PowerCoefficient = 1.0f;

        void Update()
        {
            float distanceFixedCoefficient = 1.0f;
            if (Input.GetKey(KeyCode.F1))
            {
                Vector3 direction = CenterPosition - Player1.transform.position;
                if (direction.magnitude > 0.5f)
                {
                    if (direction.magnitude < 3f)
                    {
                        distanceFixedCoefficient = 3.0f;
                    }
                    else
                    {
                        distanceFixedCoefficient = 1.0f;
                    }
                    // If not inside the center, try to get inside
                    direction = direction.normalized;
                    Player1.AddForce(direction * PowerCoefficient * distanceFixedCoefficient);
                }
            }

            if (Input.GetKey(KeyCode.F2))
            {
                Vector3 direction = CenterPosition - Player2.transform.position;
                if (direction.magnitude > 0.5f)
                {
                    if (direction.magnitude < 3f)
                    {
                        distanceFixedCoefficient = 3.0f;
                    }
                    else
                    {
                        distanceFixedCoefficient = 1.0f;
                    }
                    // If not inside the center, try to get inside
                    direction = direction.normalized;
                    Player2.AddForce(direction * PowerCoefficient * distanceFixedCoefficient);
                }
            }

            if (Input.GetKey(KeyCode.F3))
            {
                Vector3 direction = CenterPosition - Player3.transform.position;
                if (direction.magnitude > 0.5f)
                {
                    if (direction.magnitude < 3f)
                    {
                        distanceFixedCoefficient = 3.0f;
                    }
                    else
                    {
                        distanceFixedCoefficient = 1.0f;
                    }
                    // If not inside the center, try to get inside
                    direction = direction.normalized;
                    Player3.AddForce(direction * PowerCoefficient * distanceFixedCoefficient);
                }
            }
        }
    }
}