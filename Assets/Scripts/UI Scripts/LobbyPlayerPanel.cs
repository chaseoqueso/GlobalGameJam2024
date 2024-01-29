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

    public void PlayerIsReady(bool isReady, Sprite readySprite)
    {
        readyUI.SetActive(isReady);
        readyImg.sprite = readySprite;
    }

    public void SetValues(string username)
    {
        usernameText.text = username;
    }
}
