using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fall_Friends.Controllers;
using System.Linq;
using Newtonsoft.Json;

public class PlayerManager : MonoBehaviour
{
    public List<DemoPlayer> playerDatas = new List<DemoPlayer>();
    public GameObject PlayerPrefab;
    public GameObject PlayersParent;
    private int skinColorCount = 3; // TODO: change to actual color number later
    private int nextSkinColorIndex = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void addNewPlayer(string playerId, string iconURL, bool isBot = false)
    {
        foreach (DemoPlayer player in playerDatas)
        {
            if (player.GetPlayerId() == playerId)
            {
                return;
            }
        }
        float xPosition = Random.Range(-9.0f, 9.0f);
        float zPosition = Random.Range(-9.0f, 9.0f);
        float yPosition = Random.Range(1.0f, 3.0f);
        Vector3 vec3 = new Vector3(xPosition, yPosition, zPosition);
        GameObject newPlayer = Instantiate(PlayerPrefab, vec3, Quaternion.identity, PlayersParent.transform);
        newPlayer.name = playerId;
        newPlayer.GetComponentInChildren<NametagController>().ApplyIcon(iconURL, playerId);
        DemoPlayer playerData = newPlayer.GetComponent<DemoPlayer>();
        playerData.IsActive = true;
        playerData.SetPlayerId(playerId);
        playerData.SetSkinColorIndex(nextSkinColorIndex);
        playerData.IsBot = isBot;
        nextSkinColorIndex = (nextSkinColorIndex + 1) % skinColorCount;
        playerDatas.Add(playerData);
    }

    public void deletePlayer(string playerId)
    {
        GameObject playerToDestroy = GameObject.Find(playerId);
        if (playerToDestroy){
            Destroy(playerToDestroy.gameObject);
            Debug.Log("player gameobject destroyed");
        }
        for (int i = 0; i < playerDatas.Count; i++) {
            if (playerDatas[i].GetPlayerId() == playerId) playerDatas.RemoveAt(i);
        }
    }
    
    public void resetTimer(string playerId)
    {
        GameObject playerToResetTimer = GameObject.Find(playerId);
        if (playerToResetTimer){
            playerToResetTimer.GetComponent<DemoPlayer>().ResetTimer();
            Debug.Log("player reset timer");
        }
    }
    
    public void giftReward(string playerId, string giftName)
    {
        DemoPlayer playerToReward = GameObject.Find(playerId).GetComponent<DemoPlayer>();
        
        //need to set up different gift levels
        if (giftName == "") {
            int currSkinColorIndex = playerToReward.GetSkinColorIndex();
            playerToReward.SetSkinColorIndex((currSkinColorIndex + 1) % skinColorCount);
        }
        else if (giftName == "Rose") {
            playerToReward.StartGlowEffect();
        }
        else if (giftName == "Star") {
            playerToReward.GrowSize(1.5f);
        }
    }
    
    
    public string getRankingJson()
    {
        Debug.Log("CalledGetRank");
        rankPlayers();
        List<string> playerIds = playerDatas.Select(player => player.GetPlayerId()).ToList();
        Debug.Log(playerIds.Count);
        var data = new { playerIds = playerIds };
        return JsonConvert.SerializeObject(data);
    }

    public void Restart()
    {
        foreach (DemoPlayer player in playerDatas)
        {
            GameObject playerGo = player.gameObject;
            Destroy(playerGo);
        }
        playerDatas.Clear();
    }

    public void rankPlayers()
    {
        for (int i = 0; i < playerDatas.Count; i++) {
            for (int j = i + 1; j < playerDatas.Count; j++) {
                if (playerDatas[j].getScore() > playerDatas[i].getScore()) {
                    // Swap
                    DemoPlayer tmp = playerDatas[i];
                    playerDatas[i] = playerDatas[j];
                    playerDatas[j] = tmp;
                }
            }
        }
    }
}
