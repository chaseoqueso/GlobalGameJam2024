using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    #region Parts and Variable Stats
    public Part head;
    public Part body;
    public Part legs;

    private float current_health;
    private float max_health;
    private float max_speed;  //Determined max speed
    private float weight;  //Reduces knockback distance and damage taken
    private float attack;  //Increases knockback distance and damage done
    private float handling;  //Movement acceleration
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        this.current_health = this.max_health;
        this.max_speed = this.head.speed + this.body.speed + this.legs.speed;
        this.weight = this.head.weight + this.body.weight + this.legs.weight;
        this.attack = this.head.attack + this.body.attack + this.legs.attack;
        this.handling = this.head.handling + this.body.handling + this.legs.handling;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onCollisionEnter3D(Collision collision)
    {

    }
}
