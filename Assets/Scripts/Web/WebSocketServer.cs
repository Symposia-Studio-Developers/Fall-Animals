using UnityEngine;
using WebSocketSharp.Server;
using System.Collections.Generic;

public class WebSocketServer : MonoBehaviour
{
    WebSocketSharp.Server.WebSocketServer wssv;
    public PlayerManager playerManager;
    
    [System.Serializable]
    public class Request
    {
        public string action;
        public string playerId;
        public string danmu;
    }
    
    public Queue<Request> myRequestQueue = new Queue<Request>();

    void Start()
    {
        wssv = new WebSocketSharp.Server.WebSocketServer("ws://localhost:8080");
        wssv.AddWebSocketService<WsServer>("/testGame");
        wssv.Start();
        Debug.Log("WebSocket Server started on ws://localhost:8080/testGame");

        // Subscribe to the event
        WsServer.OnDataReceived += OnDataReceived;
        WsServer.OnGetPlayersRequest += OnGetPlayersRequest;
    }

    private void OnApplicationQuit()
    {
        wssv.Stop();

        // Unsubscribe from the event
        WsServer.OnDataReceived -= OnDataReceived;
        WsServer.OnGetPlayersRequest -= OnGetPlayersRequest;
    }

    // The method that's called when the event is fired
    private void OnDataReceived(string data)
    {
        Debug.Log("Received request in MonoBehaviour: " + data);
        Request newRequest = JsonUtility.FromJson<Request>(data);
        myRequestQueue.Enqueue(newRequest);
    }
    
    private void OnGetPlayersRequest(WsServer.GetPlayersArgs args)
    {
        args.json = playerManager.getRankingJson();
    }
}