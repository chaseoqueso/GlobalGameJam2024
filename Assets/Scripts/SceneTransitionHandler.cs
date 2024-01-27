using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SceneTransitionHandler : NetworkBehaviour
{
    static public SceneTransitionHandler Instance { get; internal set; }

    [HideInInspector]
    public delegate void ClientLoadedSceneDelegateHandler(ulong clientId);
    [HideInInspector]
    public event ClientLoadedSceneDelegateHandler OnClientLoadedScene;

    [HideInInspector]
    public delegate void SceneStateChangedDelegateHandler(SceneStates newState);
    [HideInInspector]
    public event SceneStateChangedDelegateHandler OnSceneStateChanged;

    private int numberOfClientsLoaded;
    
    /// <summary>
    /// Example scene states
    /// </summary>
    public enum SceneStates
    {
        MainMenu,
        Lobby,
        Ingame
    }

    private SceneStates sceneState;

    /// <summary>
    /// Awake
    /// If another version exists, destroy it and use the current version
    /// Set our scene state to INIT
    /// </summary>
    private void Awake()
    {
        if(Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        SetSceneState(SceneStates.MainMenu);
        DontDestroyOnLoad(this);
    }

    /// <summary>
    /// SetSceneState
    /// Sets the current scene state to help with transitioning.
    /// </summary>
    /// <param name="sceneState"></param>
    private void SetSceneState(SceneStates sceneState)
    {
        this.sceneState = sceneState;
        OnSceneStateChanged?.Invoke(this.sceneState);
    }

    /// <summary>
    /// GetCurrentSceneState
    /// Returns the current scene state
    /// </summary>
    /// <returns>current scene state</returns>
    public SceneStates GetCurrentSceneState()
    {
        return sceneState;
    }

    /// <summary>
    /// Registers callbacks to the NetworkSceneManager. This should be called when starting the server
    /// </summary>
    public void RegisterCallbacks()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        
    }

    /// <summary>
    /// Switches to a new scene
    /// </summary>
    /// <param name="scene"></param>
    public void SwitchScene(SceneStates scene)
    {
        string scenename = GetSceneName(scene);
        if(NetworkManager.Singleton.IsListening)
        {
            numberOfClientsLoaded = 0;
            NetworkManager.Singleton.SceneManager.LoadScene(scenename, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadSceneAsync(scenename);
        }
        SetSceneState(scene);
    }

    private string GetSceneName(SceneStates state)
    {
        return state switch
        {
            SceneStates.Lobby => GameManager.CHAR_SELECT_SCENE,
            SceneStates.Ingame => GameManager.GAME_SCENE,
            _ => GameManager.MAIN_MENU_SCENE,
        };
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        numberOfClientsLoaded += 1;
        OnClientLoadedScene?.Invoke(clientId);
    }

    public bool AllClientsAreLoaded()
    {
        return numberOfClientsLoaded == NetworkManager.Singleton.ConnectedClients.Count;
    }

    /// <summary>
    /// ExitAndLoadStartMenu
    /// This should be invoked upon a user exiting a multiplayer game session.
    /// </summary>
    public void ExitAndLoadStartMenu()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }
        
        OnClientLoadedScene = null;
        SetSceneState(SceneStates.MainMenu);
        SceneManager.LoadScene(GameManager.MAIN_MENU_SCENE);
    }
}