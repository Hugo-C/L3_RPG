using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpell : Spell {

    public GameObject bouncingSpell;

    protected override void OnCantMove(GameObject go) {
        animator.SetTrigger("end");
        if (go != null) {
            if (go.tag == "Ennemy") {
                Ennemy ennemy = go.GetComponent<Ennemy>();
                ennemy.Hit();
            } else if (go.tag == "BlockingBg") {
                Vector3 v = new Vector3(0f, 0f, gameObject.transform.eulerAngles.z + 180f);
                print(gameObject.transform.eulerAngles.z);
                Instantiate(bouncingSpell, gameObject.transform.position, Quaternion.Euler(v));
            }
        }

        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);  // destroy the spell once the animation is complete
        cantMove = true;
    }
}
