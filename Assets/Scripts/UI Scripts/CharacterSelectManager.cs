using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

// public enum ModelPart{
//     Head,   // 0
//     Torso,  // 1
//     Legs    // 2
// }

public enum ModelID{
    ShovelKnight,
    OldGod,
    Dinosaur,
    Clown,
    Kirby,
    EndEnum
}

public class CharacterSelectManager : NetworkBehaviour
{
    [Tooltip("The minimum number of players before the game will start.")]
    public int minPlayers = 1;

    [SerializeField] private ModelSelectCircle modelSelectCircle;

    public bool playerIsReady {get; private set;}

    #region Interactable Objects
        [SerializeField] private GameObject nextArrowPanel;
        [SerializeField] private GameObject previousArrowPanel;

        [SerializeField] private Button readyButton;
        [SerializeField] private TMP_Text readyText;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button randomizeButton;

        [SerializeField] private Slider speedSlider;
        [SerializeField] private Slider weightSlider;
        [SerializeField] private Slider handlingSlider;
        [SerializeField] private Slider chargeSlider;
    #endregion

    [SerializeField] private GameObject lobbyPanelBackground;

    [SerializeField] private GameObject playerPanelPrefab;
    [SerializeField] private Sprite readyButtonSprite;
    [SerializeField] private Sprite cancelButtonSprite;
    
    private bool allPlayersInLobby;
    private Dictionary<ulong, bool> clientsInLobby;

    void Start()
    {
        playerIsReady = false;

        SetStats();

        DisplayNewPlayer(); // TODO: Pass in this player's info
    }

    #region Display Model/Stat Stuff
        public void RotateHead(bool selectNext)
        {
            modelSelectCircle.RotateHead(selectNext);
            SetStats();
        }

        public void RotateTorso(bool selectNext)
        {
            modelSelectCircle.RotateTorso(selectNext);
            SetStats();
        }

        public void RotateLegs(bool selectNext)
        {
            modelSelectCircle.RotateLegs(selectNext);
            SetStats();
        }

        private void SetStats()
        {
            // TODO: Display the stats of the current character

            // TEMP for visualization purposes
            speedSlider.value = Random.Range(1,10f);
            weightSlider.value = Random.Range(1,10f);
            handlingSlider.value = Random.Range(1,10f);
            chargeSlider.value = Random.Range(1,10f);
        }
    #endregion

    #region Interactables
        public void ReadyButton()
        {
            // If the player is cancelling Ready, return to interactable UI
            ToggleInteractableUI(playerIsReady);
            playerIsReady = !playerIsReady;

            if(playerIsReady){
                readyText.text = "CANCEL";
                readyButton.image.sprite = cancelButtonSprite;
            }
            else{
                readyText.text = "READY";
                readyButton.image.sprite = readyButtonSprite;
            }

            PlayerIsReady(playerIsReady);
        }

        private void ToggleInteractableUI(bool toggle)
        {
            nextArrowPanel.SetActive(toggle);
            previousArrowPanel.SetActive(toggle);
            quitButton.interactable = toggle;
            randomizeButton.interactable = toggle;
        }

        public void RandomizeButton()
        {
            int headValue = Random.Range(0,(int)ModelID.EndEnum);
            int torsoValue = Random.Range(0,(int)ModelID.EndEnum);
            int legsValue = Random.Range(0,(int)ModelID.EndEnum);

            // TODO: Rotate to these immediately
        }

        public void QuitButton()
        {
            // TODO: Alert server you left the lobby - tell all the clients to remove this player - and return to main menu
        }
    #endregion

    #region Lobby Stuff

        public override void OnNetworkSpawn()
        {
            clientsInLobby = new();
            
            //Always add ourselves to the list at first
            clientsInLobby.Add(NetworkManager.LocalClientId, false);

            //If we are hosting, then handle the server side for detecting when clients have connected
            //and when their lobby scenes are finished loading.
            if (IsServer)
            {
                allPlayersInLobby = false;

                //Server will be notified when a client connects
                NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
                SceneTransitionHandler.Instance.OnClientLoadedScene += ClientLoadedScene;
            }

            //Update our lobby
            GenerateUsersInLobby();
        }

        private void GenerateUsersInLobby()
        {
            // TODO fill the lobby UI with players
            // lobbyPlayerPanel.PlayerIsReady(isReady);
        }

