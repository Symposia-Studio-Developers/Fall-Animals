﻿using System;
using System.Collections;
using System.IO;
using Fall_Friends.Controllers;
using Helpers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

        [SerializeField] private int totalIconCount;
        [SerializeField] GameObject wssvGameObject;
        WebSocketServer webSocketServer;
        private int iconCount = 1;
        private float _cameraSwitchTimer;
        private int _currCameraIndex = 0;
        [SerializeField] private float[] _cameraTime;
        [SerializeField] private Camera[] _cameras;
        [SerializeField] private float _timePerRound;
        [SerializeField] private TextMeshProUGUI _remainingTime;
        [SerializeField] private Slider _visualTimer;
        [SerializeField] private GameObject _gameEndPanel;
        [SerializeField] private TextMeshProUGUI[] _names;
        [SerializeField] private RawImage[] _icons;
        [SerializeField] private string[] _initBotNames = new[] {"Cheese", "TT", "Angela", "LittleFatty", "Kevin"};
        private float _timer;
        private bool _timerOn = true;
        
        [Tooltip("Value should between 0 - 1, 0 means no bot will generate; 1 means generate a bot for every enter live room watcher.")]
        public float BotGenerationChance = 0.05f;


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
            StartCoroutine(LoadIcon(NewLeader.GetPlayerId(), LeaderIcon));
            LeaderLatestMessage.text = "";
        }

        private IEnumerator LoadIcon(string playerID, RawImage img)
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
                img.texture = texture;
            }
        }

        private void Start()
        {
            InitBots();
        }

        private void InitBots()
        {

            PlayerManager pm = players.GetComponent<PlayerManager>();

            for (int i = 0; i < _initBotNames.Length; i++)
            {
                pm.addNewPlayer(_initBotNames[i], Path.Join(Application.streamingAssetsPath, (iconCount++ % totalIconCount).ToString() + ".webp"), true);
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
                        players.GetComponent<PlayerManager>().addNewPlayer(currRequest.playerId, Path.Join(Application.streamingAssetsPath, (iconCount++ % totalIconCount).ToString() + ".webp"));
                    }
                    else if (currRequest.action == "deletePlayer")
                    {
                        Debug.Log(currRequest.playerId + ": Delete Player");
                        players.GetComponent<PlayerManager>().deletePlayer(currRequest.playerId);
                    }
                    else if (currRequest.action == "audienceJoinRoom")
                    {
                        if (Random.value < BotGenerationChance)
                        {
                            Debug.Log($"One bot is initializing by {currRequest.playerId}'s enter!");
                            PlayerManager pm = players.GetComponent<PlayerManager>();
                            pm.addNewPlayer(GenerateRandomName(), Path.Join(Application.streamingAssetsPath, (iconCount++ % totalIconCount).ToString() + ".webp"), true);
                        }
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

            if (_timerOn)
            {
                _timer += Time.deltaTime;
            }
            if (_timer > _timePerRound && _timerOn)
            {
                // Add Round End Logic here.
                StartCoroutine(GameEnd());
            }
            else if (_timerOn)
            {
                _remainingTime.text = (300 - _timer).ToString("F0");
                _visualTimer.value = (300 - _timer) / 300;
            }
        }

        private string GenerateRandomName()
        {
            const string glyph = "abcdefghijklmnopqrstuvwxyz0123456789";
            int length = Random.Range(1, 7);
            string result = "";
            for (int i = 0; i < length; i++)
            {
                result += glyph[Random.Range(0, 36)];
            }
            return result;
        }

        private IEnumerator GameEnd()
        {
            Debug.Log("Game end.");
            _timerOn = false;
            PlayerManager manager = players.GetComponent<PlayerManager>();
            manager.rankPlayers();
            for (int i = 0; i < 3; i++)
            {
                _names[i].text = "";
                _icons[i].texture = null;
                if (i >= manager.playerDatas.Count)
                {
                    break;
                }
                else
                {
                    _names[i].text = manager.playerDatas[i].GetPlayerId();
                    StartCoroutine(LoadIcon(manager.playerDatas[i].GetPlayerId(), _icons[i]));
                }
            }

            yield return new WaitForSeconds(3f);
            _gameEndPanel.SetActive(true);

            yield return new WaitForSeconds(15f);

            Restart();

            _gameEndPanel.SetActive(false);
        }

        private void Restart()
        {
            players.GetComponent<PlayerManager>().Restart();
            _timer = 0.0f;
            CurrentLeader = null;
            LeaderLatestMessage.text = "";
            _timerOn = true;
            InitBots();
        }
    }
}