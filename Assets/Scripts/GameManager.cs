using Fall_Friends.Templates;
using UnityEngine;

namespace Fall_Friends.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        public GameObject PlayerPrefab;

        public Vector3 CenterPosition;
        public float PowerCoefficient = 1.0f;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                AddNewPlayer();
            }
        }

        public void AddNewPlayer()
        {
            float xPosition = Random.Range(-9.0f, 9.0f);
            float zPosition = Random.Range(-9.0f, 9.0f);
            float yPosition = Random.Range(1.0f, 3.0f);
            Vector3 vec3 = new Vector3(xPosition, yPosition, zPosition);
            Debug.Log(vec3);
            Instantiate(PlayerPrefab, vec3, Quaternion.identity);
        }

    }
}