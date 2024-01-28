using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct NetworkString : INetworkSerializable
{
    private FixedString32Bytes info;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
    }

    public override string ToString()
    {
        return info.ToString();
    }

    public static implicit operator string(NetworkString s) => s.ToString();
    public static implicit operator NetworkString(string s) => new NetworkString() { info = new FixedString32Bytes(s) };
}

public class GameManager : NetworkBehaviour
{
    static public GameManager Instance { get; internal set; }

    public const string MAIN_MENU_SCENE = "MainMenu";
    public const string CHAR_SELECT_SCENE = "Character Customization";
    public const string GAME_SCENE = "Networking";      // TEMP - TODO: Change this to the actual game scene name

    public NetworkVariable<NetworkString> joinCode = new();
    public string cachedUsername;

    private Dictionary<ulong, string> usernameDict = new();

    void Awake()
    {
        if(Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void AddCachedUsername(ulong clientId)
    {
        Debug.Log($"Adding cached username {cachedUsername} to client {clientId}");
        NetworkManager.Singleton.OnClientConnectedCallback -= AddCachedUsername;
        AddUsernameServerRpc(clientId, cachedUsername);
    }

    public void AddUsername(ulong clientId, string username)
    {
        AddUsernameServerRpc(clientId, username);
    }

    public string GetUsername(ulong clientId)
    {
        if(usernameDict.ContainsKey(clientId))
            return usernameDict[clientId];
        
        return null;
    }

    public void RemoveUsername(ulong clientId)
    {
        RemoveUsernameServerRpc(clientId);
    }

    public void SyncUsernames(ulong[] clientIds)
    {
        SyncUsernamesServerRpc(clientIds);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddUsernameServerRpc(ulong clientId, string username)
    {
        Debug.Log($"SERVER: Adding username {username} to client {clientId}");
        AddUsernameInternal(clientId, username);
        Debug.Log($"SERVER: New username for client {clientId}: {usernameDict[clientId]}");
        AddUsernameClientRpc(clientId, username);
    }

    [ClientRpc]
    private void AddUsernameClientRpc(ulong clientId, string username)
    {
        Debug.Log($"CLIENT: Adding username {username} to client {clientId}");
        AddUsernameInternal(clientId, username);
        Debug.Log($"CLIENT: New username for client {clientId}: {usernameDict[clientId]}");
    }

    private void AddUsernameInternal(ulong clientId, string username)
    {
        if (usernameDict.ContainsKey(clientId))
        {
            usernameDict[clientId] = username;
        }
        else
        {
            usernameDict.Add(clientId, username);
        }
    }

    [ServerRpc]
    private void RemoveUsernameServerRpc(ulong clientId)
    {
        RemoveUsernameInternal(clientId);
        RemoveUsernameClientRpc(clientId);
    }

    [ClientRpc]
    private void RemoveUsernameClientRpc(ulong clientId)
    {
        RemoveUsernameInternal(clientId);
    }

    private void RemoveUsernameInternal(ulong clientId)
    {
        if (usernameDict.ContainsKey(clientId))
        {
            usernameDict.Remove(clientId);
        }
    }
    
    [ServerRpc]
    private void SyncUsernamesServerRpc(ulong[] clientIds)
    {
        Debug.Log($"SERVER: Syncing usernames for {clientIds.Length} client(s).");
        ClientRpcParams clientRpcParams = new();
        clientRpcParams.Send = new ClientRpcSendParams();
        clientRpcParams.Send.TargetClientIds = clientIds;

        ulong[] keys = new ulong[usernameDict.Count];
        NetworkString[] values = new NetworkString[usernameDict.Count];
        int i = 0;
        foreach(var kvp in usernameDict)
        {
            Debug.Log($"SERVER: Username {i} - Client: {kvp.Key} Username {kvp.Value}.");
            keys[i] = kvp.Key;
            values[i] = kvp.Value;
            i++;
        }
        
        SyncUsernamesClientRpc(keys, values, clientRpcParams);
    }
    
    [ClientRpc]
    private void SyncUsernamesClientRpc(ulong[] keys, NetworkString[] values, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"CLIENT: Syncing usernames for {keys.Length} client(s).");
        usernameDict.Clear();
        for(int i = 0; i < keys.Length; i++)
        {
            usernameDict.Add(keys[i], values[i]);
            Debug.Log($"CLIENT: Synced username {values[i]} to client {keys[i]}");
        }
    }
}
