using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerPanel : MonoBehaviour
{
    [SerializeField] private GameObject readyUI;

    void Start()
    {
        // TODO: Set values
    }

    public void PlayerIsReady(bool isReady)
    {
        readyUI.SetActive(isReady);
    }
}
