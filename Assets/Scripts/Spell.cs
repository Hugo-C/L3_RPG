using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MovingObject {

    Animator animator;
    bool fading;

    protected override void Start() {
        animator = gameObject.GetComponent<Animator>();
        fading = false;
        base.Start();
    }

    protected override void OnCantMove(GameObject gameObject) {
        animator.SetTrigger("end");
        Destroy(this.gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        fading = true;
    }

    // Update is called once per frame
    void Update () {
        if (!fading) {
            AttemptMove(0, -1);
        }
	}
}
