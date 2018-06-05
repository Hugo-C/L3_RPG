using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpell : Spell {

    public GameObject bouncingSpell;

    protected override void OnCantMove(GameObject go) {
        cantMove = true;
        if (go != null) {
            if (go.tag == "Ennemy") {
                Ennemy ennemy = go.GetComponent<Ennemy>();
                ennemy.Hit();
            } else if (go.tag == "BlockingBg") {
                Vector3 v = new Vector3(0f, 0f, gameObject.transform.eulerAngles.z + 180f);
                Instantiate(bouncingSpell, gameObject.transform.position, Quaternion.Euler(v));
            }
        }
        MyDestroy();  // destroy the spell once the animation is complete
    }
}
