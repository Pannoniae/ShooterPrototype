using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour {
    
    public int maxHP = 100;
    public int HP;
    public PlayerMovement playerMovement;

    void Awake() {
        HP = maxHP;
    }

    void Start() {
        playerMovement = GameManager.instance.player.GetComponent<PlayerMovement>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            damage(10);
        }
        if (HP <= 0) {
            Vignette vignette;
            GameManager.instance.postProcessing.profile.TryGet(out vignette);
            vignette.active = true;
            GameManager.instance.deadObject.SetActive(true);
            GameManager.instance.crosshair.enabled = false;
            playerMovement.disabled = true;
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