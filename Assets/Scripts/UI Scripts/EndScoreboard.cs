using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EndScoreboard : MonoBehaviour
{
    [SerializeField] private GameObject scoresGrid;
    [SerializeField] private GameObject playerScorePanelPrefab;

    void OnEnable()
    {
        // TODO: Buttons only visible - or at least only interactable - for host

        foreach(ulong clientID in NetworkManager.Singleton.ConnectedClientsIds){
            GameObject newPlayerPanel = Instantiate(playerScorePanelPrefab, new Vector3(0,0,0), Quaternion.identity, scoresGrid.transform);

            string username = GameManager.Instance.GetUsername(clientID);

            // TODO: Get other values
            int kills = 0;
            int deaths = 0;
            int score = 0;

            newPlayerPanel.GetComponent<PlayerScorePanel>().SetValues(username, kills, deaths, score);
        }
    }

    public void NewGameButton()
    {
        Debug.Log("new game");
        // TODO (already attached to the button in the prefab)
    }

    public void QuitGameButton()
    {
        Debug.Log("QUIT");
        // TODO (already attached to the button in the prefab)
    }
}
