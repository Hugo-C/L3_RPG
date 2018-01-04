using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MovingObject {

    const float MOVE_COEF = 0.15f;  // use to slow down the mouvement speed
    Animator animator;
    bool cantMove;
    float horizontal;
    float vertical;
    List<string> collidingTag;


    protected override void Start() {
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
    }
    protected override void OnCantMove(GameObject go) {
        animator.SetTrigger("end");
        if(go != null && go.tag == "Ennemy") {
            Ennemy ennemy = go.GetComponent<Ennemy>();
            ennemy.Hit();
        }
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);  // destroy the spell once the animation is complete
        cantMove = true;
    }
}
