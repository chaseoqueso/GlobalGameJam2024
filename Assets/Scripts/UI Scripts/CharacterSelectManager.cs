using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ModelPart{
    Head,   // 0
    Torso,  // 1
    Legs    // 2
}

public enum ModelID{
    ShovelKnight,
    OldGod,
    Dinosaur,
    Clown,
    Kirby,
    EndEnum
}

public class CharacterSelectManager : MonoBehaviour
{
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

    void Start()
    {
        playerIsReady = false;

        SetStats();

        DisplayNewPlayer(); // TODO: Pass in this player's info
    }

    #region Model/Stat Stuff
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
            speedSlider.value = Random.Range(1,11);
            weightSlider.value = Random.Range(1,11);
            handlingSlider.value = Random.Range(1,11);
            chargeSlider.value = Random.Range(1,11);
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
            }
            else{
                readyText.text = "READY";
            }

            // TODO: Alert server
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
            // TODO: Generate three random numbers and select a random thing from each category
        }

        public void QuitButton()
        {
            // TODO: Alert server you left the lobby - tell all the clients to remove this player - and return to main menu
        }
    #endregion

    #region Lobby Stuff
        public void DisplayNewPlayer() // TODO: pass in player number or Player or something
        {
            Instantiate(playerPanelPrefab, new Vector3(0,0,0), Quaternion.identity, lobbyPanelBackground.transform);

            // TODO: Player name and a tiny camera window of their character or at least the head
        }

        public void RemovePlayer() // TODO: pass in player number or Player or something
        {
            // TODO: Delete that panel
        }

        public void PlayerIsReady(bool isReady) // TODO: pass in player number or Player or something
        {
            // TODO: Tell the UI to toggle that one's Ready overlay (or no longer Ready)
        }
    #endregion
}
