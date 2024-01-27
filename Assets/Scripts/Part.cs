using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


public enum ModelPart
{
    head,
    body,
    legs
}

public enum ModelID
{
    ShovelKnight,
    OldGod,
    Dinosaur,
    Clown,
    Kirby,
    EndEnum
}

public class Part : MonoBehaviour
{

    #region Stats
    [Tooltip("Determined max speed")]
    public float speed;
    [Tooltip("Reduces knockback distance and damage taken")]
    public float weight;
    [Tooltip("Reduces charge cooldown")]
    public float charge_up;
    [Tooltip("Movement acceleration")]
    public float handling;
    [Tooltip("Which model part the Part represents")]
    public ModelPart body_part;
    [Tooltip("Which model the Part belongs to")]
    public ModelID model_id;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
