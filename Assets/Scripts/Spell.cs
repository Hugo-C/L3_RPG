using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spell : MovingObject {
    private const float MoveCoef = 0.15f;  // use to slow down the mouvement speed*
    private const float MaxDistanceFromPlayer = 50f;

    protected GameObject Player;
    protected Animator Animator;
    protected bool CantMove;
    private float _horizontal;
    private float _vertical;
    protected List<string> CollidingTag;


    protected override void Start() {
        Player = GameObject.Find("Player");
        Animator = gameObject.GetComponent<Animator>();
        CantMove = false;
        _horizontal = (float)Math.Cos((gameObject.transform.rotation.eulerAngles.z - 90f) * Math.PI / 180);  // we need to add 90 since the default spell look down
        _vertical = (float)Math.Sin((gameObject.transform.rotation.eulerAngles.z - 90f) * Math.PI / 180);
        _horizontal *= MoveCoef; 
        _vertical *= MoveCoef;
        CollidingTag = new List<string> { "BlockingBg", "Ennemy" };
        base.Start();
    }

    // Update is called once per frame
    void Update() {
        if (!CantMove) {
            AttemptMove(_horizontal, _vertical, CollidingTag);
        }
        if (Player != null && Vector3.Distance(Player.transform.position, transform.position) > MaxDistanceFromPlayer) {
            MyDestroy();
        }
    }

    protected abstract override void OnCantMove(GameObject go);

    protected virtual void MyDestroy() {
        Animator.SetTrigger("end");
        Destroy(gameObject, Animator.GetCurrentAnimatorStateInfo(0).length / 2.5f);  // destroy the spell once the animation is complete
    }
}
