using System.Collections;
using System.IO;
using Helpers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace Fall_Friends.Manager
{
    public class GameManager : Templates.Singleton<GameManager>
    {
        public Vector3 CenterPosition;
        public float PowerCoefficient = 1.0f;
        public GameObject players;

        [SerializeField] private string[] defaultIcons;
        [SerializeField] GameObject wssvGameObject;
        WebSocketServer webSocketServer;
        private int iconCount = 0;
        private float _cameraSwitchTimer;
        private int _currCameraIndex = 0;
        [SerializeField] private float[] _cameraTime;
        [SerializeField] private Camera[] _cameras;


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
                    if (currRequest.action == "addPlayer")
                    {
                        Debug.Log(currRequest.playerId + ": Add player");
                        players.GetComponent<PlayerManager>().addNewPlayer(currRequest.playerId, currRequest.playerIcon);
                        // add code here
                    }
                    else if (currRequest.action == "like")
                    {
                        Debug.Log(currRequest.playerId + ": Thumbs up");
                        players.GetComponent<PlayerManager>().resetTimer(currRequest.playerId);
                        // add code here
                    }
                    else if (currRequest.action == "gift")
                    {
                        Debug.Log(currRequest.playerId + ": Send gift");
                        players.GetComponent<PlayerManager>().giftReward(currRequest.playerId, currRequest.giftName);
                        // add code here
                    }
                    else if (currRequest.action == "sendDanmu")
                    {
                        Debug.Log(currRequest.playerId + ": Send dan mu: " + currRequest.danmu);
                        // add code here
                    }
                    else if (currRequest.action == "addBot")
                    {
                        Debug.Log(currRequest.playerId + ": Add bot");
                        // add code here
                    }
                    else if (currRequest.action == "addPlayerSC")
                    {
                        Debug.Log(currRequest.playerId + ": Add bot through SC");
                        players.GetComponent<PlayerManager>().addNewPlayer(currRequest.playerId, defaultIcons[iconCount++ % defaultIcons.Length]);
                    }
                    else if (currRequest.action == "deletePlayer")
                    {
                        Debug.Log(currRequest.playerId + ": Delete Player");
                        players.GetComponent<PlayerManager>().deletePlayer(currRequest.playerId);
                    }
                }
            }

            _cameraSwitchTimer += Time.deltaTime;
            if (_cameraSwitchTimer > _cameraTime[_currCameraIndex])
            {
                _cameras[_currCameraIndex].gameObject.SetActive(false);
                _currCameraIndex = (_currCameraIndex + 1) % _cameras.Length;
                _cameras[_currCameraIndex].gameObject.SetActive(true);
                _cameraSwitchTimer = 0.0f;
            }
        }
    }
}