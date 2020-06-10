using MLAPI;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour {
    // the actual instance lol
    private static GameManager _instance;
    
    // references
    public GameObject player;
    public Camera playerCamera;
    public TextMeshProUGUI hpText;
    public GameObject deadObject;
    public Volume postProcessing;
    public TextMeshProUGUI crosshair;
    public AmmoDisplay ammoDisplay;
    
    // DONT TOUCH
    public NetworkingManager networkingManager;

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
        //Application.targetFrameRate = 60;
    }
}