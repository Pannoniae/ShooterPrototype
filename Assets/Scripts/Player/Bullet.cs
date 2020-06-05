using UnityEngine;
public class Bullet : MonoBehaviour {
    private void OnCollisionEnter(Collision other) {
        // TODO actually hit the player lol
        var target = other.gameObject.GetComponent<Hittable>();
        target?.hit();
        Destroy(this); // kill the bullet
    }
}
