using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Player : MonoBehaviour {
    
    public int maxHP = 100;
    public int HP;
    public PlayerMovement toDisableScript;
    public GameObject deadObject;

    void Awake() {
        HP = maxHP;
    }

    void Start() {
        toDisableScript = GameManager.instance.player.GetComponent<PlayerMovement>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            damage(10);
        }
        if (HP <= 0) {
            deadObject.SetActive(true);
            toDisableScript.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void damage(int dmg) {
        HP -= dmg;
        if (HP < 0) HP = 0;
        GameManager.instance.hpText.text = HP.ToString();
        // TODO fancy damage effect lol
    }
}