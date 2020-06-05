using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public TMP_InputField usernameField;

    public void Awake() {
        if (instance == null)
            instance = this;
        else if (instance != null) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ConnectToServer() {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        GameManager.instance.player.GetComponent<PlayerMovement>().enabled = true;
        Client.instance.ConnectToServer();
    }

}
