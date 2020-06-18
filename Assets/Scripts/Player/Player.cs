using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

public class Player : MonoBehaviour, Hittable {
    
    public int maxHP = 100;
    public int HP;
    public GameObject crossHair;
    public GameObject HPCount;
    public GameObject ammoCount;
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
    }

    public void damage(int dmg) {
        HP -= dmg;
        GameManager.instance.hpText.text = HP.ToString();
        if (HP <= 0) {
            GameManager.instance.postProcessing.profile.TryGet(out Vignette vignette);
            vignette.active = true;
            crossHair.SetActive(false);
            HPCount.SetActive(false);
            ammoCount.SetActive(false);
            GameManager.instance.deadObject.SetActive(true);
            GameManager.instance.crosshair.enabled = false;
            playerMovement.disabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // TODO fancy damage effect lol
    }

    public void hit() {
        damage(new Random().Next(15, 35));
    }
}