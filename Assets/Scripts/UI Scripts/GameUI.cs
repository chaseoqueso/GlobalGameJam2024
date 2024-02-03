using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [SerializeField] private float firstSegmentThreshold;
    [SerializeField] private float secondSegmentThreshold;

    private bool firstSegmentActive = true;
    private bool secondSegmentActive = false;
    private bool thirdSegmentActive = false;
    private bool fullChargeActive = false;

    [SerializeField] private Gradient chargeBarGradient;
    [SerializeField] private Slider chargeBar;
    [SerializeField] private Image chargeBarFill;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreboardText1;
    [SerializeField] private TMP_Text scoreboardText2;
    
    [SerializeField] private Slider healthBar;

    [SerializeField] private GameObject gameOverUI;

    void Awake()
    {
        if (Instance != null && Instance != this){ 
            Destroy(this); 
        } 
        else{ 
            Instance = this;
        }

        // TODO: Set default values from player info
        // healthBar.maxValue = player.health;
        SetHealthBar(healthBar.maxValue);

        SetCharge(0f);
    }

    #region Health Bar
        public void SetHealthBar(float value)
        {
            if(value < healthBar.value){
                DamageFeedback();
            }

            healthBar.value = value;
            CheckHealthStatus();
        }

        public void IncrementHealth(float value)
        {
            healthBar.value += value;
        }

        public void DecreaseHealth(float value)
        {
            healthBar.value -= value;
            DamageFeedback();
            CheckHealthStatus();    
        }

        private void DamageFeedback()
        {
            // TODO: UI feedback that the player took damage
        }

        private void CheckHealthStatus()
        {
            if(healthBar.value == 0){
                // TODO: UI death stuff
            }
        }
    #endregion

    #region Timer
        public void SetTime(float time)
        {
            float seconds = Mathf.FloorToInt(time%60);
            timerText.text = $"{Mathf.FloorToInt(time/60)}:{(seconds >= 10 ? seconds : "0" + seconds)}";
        }
    #endregion

    #region Scoreboard
        // Called if YOUR place changes, or if FIRST place changes
        public void UpdateScoreboard(string firstPlayerName, int firstScore, string secondPlayerName, int secondScore, int thisPlayersPlace)
        {
            scoreboardText1.text = " 1st  " + firstPlayerName + " - " + firstScore + "\n";

            if(thisPlayersPlace == 1){
                scoreboardText2.text += "2nd " + secondPlayerName;
            }
            else{
                string placeStr = " " + thisPlayersPlace;
                if( thisPlayersPlace == 2 ){
                    placeStr = "2nd ";
                }
                else if( thisPlayersPlace == 3 ){
                    placeStr += "rd ";
                }
                else{
                    placeStr += "th ";
                }
                scoreboardText2.text += placeStr + secondPlayerName + " - " + secondScore;
            }
        }
    #endregion

    #region Charge Bar
        public void IncrementCharge(float value)
        {
            chargeBar.value += value;
            CheckSegment();
        }

        public void DecreaseCharge(float value)
        {
            chargeBar.value -= value;
            CheckSegment();
        }

        public void SetCharge(float value)
        {
            chargeBar.value = value;
            CheckSegment();
        }

        private void CheckSegment()
        {
            chargeBarFill.color = chargeBarGradient.Evaluate(chargeBar.value);

            // If we just hit max, activate the UI for it
            if(!fullChargeActive && chargeBar.value >= chargeBar.maxValue){
                // If we got boosted here skipping over another segment, activate third segment UI first
                if(!thirdSegmentActive){
                    ActivateThirdSegment(true);
                }
                ActivateFullChargeUI(true);
            }
            // If we were at max and no longer are, deactivate the max UI
            else if(fullChargeActive && chargeBar.value < chargeBar.maxValue){
                ActivateFullChargeUI(false);
            }

            // If first segment is not currently active and we're now under that threshold, activate it
            if(!firstSegmentActive && chargeBar.value < firstSegmentThreshold){
                ActivateFirstSegment(true);
                return;
            }

            // If second segment is not currently active and we're now under that threshold, activate it
            if(!secondSegmentActive && chargeBar.value < secondSegmentThreshold){
                ActivateSecondSegment(true);
                return;
            }

            // If third segment is not currently active and we're now under that threshold, activate it
            if(!thirdSegmentActive && chargeBar.value < chargeBar.maxValue){
                ActivateThirdSegment(true);
            }
        }

        private void ActivateFirstSegment(bool set)
        {
            // TODO: Set to basic default

            firstSegmentActive = true;

            if(firstSegmentActive){
                secondSegmentActive = false;
                thirdSegmentActive = false;
            }
        }

        private void ActivateSecondSegment(bool set)
        {
            // TODO: Transition into second segment

            secondSegmentActive = true;

            if(secondSegmentActive){
                firstSegmentActive = false;
                thirdSegmentActive = false;
            }
        }

        private void ActivateThirdSegment(bool set)
        {
            // TODO: Transition into third segment

            thirdSegmentActive = set;

            if(thirdSegmentActive){
                firstSegmentActive = false;
                secondSegmentActive = false;
            }
        }

        private void ActivateFullChargeUI(bool set)
        {
            // TODO: Light on fire
            // Juicy effects idk
            // Make the "RAD" text look flashy

            fullChargeActive = set;
        }
    #endregion

    #region Game Over UI
        public void ToggleGameOverUI(bool set)
        {
            gameOverUI.SetActive(set);

            // TODO: Buttons only visible - or at least only interactable - for host
        }
    #endregion
}
