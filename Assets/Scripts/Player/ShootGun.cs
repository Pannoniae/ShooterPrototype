using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootGun : MonoBehaviour
{
    public Animation shoot;
    // Start is called before the first frame update
    void Start()
    {
        shoot = gameObject.GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            shoot.Play("Fire");
        else if (Input.GetKeyDown("r"))
            shoot.Play("Reload");
    }
}
