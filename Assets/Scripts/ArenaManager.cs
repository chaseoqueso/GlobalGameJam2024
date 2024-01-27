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
                GameObject player = Instantiate(playerPrefab, new Vector3(0, 10, 0), Quaternion.identity);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            }
        }
    }
}
