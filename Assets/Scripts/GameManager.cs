using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    #region Singleton
    private static GameManager instance;
    public static GameManager Instance
    { 
        get { 
            return instance; 
        } 
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        } else
        {
            instance = this;
        }

        DontDestroyOnLoad(this);
    }

    #endregion

    #region PlayerManagement
    private int player_count = 0;
    public int MAX_PLAYERS = 4;
    private Player[] players;

    void newPlayer(int player_id)
    {
        if (player_count == MAX_PLAYERS) {
            Debug.Log("Already at max players");
            return;
        }

        players[player_count] = new Player(player_id);
        player_count++;
    }

    #endregion

    void Start()
    {
        players = new Player[MAX_PLAYERS];
    }
}
