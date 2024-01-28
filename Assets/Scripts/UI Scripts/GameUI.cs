using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    void Awake()
    {
        if (Instance != null && Instance != this){ 
            Destroy(this); 
        } 
        else{ 
            Instance = this;
        }

        SetCharge(0f);
    }

    #region Timer
        public void SetTime()
        {

        }
    #endregion

    #region Scoreboard
        public void UpdateScoreboard()
        {
            
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
            chargeBarFill.color = chargeBarGradient.Evaluate(chargeBar.normalizedValue);

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
}
