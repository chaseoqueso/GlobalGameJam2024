using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public ModelPart bodyPart;
    [Tooltip("Which model the Part belongs to")]
    public ModelID modelId;
    #endregion

    static public float[] GetCombinedStats(Part h, Part t, Part l)
    {
        float speed = (h.speed + t.speed + l.speed) / 3;
        float weight = (h.weight + t.weight + l.weight) / 3;
        float charge_up = (h.charge_up + t.charge_up + l.charge_up) / 3;
        float handling = (h.handling + t.handling + l.handling) / 3;

        float[] stats = { speed, weight, charge_up, handling };
        return stats;
    }

}
