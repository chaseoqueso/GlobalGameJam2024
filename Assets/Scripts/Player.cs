using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{

    public int id;
    public int score;
    public bool is_dead;
    public int respawn_timer;
    public Character character;

    public Player(int player_id)
    {
        this.id = player_id;
        this.score = 0;
        this.is_dead = false;
        this.respawn_timer = 5;
        this.character = null;  
    }

    void AssignCharacter(Character character)
    {
        this.character = character;
    }
}
