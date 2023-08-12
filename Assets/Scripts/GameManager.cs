using System;
using System.Collections;
using System.IO;
using Fall_Friends.Controllers;
using Helpers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Fall_Friends.Manager
{
    public class GameManager : Templates.Singleton<GameManager>
    {
        public Vector3 CenterPosition;
        public float PowerCoefficient = 1.0f;
        public GameObject players;

        public Camera CurrentCamera => _cameras[_currCameraIndex];
        public DemoPlayer CurrentLeader = null;
        public TextMeshProUGUI LeaderLatestMessage;
        public RawImage LeaderIcon;

        [SerializeField] private string[] defaultIcons;
        [SerializeField] GameObject wssvGameObject;
        WebSocketServer webSocketServer;
        private int iconCount = 0;
        private float _cameraSwitchTimer;
        private int _currCameraIndex = 0;
        [SerializeField] private float[] _cameraTime;
        [SerializeField] private Camera[] _cameras;
        [SerializeField] private float _timePerRound;
        [SerializeField] private TextMeshProUGUI _remainingTime;
        [SerializeField] private Slider _visualTimer;
        private float _timer;


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

        public void ChangeLeader(DemoPlayer NewLeader)
        {
            CurrentLeader = NewLeader;
            StartCoroutine(LoadIcon(NewLeader.GetPlayerId()));
            LeaderLatestMessage.text = "";
        }

        private IEnumerator LoadIcon(string playerID)
        {
            string URL = Path.Join(Application.persistentDataPath, playerID + ".png");
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError ||
                www.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log("An error occured when loading texture data from persistent data path: " + www.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                LeaderIcon.texture = texture;
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
                        if (currRequest.playerId == CurrentLeader.GetPlayerId())
                        {
                            LeaderLatestMessage.text = currRequest.danmu;
                        }
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

            _timer += Time.deltaTime;
            if (_timer > _timePerRound)
            {
                // Add Round End Logic here.
            }
            else
            {
                _remainingTime.text = (300 - _timer).ToString("F0");
                _visualTimer.value = (300 - _timer) / 300;
            }
        }
    }
}