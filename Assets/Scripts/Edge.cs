using System.Collections;
using System.Collections.Generic;
using Fall_Friends.Controllers;
using Fall_Friends.Manager;
using UnityEngine;

public class Edge : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.players.GetComponent<PlayerManager>().deletePlayer(other.GetComponent<DemoPlayer>().GetPlayerId());
        }   
    }
}
