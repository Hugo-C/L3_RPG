using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject {


    const float STEEP = 0.5f;

    const int ANIM_IDLE = 0;
    const int ANIM_WALK_FRONT = 1;
    const int ANIM_WALK_RIGHT = 2;
    const int ANIM_WALK_BACK = 3;
    const int ANIM_WALK_LEFT = 4;

    private Animator animator;

    // Use this for initialization
    protected override void Start () {
        animator = gameObject.GetComponent<Animator>();
        base.Start();
    }
	
	// Update is called once per frame
	void Update () {
        int horizontal = 0;
        int vertical = 0;

        horizontal = (int) (Input.GetAxisRaw("Horizontal"));
        vertical = (int) (Input.GetAxisRaw("Vertical"));

        if (horizontal != 0) {
            vertical = 0;  // the player can't move diagnoly (for now at least)
        }

        HandleAnimation(horizontal, vertical);
        if (horizontal != 0 || vertical != 0) {
            AttemptMove<Wall>(horizontal, vertical);
        }
	}

    private void HandleAnimation(int horizontal, int vertical) {
        if (vertical == 0 && horizontal == 0) {
            animator.SetInteger("walk", ANIM_IDLE); // we don't have yet an idle animation
        } else if (vertical > 0) {
            animator.SetInteger("walk", ANIM_WALK_FRONT);
        } else if (horizontal > 0) {
            animator.SetInteger("walk", ANIM_WALK_RIGHT);
        } else if (vertical < 0) {
            animator.SetInteger("walk", ANIM_WALK_BACK);
        } else if (horizontal < 0) {
            animator.SetInteger("walk", ANIM_WALK_LEFT);
        } else {
            Debug.Log("error in HandleAnimation : InvalidValue");
        }
    }

    protected override void AttemptMove<T>(int xDir, int yDir) {
        RaycastHit2D hit1;
        RaycastHit2D hit2;
        if (Move(xDir, yDir, out hit1, out hit2)) {
            //Debug.Log("i moved");
        }
    }

    protected override void OnCantMove<T>(T component) {
        Debug.Log("I'm blocked !");
    }
    
}
