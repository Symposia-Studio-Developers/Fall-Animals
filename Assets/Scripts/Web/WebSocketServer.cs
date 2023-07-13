using UnityEngine;
using WebSocketSharp.Server;

public class WebSocketServer : MonoBehaviour
{
    WebSocketSharp.Server.WebSocketServer wssv;

    void Start()
    {
        wssv = new WebSocketSharp.Server.WebSocketServer("ws://localhost:8080");
        wssv.AddWebSocketService<WsServer>("/testGame");
        wssv.Start();
        Debug.Log("WebSocket Server started on ws://localhost:8080/testGame");
    }

    private void OnApplicationQuit()
    {
        wssv.Stop();
    }
}