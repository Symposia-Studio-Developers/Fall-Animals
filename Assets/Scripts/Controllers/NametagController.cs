using System;
using System.Collections;
using System.IO;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Fall_Friends.Controllers
{
    public class NametagController : MonoBehaviour
    {
        private Camera _camera;
        private DemoPlayer _demoPlayer;
        [SerializeField] private TextMeshPro _nametag;
        [SerializeField] private RawImage _icon;

        public void ApplyIcon(string URL, string playerID)
        {
            StartCoroutine(DownloadAndReplaceIcon(URL, playerID));
        }

        private void Start()
        {
            _camera = Camera.main;
            _demoPlayer = GetComponentInParent<DemoPlayer>();
        }

        private void Update()
        {
            transform.LookAt(_camera.transform);
            Vector3 rotation = transform.eulerAngles;
            //Vector3 worldPosition = transform.position;
            rotation.x = 0;
            rotation.z = 0;
            rotation.y += 180;
            //worldPosition.y = 6.5f;
            //transform.position = worldPosition;
            transform.eulerAngles = rotation;
            if (_nametag.text == "")
            {
                _nametag.text = _demoPlayer.GetPlayerId();
            }
        }
        
        private IEnumerator DownloadAndReplaceIcon(string URL, string playerID)
        {
            UnityWebRequest www = UnityWebRequest.Get(URL);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError ||
                www.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log("An error occured when downloading user icon: " + www.error);
                // TODO: Add default icon for error.
            }
            else
            {
                //Texture2D loadedTexture = DownloadHandlerTexture.GetContent(www);
                byte[] iconWebpData = www.downloadHandler.data;
                File.WriteAllBytes(Path.Join(Application.persistentDataPath, playerID + ".webp"), iconWebpData);
                // Convert webp to png here
                Webp2Png.ConvertWebp2Png(Path.Join(Application.persistentDataPath, playerID + ".webp"),
                    Path.Join(Application.persistentDataPath, playerID + ".png"));
                // Load Icon and change _icon to sprite
                StartCoroutine(LoadIcon(Path.Join(Application.persistentDataPath, playerID + ".png")));
            }
        }

        private IEnumerator LoadIcon(string URL)
        {
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
                _icon.texture = texture;
            }
        }
    }
}