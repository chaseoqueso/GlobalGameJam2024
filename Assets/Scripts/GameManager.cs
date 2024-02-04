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

public struct PlayerModels : INetworkSerializeByMemcpy 
{
    public ModelID head;
    public ModelID body;
    public ModelID legs;
}

public class GameManager : NetworkBehaviour
{
    static public GameManager Instance { get; internal set; }

    public const string MAIN_MENU_SCENE = "MainMenu";
    public const string CHAR_SELECT_SCENE = "Character Customization";
    public const string GAME_SCENE = "Networking";      // TEMP - TODO: Change this to the actual game scene name

    public NetworkVariable<NetworkString> joinCode = new();
    public string cachedUsername;

    public Dictionary<ModelID,GameObject> headDatabase = new Dictionary<ModelID,GameObject>();
    public Dictionary<ModelID,GameObject> torsoDatabase = new Dictionary<ModelID,GameObject>();
    public Dictionary<ModelID,GameObject> legsDatabase = new Dictionary<ModelID,GameObject>();

    public float roundDuration = 60;
    public float timer { get; private set; }
    private bool gameHasStarted;

    private Dictionary<ulong, string> usernameDict = new();
    private Dictionary<ulong, PlayerModels> modelDict = new();
    private SortedList<ulong, int> scoreboard = new();

    void Awake()
    {
        if(Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        if(gameHasStarted)
            UpdateTimer();
    }

    public HashSet<ulong> GetClientIDs()
    {
        return new HashSet<ulong>(usernameDict.Keys);
    }

    public void UpdatePlayerScore(ulong clientID, int score)
    {
        if (scoreboard.ContainsKey(clientID))
        {
            scoreboard[clientID] = score;
        }
        else
        {
            scoreboard.Add(clientID, score);
        }

        if (IsClient && scoreboard.Count > 1)
        {
            int place = scoreboard.IndexOfKey(clientID) + 1;
            bool isInFirst = place == 1;

            string firstPlayerName = GetUsername(isInFirst ? clientID : scoreboard.ElementAt(0).Key);
            int firstPlayerScore = isInFirst ? scoreboard[clientID] : scoreboard.ElementAt(0).Value;
            string otherPlayerName = GetUsername(isInFirst ? scoreboard.ElementAt(1).Key : clientID);
            int otherPlayerScore = isInFirst ? scoreboard.ElementAt(1).Value : scoreboard[clientID];
            
            GameUI.Instance.UpdateScoreboard(firstPlayerName, 
                                            firstPlayerScore, 
                                            otherPlayerName, 
                                            otherPlayerScore, 
                                            place);
        }
    }

#region Usernames
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

    public void RemovePlayer(ulong clientId)
    {
        RemoveUsernameServerRpc(clientId);
        RemovePlayerModelServerRpc(clientId);
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
#endregion

#region PlayerModels

    public void LoadAllModels()
    {
        // Load in the models of each type from the relevant folder in Resources

        // Heads

        Object[] headList = Resources.LoadAll("Models/Heads");   //, typeof(ModelPart));
        // Debug.Log(string.Format("Loaded {0} heads", headList.Length));
        foreach(Object h in headList){
            GameObject head = (GameObject)h;

            ModelID modelID = head.GetComponent<Part>().modelId;     // TODO: Get the ID for this model
            
            if(headDatabase.ContainsKey( modelID )){
                    continue;
            }
            
            // Add the model to the dictionary
            headDatabase.Add( modelID, head );
        }

        // Torsos

        Object[] torsoList = Resources.LoadAll("Models/Torsos");   //, typeof(ModelPart));
        // Debug.Log(string.Format("Loaded {0} torsos", torsoList.Length));
        foreach (Object t in torsoList){
            GameObject torso = (GameObject)t;   // Cast by ModelPart ScriptableObject type
            ModelID modelID = torso.GetComponent<Part>().modelId;     // TODO: Get the ID for this model
                                                                      // 
            if (torsoDatabase.ContainsKey( modelID )){
                    continue;
            }
            
            // Add the model to the dictionary
            torsoDatabase.Add( modelID, torso );
        }

        // Legs

        Object[] legsList = Resources.LoadAll("Models/Legs");   //, typeof(ModelPart));
        // Debug.Log(string.Format("Loaded {0} legs", legsList.Length));
        foreach (Object l in legsList){
            GameObject legs = (GameObject)l;   // Cast by ModelPart ScriptableObject type
            ModelID modelID = legs.GetComponent<Part>().modelId;     // TODO: Get the ID for this model
                                                                      // 
            if (legsDatabase.ContainsKey( modelID )){
                    continue;
            }
            
            // Add the model to the dictionary
            legsDatabase.Add( modelID, legs );
        }

        // If the number of heads =/= torsos =/= legs, throw an error
        if( headDatabase.Count != torsoDatabase.Count || torsoDatabase.Count != legsDatabase.Count ){
            Debug.LogError("The number of model parts do not match!");
        }
    }

    public void SetPlayerModel(ulong clientId, PlayerModels models)
    {
        SetPlayerModelServerRpc(clientId, models);
    }

    public PlayerModels GetPlayerModels(ulong clientId)
    {
        return modelDict[clientId];
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerModelServerRpc(ulong clientId, PlayerModels models)
    {
        SetPlayerModelInternal(clientId, models);
        SetPlayerModelClientRpc(clientId, models);
    }

    [ClientRpc]
    private void SetPlayerModelClientRpc(ulong clientId, PlayerModels models)
    {
        SetPlayerModelInternal(clientId, models);
    }

    private void SetPlayerModelInternal(ulong clientId, PlayerModels models)
    {
        if(modelDict.ContainsKey(clientId))
        {
            modelDict[clientId] = models;
        }
        else
        {
            modelDict.Add(clientId, models);
        }
    }

    [ServerRpc]
    private void RemovePlayerModelServerRpc(ulong clientId)
    {
        RemovePlayerModelInternal(clientId);
        RemovePlayerModelClientRpc(clientId);
    }

    [ClientRpc]
    private void RemovePlayerModelClientRpc(ulong clientId)
    {
        RemovePlayerModelInternal(clientId);
    }

    private void RemovePlayerModelInternal(ulong clientId)
    {
        if(modelDict.ContainsKey(clientId))
        {
            modelDict.Remove(clientId);
        }
    }
#endregion

#region Timer
    private void UpdateTimer()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;    // Timer is purely visual on the client
            if(timer < 0)
                timer = 0;

            if(IsClient)
            {
                GameUI.Instance.SetTime(timer);
            }

            if(IsServer && timer <= 0)
            {
                EndGameClientRpc();
            }
        }
    }

    [ServerRpc]
    public void StartTimerServerRpc()
    {
        StartTimer();
        StartTimerClientRpc();
    }

    [ClientRpc]
    private void StartTimerClientRpc()
    {
        StartTimer();
    }

    private void StartTimer()
    {
        timer = roundDuration;
        gameHasStarted = true;
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        // TODO end the game
    }
#endregion

}
