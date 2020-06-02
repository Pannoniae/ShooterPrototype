﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    // the actual instance lol
    private static GameManager _instance;
    
    // references
    public GameObject player;
    public TextMeshProUGUI hpText;

    public static GameManager instance {
        get {
            if (_instance != null) {
                return _instance;
            }
            _instance = new GameObject("Game Manager").AddComponent<GameManager>();
            return _instance;
        }
    }

    public void Awake() {
        _instance = this;
        Application.targetFrameRate = 60;
    }
}