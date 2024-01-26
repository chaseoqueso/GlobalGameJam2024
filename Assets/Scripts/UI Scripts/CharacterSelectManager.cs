using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    public bool playerIsReady {get; private set;}

    [SerializeField] private GameObject nextArrowPanel;
    [SerializeField] private GameObject previousArrowPanel;

    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyText;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button randomizeButton;

    [SerializeField] private GameObject playerPanelPrefab;

    void Start()
    {
        playerIsReady = false;
    }

    public void RotateHead(bool selectNext)
    {
        
    }

    public void RotateTorso(bool selectNext)
    {
        
    }

    public void RotateLegs(bool selectNext)
    {
        
    }

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

    public void DisplayNewPlayer(int playerNumber)
    {
        // TODO: Instantiate new playerPanelPrefab - need player name and a tiny camera window of their character or at least the head
    }

    public void RemovePlayer(int playerNumber)
    {
        // TODO: Delete that panel
    }

    public void PlayerIsReady(int playerNumber, bool isReady)
    {
        // TODO: Tell the UI to toggle that one Ready (or no longer Ready)
    }
}
