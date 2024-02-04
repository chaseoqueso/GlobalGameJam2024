using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerScorePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text username;
    [SerializeField] private TMP_Text kills;
    [SerializeField] private TMP_Text deaths;
    [SerializeField] private TMP_Text score;

    public void SetValues(string _username, int _kills, int _deaths, int _score)
    {
        username.text = _username;
        kills.text = _kills + "";
        deaths.text = _deaths + "";
        score.text = _score + "";
    }
}
