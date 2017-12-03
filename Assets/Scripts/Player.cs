using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {


    const int IDLE = 0;
    const int WALK_FRONT = 1;
    const int WALK_RIGHT = 2;
    const int WALK_BACK = 3;
    const int WALK_LEFT = 4;

    const float WALK_STEEP = 0.1f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Animator a = gameObject.GetComponent<Animator>();
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow)) {
            a.SetInteger("walk", WALK_FRONT);
            Vector3 newPosition = gameObject.transform.position;
            newPosition.y += WALK_STEEP;
            gameObject.transform.position = newPosition;
        } else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            a.SetInteger("walk", WALK_RIGHT);
            Vector3 newPosition = gameObject.transform.position;
            newPosition.x += WALK_STEEP;
            gameObject.transform.position = newPosition;
        } else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            a.SetInteger("walk", WALK_BACK);
            Vector3 newPosition = gameObject.transform.position;
            newPosition.y -= WALK_STEEP;
            gameObject.transform.position = newPosition;
        } else if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow)) {
            a.SetInteger("walk", WALK_LEFT);
            Vector3 newPosition = gameObject.transform.position;
            newPosition.x -= WALK_STEEP;
            gameObject.transform.position = newPosition;
        } else {
            a.SetInteger("walk", IDLE);
        }
	}
}
