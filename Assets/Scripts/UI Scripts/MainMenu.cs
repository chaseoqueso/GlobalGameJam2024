using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField IPInputField;
    public TMP_InputField portInputField;

    public void HostButton()
    {

    }
    public void TryJoinGame()
    {

    }
    public void ExitGame()
    {
        Debug.Log ("Quit");
        Application.Quit();
    }
}
