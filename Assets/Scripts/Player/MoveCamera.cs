using UnityEngine;

public class MoveCamera : MonoBehaviour {

    void Update() {
        transform.position = GameManager.instance.player.transform.position;
    }
}