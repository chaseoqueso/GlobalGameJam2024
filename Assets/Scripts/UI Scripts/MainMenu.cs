using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Text.RegularExpressions;

public class MainMenu : MonoBehaviour
{
    public enum NetworkMode
    {
        Host,
        Client
    }

    public TMP_InputField IPInputField;
    public TMP_InputField portInputField;
    public TMP_Text menuStatusText;
    public Button startButton;
    public Button backButton;

    NetworkManager networkManager;
    UnityTransport networkTransport;
    
    // This is needed to make the port field more convenient. GUILayout.TextField is very limited and we want to be able to clear the field entirely so we can't cache this as ushort.
    string portString = "7777";
    string connectAddress = "127.0.0.1";
    NetworkMode networkMode;
    TMP_Text startServerButtonText;

    void Awake()
    {
        // Only cache networking manager but not transport here because transport could change anytime.
        networkManager = NetworkManager.Singleton;
        startServerButtonText = startButton.GetComponentInChildren<TMP_Text>();

        IPInputField.text = connectAddress;
        portInputField.text = portString;
    }
    
    void Start()
    {
        networkTransport = (UnityTransport)networkManager.NetworkConfig.NetworkTransport;
        
        ShowStatusText(false);
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnOnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnOnClientDisconnectCallback;
    }

    void OnOnClientConnectedCallback(ulong obj)
    {
        // Do Stuff
    }

    void OnOnClientDisconnectCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer && clientId != NetworkManager.ServerClientId)
        {
            return;
        }
        // Do Stuff
    }

    bool SetConnectionData()
    {
        connectAddress = SanitizeInput(IPInputField.text);
        portString = SanitizeInput(IPInputField.text);

        if (connectAddress == "")
        {
            menuStatusText.text = "IP Address Invalid";
            StopAllCoroutines();
            StartCoroutine(ShowInvalidInputStatus());
            return false;
        }
        
        if (portString == "")
        {
            menuStatusText.text = "Port Invalid";
            StopAllCoroutines();
            StartCoroutine(ShowInvalidInputStatus());
            return false;
        }

        if (ushort.TryParse(portString, out ushort port))
        {
            networkTransport.SetConnectionData(connectAddress, port);
        }
        else
        {
            networkTransport.SetConnectionData(connectAddress, 7777);
        }
        return true;
    }

    public void NetworkMenuHost()
    {
        networkMode = NetworkMode.Host;
        startServerButtonText.text = "Host";
    }

    public void NetworkMenuClient()
    {
        networkMode = NetworkMode.Client;
        startServerButtonText.text = "Join";
    }

    public void StartNetworkButton()
    {
        if(networkMode == NetworkMode.Host)
        {
            StartHost();
        }
        else
        {
            TryJoinGame();
        }
    }
    
    public void BackButton()
    {
        networkManager.Shutdown();
    }

    private void StartHost()
    {
        if (SetConnectionData() && NetworkManager.Singleton.StartHost())
        {
            SceneTransitionHandler.Instance.RegisterCallbacks();
            SceneTransitionHandler.Instance.SwitchScene(GameManager.CHAR_SELECT_SCENE);
        }
    }

    private void TryJoinGame()
    {
        if (SetConnectionData())
        {
            NetworkManager.Singleton.StartClient();
            StopAllCoroutines();
            StartCoroutine(ShowConnectingStatus());
        }
    }

    public void ExitGame()
    {
        Debug.Log ("Quit");
        Application.Quit();
    }
    
    static string SanitizeInput(string dirtyString)
    {
        // sanitize the input for the ip address
        return Regex.Replace(dirtyString, "[^0-9.]", "");
    }
    
    void ShowStatusText(bool visible)
    {
        Color c = menuStatusText.color;
        menuStatusText.color = visible ? new(c.r, c.g, c.b, 1) : new(c.r, c.g, c.b, 0);
    }

    IEnumerator ShowInvalidInputStatus()
    {
        ShowStatusText(true);

        yield return new WaitForSeconds(3f);
        
        ShowStatusText(false);
    }
    
    IEnumerator ShowConnectingStatus()
    {
        menuStatusText.text = "Attempting to Connect...";
        ShowStatusText(true);
        
        startButton.interactable = false;
        backButton.interactable = false;
        
        var unityTransport = networkManager.GetComponent<UnityTransport>();
        var connectTimeoutMs = unityTransport.ConnectTimeoutMS;
        var maxConnectAttempts = unityTransport.MaxConnectAttempts;

        yield return new WaitForSeconds(connectTimeoutMs * maxConnectAttempts / 1000f);

        // wait to verify connect status
        yield return new WaitForSeconds(1f);
        
        menuStatusText.text = "Connection Attempt Failed";
        startButton.interactable = true;
        backButton.interactable = true;
        
        yield return new WaitForSeconds(3f);
        
        ShowStatusText(false);
    }
}
