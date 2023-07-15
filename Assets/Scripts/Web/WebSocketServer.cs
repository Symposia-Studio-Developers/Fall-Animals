using UnityEngine;
using WebSocketSharp.Server;

public class WebSocketServer : MonoBehaviour
{
    WebSocketSharp.Server.WebSocketServer wssv;
    public GameObject playerPrefab; 

    void Start()
    {
        wssv = new WebSocketSharp.Server.WebSocketServer("ws://localhost:8080");
        wssv.AddWebSocketService<WsServer>("/testGame");
        wssv.Start();
        Debug.Log("WebSocket Server started on ws://localhost:8080/testGame");

        // Subscribe to the event
        WsServer.OnDataReceived += OnDataReceived;
    }

    private void OnApplicationQuit()
    {
        wssv.Stop();

        // Unsubscribe from the event
        WsServer.OnDataReceived -= OnDataReceived;
    }

    // The method that's called when the event is fired
    private void OnDataReceived(string data)
    {
        Debug.Log("Received data in MonoBehaviour: " + data);
        
        // Handle the received data to spawn game objects
        if (data == "getPlayers")
    {
        // Instantiate a new Player object at position (0, 0, 0) with no rotation
        GameObject newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        // can now do whatever with the newPlayer object
        // For example, add it to a list of player objects, etc.
    }
    }
}