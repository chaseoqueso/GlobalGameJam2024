using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class EndScoreboard : NetworkBehaviour
{
    [SerializeField] private GameObject scoresGrid;
    [SerializeField] private GameObject playerScorePanelPrefab;
    [SerializeField] private GameObject newGameButton;

    void OnEnable()
    {
        newGameButton.SetActive(IsHost || IsServer);

        foreach(ulong clientID in GameManager.Instance.GetClientIDs()){
            GameObject newPlayerPanel = Instantiate(playerScorePanelPrefab, new Vector3(0,0,0), Quaternion.identity, scoresGrid.transform);

            string username = GameManager.Instance.GetUsername(clientID);

            Player currentPlayer = GameManager.Instance.GetPlayer(clientID);
            int kills = currentPlayer.kills.Value;
            int deaths = currentPlayer.deaths.Value;
            int score = currentPlayer.score;

            newPlayerPanel.GetComponent<PlayerScorePanel>().SetValues(username, kills, deaths, score);
        }
    }

    public void NewGameButton()
    {
        SceneTransitionHandler.Instance.SwitchScene(SceneTransitionHandler.SceneStates.Lobby);
    }

    public void QuitGameButton()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(GameManager.MAIN_MENU_SCENE);
    }
}
