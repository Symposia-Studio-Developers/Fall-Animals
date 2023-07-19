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


        // Call the base class Awake method to ensure the Singleton is set up correctly
        protected override void Awake() 
            {
                base.Awake();
                webSocketServer = wssvGameObject.GetComponent<WebSocketServer>();
                if (webSocketServer == null)
                {
                    Debug.LogError("WebSocketServer component not found on the assigned GameObject.");
                }
            }


        private void Update()
{
    if (webSocketServer == null) return; // Don't run if webSocketServer is null

    while (webSocketServer.myRequestQueue.Count != 0)
    {
        WebSocketServer.Request currRequest;
        bool success = webSocketServer.myRequestQueue.TryDequeue(out currRequest);
        
        if (success) // Only if TryDequeue is successful (i.e., the queue was not empty)
        {
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
}