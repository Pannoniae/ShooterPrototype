using UnityEngine;

public class Gun : MonoBehaviour {
    public virtual int magazineSize => 30;
    public virtual int startingMagazines => 4;
    public int ammo; // how much is in the current magazine
    public int reserveMagazines; // how many magazines are left

    public GameObject bullet;

    public Animation shoot;

    // Start is called before the first frame update
    void Start() {
        shoot = gameObject.GetComponent<Animation>();
        ammo = magazineSize;
        reserveMagazines = startingMagazines;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Fire1")) {
            fire();
        }
        else if (Input.GetKeyDown("r")) {
            reload();
        }
    }

    private void reload() {
        if (ammo < magazineSize) { // don't reload if your mag is full you fucking maggot
            if (reserveMagazines > 0) {
                reserveMagazines--;
                ammo = magazineSize;
                GameManager.instance.ammoDisplay.updateAmmo(ammo, reserveMagazines * magazineSize);
                shoot.Play("Reload");
            }
        }
    }

    private void fire() {
        if (ammo > 0) {
            ammo--;
            shoot.Play("Fire");
            GameManager.instance.ammoDisplay.updateAmmo(ammo, reserveMagazines * magazineSize);
            var transform1 = GameManager.instance.playerCamera.transform;
            var shotBullet = Instantiate(bullet, transform1.position + transform1.forward * 0.5f, // start the bullet a bit forward so it doesn't jank the player around
                transform1.rotation);
            shotBullet.GetComponent<Rigidbody>().AddForce(transform1.forward * 500);
        }
        else {
            // TODO Lenrok or mario pls make a "click" sound when mag is empty
        }
    }
}
