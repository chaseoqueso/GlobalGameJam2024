using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private Button pressButton;
    
    void Start()
    {
        pressButton.Select();
    }

    void Update()
    {
        if(Keyboard.current.anyKey.wasPressedThisFrame)
        {
            mainMenu.ToggleTitleScreenOnClick(false);
        }
    }
}
