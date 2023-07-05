using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.Linq;

using Fall_Friends.Controllers;

public class WsServer : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        // this method is called when a message is received
        Debug.Log("Message Received: " + e.Data);

        // Handle received data based on the game logic
        if (e.Data == "getPlayers")
        {
            List<DemoPlayer> players = GetPlayers();  // replace with your actual method to get player list
            List<string> playerIds = players.Select(player => player.playerId).ToList();
            
            // Convert to JSON
            string json = JsonUtility.ToJson(playerIds);

            // Send the player IDs back
            Send(json);
        }
        // Can add more commands here to handle other requests
    }

    // Need to implement this method to return the list of players
    private List<DemoPlayer> GetPlayers()
    {
        // Placeholder. Replace with the actual method to get player list
        return new List<DemoPlayer>();
    }
}
