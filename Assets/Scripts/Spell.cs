using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MovingObject {

    const float MOVE_COEF = 0.2f;  // use to slow down the mouvement speed
    Animator animator;
    bool fading;
    float horizontal;
    float vertical;
    List<string> collidingTag;


    protected override void Start() {
        animator = gameObject.GetComponent<Animator>();
        fading = false;
        horizontal = (float)Math.Cos((double)((gameObject.transform.rotation.eulerAngles.z - 90f) * Math.PI / 180));  // we need to add 90 since the default spell look down
        vertical = (float)Math.Sin((double)((gameObject.transform.rotation.eulerAngles.z - 90f) * Math.PI / 180));
        horizontal *= MOVE_COEF; 
        vertical *= MOVE_COEF;
        collidingTag = new List<string> { "BlockingBg" };
        base.Start();
    }

    protected override void OnCantMove(GameObject go) {
        //Debug.Log("spell down by : " + go.name);
        animator.SetTrigger("end");
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        fading = true;
    }

    // Update is called once per frame
    void Update () {
        if (!fading) {
            AttemptMove(horizontal, vertical, collidingTag);
        }
	}
}
