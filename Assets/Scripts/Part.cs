using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("Which body part the Part represents")]
    public BodyPart body_part;


    public enum BodyPart  
    {
        head,
        body,
        legs
    }
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
