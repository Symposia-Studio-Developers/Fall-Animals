using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class JSONReader : MonoBehaviour
{
    //public TextAsset textJSON;
    
    [System.Serializable]
    public class Request
    {
        public string action;
        public string playerId;
        public string danmu;
    }
    
    public Queue<Request> myRequestQueue = new Queue<Request>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DirectoryInfo requestDir = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\Assets\Requests");
        var files = requestDir.GetFiles("*.txt");
        foreach (var file in files) {
            var sr = new StreamReader(file.ToString());
            var fileContents = sr.ReadToEnd();
            Request newRequest = JsonUtility.FromJson<Request>(fileContents);
            myRequestQueue.Enqueue(newRequest);
            sr.Close();
            file.Delete();
            AssetDatabase.Refresh();
        }
    }
}