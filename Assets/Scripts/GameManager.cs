﻿using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    // the actual instance lol
    private static GameManager _instance;
    
    // references
    public GameObject player;

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
    }
}