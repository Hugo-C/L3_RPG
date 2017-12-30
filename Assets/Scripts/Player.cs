using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject {

    const int ANIM_IDLE = 0;
    const int ANIM_WALK_FRONT = 1;
    const int ANIM_WALK_RIGHT = 2;
    const int ANIM_WALK_BACK = 3;
    const int ANIM_WALK_LEFT = 4;
    const float MOVE_COEF = 0.1f;
    const float SPELL_COOLDOWN = 0.5f;

    private Animator animator;
    List<string> collidingTag;

    public GameObject spell;
    private bool castOnCoolDown;  // use to limit the number of cast per minute

    // Use this for initialization
    protected override void Start () {
        castOnCoolDown = false;
        animator = gameObject.GetComponent<Animator>();
        collidingTag = new List<string> { "BlockingBg" };
        base.Start();
    }
	
	// Update is called once per frame
	void Update () {
        float horizontal = 0f;
        float vertical = 0f;
        float horizontalFire = 0f;
        float verticalFire = 0f;

        horizontal = Input.GetAxisRaw("Horizontal") * MOVE_COEF;
        vertical = Input.GetAxisRaw("Vertical") * MOVE_COEF;
        if (horizontal != 0) {
            vertical = 0;  // the player can't move diagnoly (for now at least)
        }
        HandleAnimation(horizontal, vertical);
        if (horizontal != 0 || vertical != 0) {
            AttemptMove(horizontal, vertical, collidingTag);
        }

        horizontalFire = Input.GetAxisRaw("HorizontalFire");
        verticalFire = Input.GetAxisRaw("VerticalFire");
        if ((horizontalFire != 0 || verticalFire != 0) && !castOnCoolDown) {
            StartCoroutine(CastSpell(horizontalFire, verticalFire));
        }
    }

    private void HandleAnimation(float horizontal, float vertical) {
        if (vertical == 0 && horizontal == 0) {
            animator.SetInteger("walk", ANIM_IDLE);  // we don't have yet an idle animation
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

    private IEnumerator CastSpell(float horizontalFire, float verticalFire) {
        Debug.Log("hf : " + horizontalFire + " vf : " + verticalFire);
        castOnCoolDown = true;
        float z;
        z = (float)(Math.Acos((double) (horizontalFire)) * 180 / Math.PI);
        if ((float)(Math.Asin((double)(verticalFire)) * 180 / Math.PI) > 0) {
            z = -z;
        }
        Vector3 v = new Vector3(0f, 0f, z + 90f);
        Instantiate(spell, gameObject.transform.position, Quaternion.Euler(v));
        yield return new WaitForSeconds(SPELL_COOLDOWN);
        castOnCoolDown = false;
    }

    protected override void OnCantMove(GameObject gameObject) {
        if (gameObject != null) {
            Debug.Log("i can't move, i hit : " + gameObject.name);
        }
    }
}
