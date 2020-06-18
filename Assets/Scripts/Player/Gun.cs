using MLAPI.NetworkedVar;
using System;
using System.Collections;
using UnityEditorInternal;
using UnityEngine;

public class Gun : MonoBehaviour {
    public virtual int magazineSize => 30;
    public virtual int startingMagazines => 4;

    public virtual int RPM => 500;

    public virtual bool fullAuto => true;

    public virtual float RespawnTime => 2f;

    private double shotDelay; // 1 / RPM, how much time to wait between shots
    private double lastShot; // when did the last shot happen

    private int ammo; // how much is in the current magazine
    private int reserveAmmo; // how much ammo is left

    public float DestroyTime = 2f;

    public GameObject bullet;

    public Animation shoot;

    public AudioSource shootSound;

    public AudioSource reloadSound;

    public AudioSource click;

    private bool _thrown = false;
    private bool _active = true;


    // Start is called before the first frame update
    void Start() {
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        _thrown = false;
        _active = true;
        shotDelay = 1.0 * 60 / RPM;
        shoot = gameObject.GetComponent<Animation>();
        ammo = magazineSize;
        reserveAmmo = startingMagazines * magazineSize;
    }

    // Update is called once per frame
    void Update() {
        if (!_active) return;
        if (fullAuto) {
            if (Input.GetButton("Fire1")) {
                if (canFire()) {
                    fire();
                }
            }
        }
        else {
            if (Input.GetButtonDown("Fire1")) {
                if (canFire()) {
                    fire();
                }
            }
        }
        if (Input.GetKeyDown("r")) {
            reload();
        }
    }

    private bool canFire() {
        return Time.time - lastShot > shotDelay;
    }

    private void reload(bool reset = true) {
        if (ammo >= magazineSize || reserveAmmo <= 0) return; // don't reload if your mag is full you fucking maggot
        if (reset) {
            transform.parent = null;
            var rBody = GetComponent<Rigidbody>();
            rBody.isKinematic = false;
            rBody.useGravity = true;
            var angles = GameManager.instance.playerCamera.transform.forward * 10;
            rBody.AddForce(new Vector3(angles.x, angles.y + 6, angles.z), ForceMode.Impulse);
            GetComponent<BoxCollider>().enabled = true;
            _thrown = true;
            StartCoroutine(Spawner());
        }
        else{
            int rawReloadAmount = magazineSize - ammo; // how much ammo we want to reload accounting the current mag in
            int reloadAmount = reserveAmmo >= rawReloadAmount ? rawReloadAmount : reserveAmmo; // reload a full mag if you can, otherwise reload as much as there is
            reserveAmmo -= reloadAmount;
            ammo += reloadAmount;
            GameManager.instance.ammoDisplay.updateAmmo(ammo, reserveAmmo);
            //shoot.Play("Reload");
            reloadSound.Play();
        }
    }

    private void OnCollisionEnter(Collision col) {
        if (col.gameObject.layer != 8) return;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        StartCoroutine(Destroyer());
    }

    private IEnumerator Destroyer() {
        if (!_thrown) yield break;
        _thrown = false;
        _active = false;
        yield return new WaitForSeconds(DestroyTime);
        Destroy(gameObject);
    }

    private IEnumerator Spawner() {
        if (!_thrown) yield break;
        yield return new WaitForSeconds(RespawnTime);
        var t = GameManager.instance.playerCamera.transform;
        Gun newGun = Instantiate(gameObject, t).GetComponent<Gun>();
        newGun.transform.localPosition = new Vector3(0.5f, -0.7f, 0.5f);
        newGun.transform.localRotation = Quaternion.Euler(0, 90.00001f, 0);
        var newRBody = newGun.GetComponent<Rigidbody>();
        newRBody.isKinematic = true;
        newRBody.useGravity = false;
        reload(false);
    }

    private void fire() {
        if (ammo > 0) {
            shootSound.Play();
            ammo--;
            shoot.Play("Fire");
            GameManager.instance.ammoDisplay.updateAmmo(ammo, reserveAmmo);
            var transform1 = GameManager.instance.playerCamera.transform;
            var shotBullet = Instantiate(bullet, transform1.position + transform1.forward * 0.5f, // start the bullet a bit forward so it doesn't jank the player around
                transform1.rotation);
            shotBullet.GetComponent<Rigidbody>().AddForce(transform1.forward * 500);
            // track the shot
            lastShot = Time.time;
        }
        else {
            click.Play();
        }
    }
}
