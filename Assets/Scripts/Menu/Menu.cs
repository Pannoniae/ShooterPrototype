using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
    public GameObject settingsMenu;

    public void ButtonStart() {
        SceneManager.LoadScene(1);
    }

    public void ButtonSettings() {
        settingsMenu.SetActive(!settingsMenu.activeSelf);
    }

    public void ButtonExit() {
        Application.Quit();
    }
}