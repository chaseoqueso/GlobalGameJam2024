using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
        if(Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    // [ServerRpc]
    // public void SetJoinCodeServerRpc(string joinCode)
    // {
    //     this.joinCode.Value = joinCode;
    // }
}
