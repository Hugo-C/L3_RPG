using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingSpell : Spell {

    const float TIME_WITHOUT_INTERACTION = 0.1f;


    protected override void Start() {
        base.Start();
        collidingTag = new List<string> { };
        StartCoroutine(AllowInteraction());
    }

    /* allow the spell to interact with his environnement after a short time (TIME_WITHOUT_INTERACTION) */
    private IEnumerator AllowInteraction() {
        yield return new WaitForSeconds(TIME_WITHOUT_INTERACTION);
        collidingTag = new List<string> { "BlockingBg", "Ennemy", "Player" };
    }

    protected override void OnCantMove(GameObject go) {
        animator.SetTrigger("end");
        if (go != null) {
            if (go.tag == "Ennemy") {
                Ennemy ennemy = go.GetComponent<Ennemy>();
                ennemy.Hit();
            } else if (go.tag == "Player") {
                Player player = go.GetComponent<Player>();
                player.Hit();
            }
        }

        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);  // destroy the spell once the animation is complete
        cantMove = true;
    }
}