using Fall_Friends.Templates;
using UnityEngine;

namespace Fall_Friends.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        public Vector3 CenterPosition;
        public float PowerCoefficient = 1.0f;
        public GameObject players;
        
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
                    players.GetComponent<PlayerManager>().addNewPlayer(currRequest.playerId);
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
    }
}