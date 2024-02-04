using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ArenaManager : NetworkBehaviour
{
    public GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            SceneTransitionHandler.Instance.OnClientLoadedScene += OnClientLoadedScene;
        }
    }

    private void OnClientLoadedScene(ulong _)
    {
        if(SceneTransitionHandler.Instance.AllClientsAreLoaded())
        {
            foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                GameObject player = Instantiate(playerPrefab, new Vector3(0, 10, 0), Quaternion.identity);  // TODO change this to actual spawn points
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            }

            GameManager.Instance.StartTimerServerRpc();
        }
    }

    void OnDestroy()
    {
        SceneTransitionHandler.Instance.OnClientLoadedScene -= OnClientLoadedScene;
    }
}
