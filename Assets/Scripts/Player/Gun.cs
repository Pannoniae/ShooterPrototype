using System;
using UnityEngine;

public class Gun : MonoBehaviour {
    public virtual int magazineSize => 30;
    public virtual int startingMagazines => 4;
    
    [NonSerialized]
    public int ammo; // how much is in the current magazine
    [NonSerialized]
    public int reserveAmmo; // how much ammo is left

    public GameObject bullet;

    public Animation shoot;

    public AudioSource Shootsound;

    public AudioSource Reloadsound;

    public AudioSource Click;

    // Start is called before the first frame update
    void Start() {
        shoot = gameObject.GetComponent<Animation>();
        ammo = magazineSize;
        reserveAmmo = startingMagazines * magazineSize;
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
        if (ammo < magazineSize && reserveAmmo > 0) {
            // don't reload if your mag is full you fucking maggot
            int rawReloadAmount = magazineSize - ammo; // how much ammo we want to reload accounting the current mag in
            int reloadAmount = reserveAmmo >= rawReloadAmount ? rawReloadAmount : reserveAmmo; // reload a full mag if you can, otherwise reload as much as there is
            reserveAmmo -= reloadAmount;
            ammo += reloadAmount;
            GameManager.instance.ammoDisplay.updateAmmo(ammo, reserveAmmo);
            shoot.Play("Reload");
            Reloadsound.Play();
        }
    }

    private void fire() {
        if (ammo > 0) {

            Shootsound.Play();

            ammo--;
            shoot.Play("Fire");
            GameManager.instance.ammoDisplay.updateAmmo(ammo, reserveAmmo);
            var transform1 = GameManager.instance.playerCamera.transform;
            var shotBullet = Instantiate(bullet, transform1.position + transform1.forward * 0.5f, // start the bullet a bit forward so it doesn't jank the player around
                transform1.rotation);
            shotBullet.GetComponent<Rigidbody>().AddForce(transform1.forward * 500);
        }
        else {
            // TODO Lenrok or mario pls make a "click" sound when mag is empty
            Click.Play();
        }
    }
}
