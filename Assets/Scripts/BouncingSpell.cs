using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingSpell : Spell {
    private const float TimeWithoutInteraction = 0.1f;


    protected override void Start() {
        base.Start();
        CollidingTag = new List<string>();
        StartCoroutine(AllowInteraction());
    }

    /* allow the spell to interact with his environnement after a short time (TIME_WITHOUT_INTERACTION) */
    private IEnumerator AllowInteraction() {
        yield return new WaitForSeconds(TimeWithoutInteraction);
        CollidingTag = new List<string> { "BlockingBg", "Ennemy", "Player" };
    }

    protected override void OnCantMove(GameObject go) {
        CantMove = true;
        if (go != null) {
            if (go.CompareTag("Ennemy")) {
                go.GetComponent<Ennemy>().Hit();
            } else if (go.CompareTag("Player")) {
                go.GetComponent<Player>().Hit();
            }
        }
        MyDestroy();
    }
}