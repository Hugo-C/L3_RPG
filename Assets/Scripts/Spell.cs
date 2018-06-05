using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spell : MovingObject {

    const float MOVE_COEF = 0.15f;  // use to slow down the mouvement speed*
    const float MAX_DISTANCE_FROM_PLAYER = 50f;

    protected GameObject player;
    protected Animator animator;
    protected bool cantMove;
    float horizontal;
    float vertical;
    protected List<string> collidingTag;


    protected override void Start() {
        player = GameObject.Find("Player");
        animator = gameObject.GetComponent<Animator>();
        cantMove = false;
        horizontal = (float)Math.Cos((double)((gameObject.transform.rotation.eulerAngles.z - 90f) * Math.PI / 180));  // we need to add 90 since the default spell look down
        vertical = (float)Math.Sin((double)((gameObject.transform.rotation.eulerAngles.z - 90f) * Math.PI / 180));
        horizontal *= MOVE_COEF; 
        vertical *= MOVE_COEF;
        collidingTag = new List<string> { "BlockingBg", "Ennemy" };
        base.Start();
    }

    // Update is called once per frame
    void Update() {
        if (!cantMove) {
            AttemptMove(horizontal, vertical, collidingTag);
        }
        if (Vector3.Distance(player.transform.position, transform.position) > MAX_DISTANCE_FROM_PLAYER) {
            MyDestroy();
        }
    }

    protected override abstract void OnCantMove(GameObject go);

    protected virtual void MyDestroy() {
        animator.SetTrigger("end");
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length / 2.5f);  // destroy the spell once the animation is complete
    }
}
