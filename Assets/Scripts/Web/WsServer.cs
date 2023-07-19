// STREAMER CONSOLE IN LOCAL VERSION
// using UnityEngine;
// using WebSocketSharp;
// using WebSocketSharp.Server;
// using System;
// using System.Collections.Generic;
// using System.Linq;

// using Fall_Friends.Controllers;

// public class WsServer : WebSocketBehavior
// {
//     public class GetPlayersArgs: EventArgs
//     {
//         public string json;
//     }
    
//     public static event Action<string> OnDataReceived = delegate { };
//     public static event Action<GetPlayersArgs> OnGetPlayersRequest = delegate { };

//     private int requestID = 0;
    
//     protected override void OnMessage(MessageEventArgs e)
    
//     {
//         // this method is called when a message is received
//         //Debug.Log("Message Received: " + e.Data);

//         // Handle received data based on the game logic
//         if (e.Data == "getPlayers")
//         {
//             Debug.Log("getPlayers");
//             var args = new GetPlayersArgs {};
//             OnGetPlayersRequest?.Invoke(args);
            
//             // Send the player ranking back
//             Debug.Log(args.json);
//             //Send(args.json);
//         }
//         else {
//             OnDataReceived?.Invoke(e.Data);
//         }
//         // Can add more commands here to handle other requests
//     }

//     // Need to implement this method to return the list of players
//     private List<DemoPlayer> GetPlayers()
//     {
//         // Placeholder. Replace with the actual method to get player list
//         return new List<DemoPlayer>();
//     }
// }