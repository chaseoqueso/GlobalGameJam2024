using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyPlayerPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private GameObject readyUI;

    public void PlayerIsReady(bool isReady)
    {
        readyUI.SetActive(isReady);
    }

    public void SetValues(Player p)
    {
        // TODO
        usernameText.text = "user test";


        // TODO?: tiny camera window of their character or at least the head?
    }
}
