using Fall_Friends.Templates;
using UnityEngine;

namespace Fall_Friends.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        public GameObject PlayerPrefab;

        public Vector3 CenterPosition;
        public float PowerCoefficient = 1.0f;
        
        [SerializeField] GameObject wssvGameObject;
        WebSocketServer webSocketServer;

        private void Update()
        {
            webSocketServer = wssvGameObject.GetComponent<WebSocketServer>();
            while (webSocketServer.myRequestQueue.Count != 0) //Input.GetKeyDown(KeyCode.A)
            {
                WebSocketServer.Request currRequest = webSocketServer.myRequestQueue.Peek();
                webSocketServer.myRequestQueue.Dequeue();
                if (currRequest.action == "addPlayer") {
                    Debug.Log(currRequest.playerId + ": Add player");
                    AddNewPlayer();
                    // add code here
                    
                }
                else if (currRequest.action == "like") {
                    Debug.Log(currRequest.playerId + ": Thumbs up");
                    // add code here
                    
                }
                else if (currRequest.action == "gift") {
                    Debug.Log(currRequest.playerId + ": Send gift");
                    // add code here
                    
                }
                else if (currRequest.action == "sendDanmu") {
                    Debug.Log(currRequest.playerId + ": Send dan mu: " + currRequest.danmu);
                    // add code here
                    
                }
                else if (currRequest.action == "addBot") {
                    Debug.Log(currRequest.playerId + ": Add bot");
                    // add code here
                    
                }
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