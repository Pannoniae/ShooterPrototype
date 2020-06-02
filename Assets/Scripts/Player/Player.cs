using System;
using UnityEngine;

public class Player : MonoBehaviour {
    
    public int maxHP = 100;
    public int HP;

    void Awake() {
        HP = maxHP;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            damage(10);
        }
    }

    public void damage(int dmg) {
        HP -= dmg;
        GameManager.instance.hpText.text = HP.ToString();
        // TODO fancy damage effect lol
    }
}