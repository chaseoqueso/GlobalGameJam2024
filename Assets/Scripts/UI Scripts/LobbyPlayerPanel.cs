using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private GameObject readyUI;
    [SerializeField] private Image readyImg;

    public void PlayerIsReady(bool isReady)
    {
        readyUI.SetActive(isReady);
    }

    public void SetValues(Player p)
    {
        // TODO
        usernameText.text = "user test";

        // readyImg.sprite = random sprite
    }
}
