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

    public TMP_InputField codeInputField;
    public TMP_InputField hostUsernameInputField;
    public TMP_InputField usernameInputField;
    public TMP_Text menuStatusText;
    public Button startButton;
    public Button backButton;

    NetworkManager networkManager;
    
    void Awake()
    {
        // Only cache networking manager but not transport here because transport could change anytime.
        networkManager = NetworkManager.Singleton;
    }
    
    void Start()
    {
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
    
    public void BackButton()
    {
        networkManager.Shutdown();
    }

    public async void StartHost()
    {
        Debug.Log("Starting Host");
        Debug.Log(NetworkManager.Singleton);
        Debug.Log(NetworkManager.Singleton.GetComponent<RelayManager>());
        RelayManager relayManager = NetworkManager.Singleton.GetComponent<RelayManager>();
        Debug.Log(relayManager);
        if(relayManager.IsRelayEnabled)
        {
            string joinCode = await relayManager.StartHostWithRelay();
            Debug.Log($"Join code: {joinCode}");
            
            if (joinCode != null)
            {
                GameManager.Instance.joinCode.Value = joinCode;
                GameManager.Instance.AddUsername(NetworkManager.Singleton.LocalClientId, hostUsernameInputField.text);
                // NetworkManager.Singleton.GetComponent<GameManager>().SetJoinCodeServerRpc(joinCode);
                SceneTransitionHandler.Instance.RegisterCallbacks();
                SceneTransitionHandler.Instance.SwitchScene(SceneTransitionHandler.SceneStates.Lobby);
            }
            else
            {
                Debug.LogError("heck");
            }
        }
        else
        {
            Debug.LogError("no relay");
        }
    }

    public async void TryJoinGame()
    {
        RelayManager relayManager = NetworkManager.Singleton.GetComponent<RelayManager>();
        NetworkManager.Singleton.OnClientConnectedCallback += GameManager.Instance.AddCachedUsername;
        if(relayManager.IsRelayEnabled)
        {
            if (await relayManager.StartClientWithRelay(codeInputField.text))
            {
                GameManager.Instance.cachedUsername = usernameInputField.text;
                SceneTransitionHandler.Instance.RegisterCallbacks();
                StopAllCoroutines();
                StartCoroutine(ShowConnectingStatus());
                return;
            }
            else
            {
                menuStatusText.text = "Invalid Game Code";
            }
        }
        else
        {
            menuStatusText.text = "Relay not enabled";
        }
        
        StopAllCoroutines();
        StartCoroutine(ShowInvalidInputStatus());
    }

    public void ExitGame()
    {
        Debug.Log ("Quit");
        Application.Quit();
    }
    
    // static string SanitizeInput(string dirtyString)
    // {
    //     // sanitize the input for the ip address
    //     return Regex.Replace(dirtyString, "[^0-9.]", "");
    // }
    
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
