using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ButtonFixer : MonoBehaviour, IPointerEnterHandler, IDeselectHandler//, ISelectHandler, IPointerClickHandler, ISubmitHandler
{
    // [SerializeField] private bool playSFXOnHover = true;
    // [SerializeField] private bool playSFXOnClick = true;
    
    // [FMODUnity.EventRef] public string buttonClickSFXOverride;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!EventSystem.current.alreadySelecting){
            EventSystem.current.SetSelectedGameObject(this.gameObject);
            // TriggerHoverSFX();
        }
    }

    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     TriggerClickSFX();
    // }

    // public void OnSelect(BaseEventData eventData)
    // {
    //     TriggerHoverSFX();
    // }

    public void OnDeselect(BaseEventData eventData)
    {
        this.GetComponent<Selectable>().OnPointerExit(null);
    }

    // public void OnSubmit(BaseEventData eventData)
    // {
    //     TriggerClickSFX();
    // }

    // private void TriggerHoverSFX()
    // {
    //     if(playSFXOnHover)
    //         AudioManager.Instance.PlaySFX(AudioManager.SFX.ButtonHover);
    // }

    // private void TriggerClickSFX()
    // {
    //     if(playSFXOnClick){
    //         if(buttonClickSFXOverride != ""){
    //             AudioManager.Instance.PlaySFX(buttonClickSFXOverride);
    //         }
    //         else{
    //             AudioManager.Instance.PlaySFX(AudioManager.SFX.ButtonClick);
    //         }            
    //     }
    // }
}
