using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {


    const int IDLE = 0;
    const int WALK_FRONT = 1;
    const int WALK_RIGHT = 2;
    const int WALK_BACK = 3;
    const int WALK_LEFT = 4;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Animator a = gameObject.GetComponent<Animator>();
        if (Input.GetKey(KeyCode.Z)) {
            a.SetInteger("walk", WALK_FRONT);
        } else if (Input.GetKey(KeyCode.D)) {
            a.SetInteger("walk", WALK_RIGHT);
        } else if (Input.GetKey(KeyCode.S)) {
            a.SetInteger("walk", WALK_BACK);
        } else if (Input.GetKey(KeyCode.Q)) {
            a.SetInteger("walk", WALK_LEFT);
        } else {
            a.SetInteger("walk", IDLE);
        }
	}
}