        /// <summary>
        ///     ClientLoadedScene
        ///     Invoked when a client has loaded this scene
        /// </summary>
        /// <param name="clientId"></param>
        private void ClientLoadedScene(ulong clientId)
        {
            if (IsServer)
            {
                if (!clientsInLobby.ContainsKey(clientId))
                {
                    clientsInLobby.Add(clientId, false);
                    GenerateUsersInLobby();
                }

                UpdateAndCheckPlayersInLobby();
            }
        }

        /// <summary>
        ///     OnClientConnectedCallback
        ///     Since we are entering a lobby and Netcode's NetworkManager is spawning the player,
        ///     the server can be configured to only listen for connected clients at this stage.
        /// </summary>
        /// <param name="clientId">client that connected</param>
        private void OnClientConnectedCallback(ulong clientId)
        {
            if (IsServer)
            {
                if (!clientsInLobby.ContainsKey(clientId)) clientsInLobby.Add(clientId, false);
                GenerateUsersInLobby();

                UpdateAndCheckPlayersInLobby();
            }
        }

        /// <summary>
        ///     UpdateAndCheckPlayersInLobby
        ///     Checks to see if we have at least 2 or more people to start
        /// </summary>
        private void UpdateAndCheckPlayersInLobby()
        {
            allPlayersInLobby = clientsInLobby.Count >= minPlayers;

            foreach (var clientLobbyStatus in clientsInLobby)
            {
                SendClientReadyStatusUpdatesClientRpc(clientLobbyStatus.Key, clientLobbyStatus.Value);
                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientLobbyStatus.Key))

                    //If some clients are still loading into the lobby scene then this is false
                    allPlayersInLobby = false;
            }

            CheckForAllPlayersReady();
        }

        /// <summary>
        ///     SendClientReadyStatusUpdatesClientRpc
        ///     Sent from the server to the client when a player's status is updated.
        ///     This also populates the connected clients' (excluding host) player state in the lobby
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="isReady"></param>
        [ClientRpc]
        private void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady)
        {
            if (!IsServer)
            {
                if (!clientsInLobby.ContainsKey(clientId))
                    clientsInLobby.Add(clientId, isReady);
                else
                    clientsInLobby[clientId] = isReady;
                GenerateUsersInLobby();
            }
        }

        /// <summary>
        ///     CheckForAllPlayersReady
        ///     Checks to see if all players are ready, and if so launches the game
        /// </summary>
        private void CheckForAllPlayersReady()
        {
            if (allPlayersInLobby)
            {
                var allPlayersAreReady = true;
                foreach (var clientLobbyStatus in clientsInLobby)
                    if (!clientLobbyStatus.Value)

                        //If some clients are still loading into the lobby scene then this is false
                        allPlayersAreReady = false;

                //Only if all players are ready
                if (allPlayersAreReady)
                {
                    //Remove our client connected callback
                    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;

                    //Remove our scene loaded callback
                    SceneTransitionHandler.Instance.OnClientLoadedScene -= ClientLoadedScene;

                    //Transition to the ingame scene
                    SceneTransitionHandler.Instance.SwitchScene(SceneTransitionHandler.SceneStates.Ingame);
                }
            }
        }

        private void PlayerIsReady(bool isReady)
        {
            clientsInLobby[NetworkManager.Singleton.LocalClientId] = isReady;
            if (IsServer)
            {
                UpdateAndCheckPlayersInLobby();
            }
            else
            {
                OnClientIsReadyServerRpc(NetworkManager.Singleton.LocalClientId, isReady);
            }

            GenerateUsersInLobby();
        }

        /// <summary>
        ///     OnClientIsReadyServerRpc
        ///     Sent to the server when the player clicks the ready button
        /// </summary>
        /// <param name="clientid">clientId that is ready</param>
        [ServerRpc(RequireOwnership = false)]
        private void OnClientIsReadyServerRpc(ulong clientid, bool isReady)
        {
            if (clientsInLobby.ContainsKey(clientid))
            {
                clientsInLobby[clientid] = isReady;
                UpdateAndCheckPlayersInLobby();
                GenerateUsersInLobby();
            }
        }
        
        public void DisplayNewPlayer() // TODO: pass in player number or Player or something
        {
            Instantiate(playerPanelPrefab, new Vector3(0,0,0), Quaternion.identity, lobbyPanelBackground.transform);

            // TODO: Player name and a tiny camera window of their character or at least the head
        }

        public void RemovePlayer() // TODO: pass in player number or Player or something
        {
            // TODO: Delete that panel
        }
    #endregion
}
