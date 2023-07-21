using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;
using System.Collections.Concurrent; // Added for ConcurrentQueue
using System.Collections; // Added for IEnumerator

public class WebSocketServer : MonoBehaviour
{
    WebSocket ws;

    [System.Serializable]
    public class Request
    {
        public string action;
        public string playerId;
        public string danmu;
        public string client;
    }

    [System.Serializable]
    public class RequestCheck
    {
        public string action = null;
        public string playerId = null;
        public string danmu = null;
        public string client = null;
    }

    public PlayerManager playerManager;
    public ConcurrentQueue<Request> myRequestQueue = new ConcurrentQueue<Request>(); // Replaced Queue with ConcurrentQueue


//     // The Coroutine FetchPlayerIDs goes here:
//     IEnumerator FetchPlayerIDs() {
//     while (true) {
//         if (ws != null && ws.IsAlive) {
//             ws.Send(JsonUtility.ToJson(new Request { action = "getPlayers" }));
//         }
//         yield return new WaitForSeconds(60f); // Fetch every 60 seconds
//     }
// }


    void Start()
    {
        //ws = new WebSocket("ws://54.159.171.208:8080"); // EC2 Public IP
        //ws = new WebSocket("ws://localhost:8080"); //local testing
        ws = new WebSocket("wss://symposiastudio.com/streamer-console");

        // Start the coroutine
        //StartCoroutine(FetchPlayerIDs());

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket connected successfully.");
            ws.Send(JsonUtility.ToJson(new Request { action = "identify", client = "Unity" }));
            ws.Send(JsonUtility.ToJson(new Request { action = "getPlayers" }));
        };

        ws.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket error occurred: " + e.Message);
        };

        

        // New: handle binary data, woc the web socket sharp receives binary data, woc i debugged for two days!!
        ws.OnMessage += (sender, e) =>
        {
        //Debug.Log("OnMessage event triggered.");

        string message = null;
        if (e.IsBinary)
        {
            Debug.Log("Received binary data.");
            message = System.Text.Encoding.UTF8.GetString(e.RawData);
            Debug.Log("Raw data: " + message);
        }
        else if (!string.IsNullOrEmpty(e.Data))
        {
            message = e.Data;
        }
        else
        {
            Debug.Log("Received empty data.");
            return;
        }

        Debug.Log("Raw Message Received from " + ((WebSocket)sender).Url + ", Data : " + message);

        try
        {
            RequestCheck requestCheck = JsonUtility.FromJson<RequestCheck>(message);

            if (requestCheck == null || requestCheck.action == null)
            {
                Debug.Log("Raw data: " + message);
                Debug.LogError("The incoming data is invalid or does not contain all the required keys.");
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse the incoming data into a RequestCheck object: " + ex.Message);
            Debug.LogError("Incoming data was: " + message);
            return;
        }

        try
        {
            Request newRequest = JsonUtility.FromJson<Request>(message);
            //Debug.Log("Parsed Request: " + newRequest.action + ", " + newRequest.playerId);

            if (newRequest != null)
            {
                // Check if the action is 'getPlayers'
                if (newRequest.action == "getPlayers")
                {
                // might want to add handling for the list of players here
                // For example, print the list of players to the console:
                Debug.Log("List of players received: " + newRequest.playerId);
                }

                // Check if a player has joined
                if (newRequest.action == "playerJoined") {
                Debug.Log("Player: " + newRequest.playerId + " joined");
                }

                // Check if a player has left
                if (newRequest.action == "playerLeft") {
                Debug.Log("Player: " + newRequest.playerId + " left");
                }

                // Check if a player has liked
                if (newRequest.action == "like") {
                Debug.Log("Player: " + newRequest.playerId + " liked");
                }

                // Check if a player has sent a danmu
                if (newRequest.action == "sendDanmu") {
                Debug.Log("Player: " + newRequest.playerId + " sent danmu: " + newRequest.danmu);
                }

                // Check if a player has sent a gift
                if (newRequest.action == "gift") {
                Debug.Log("Player: " + newRequest.playerId + " sent a gift");
                }

                myRequestQueue.Enqueue(newRequest);
            }
            else
            {
                Debug.LogError("Failed to enqueue Request because it is null");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse the incoming data into a Request object: " + ex.Message);
            Debug.LogError("Failed Data: " + message);
        }
        };



        ws.ConnectAsync();

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket connection closed: " + e.Reason);
        };
    }

    void OnDestroy()
    {
        //StopCoroutine(FetchPlayerIDs());
        ws.Close();
    }
}



//STREAMER CONSOLE IN LOCAL VERSION
// using UnityEngine;
// using WebSocketSharp.Server;
// using System.Collections.Generic;

// public class WebSocketServer : MonoBehaviour
// {
//     WebSocketSharp.Server.WebSocketServer wssv;
//     public PlayerManager playerManager;
    
//     [System.Serializable]
//     public class Request
//     {
//         public string action;
//         public string playerId;
//         public string danmu;
//     }
    
//     public Queue<Request> myRequestQueue = new Queue<Request>();

//     void Start()
//     {
//         wssv = new WebSocketSharp.Server.WebSocketServer("ws://54.159.166.93:8080");
//         wssv.AddWebSocketService<WsServer>("/testGame");
//         wssv.Start();
//         Debug.Log("WebSocket Server started on ws://54.159.166.93:8080/testGame");

//         // Subscribe to the event
//         WsServer.OnDataReceived += OnDataReceived;
//         WsServer.OnGetPlayersRequest += OnGetPlayersRequest;
//     }

//     private void OnApplicationQuit()
//     {
//         wssv.Stop();

//         // Unsubscribe from the event
//         WsServer.OnDataReceived -= OnDataReceived;
//         WsServer.OnGetPlayersRequest -= OnGetPlayersRequest;
//     }

//     // The method that's called when the event is fired
//     private void OnDataReceived(string data)
//     {
//         Debug.Log("Received request in MonoBehaviour: " + data);
//         Request newRequest = JsonUtility.FromJson<Request>(data);
//         myRequestQueue.Enqueue(newRequest);
//     }
    
//     private void OnGetPlayersRequest(WsServer.GetPlayersArgs args)
//     {
//         args.json = playerManager.getRankingJson();
//     }
// }