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
    const float INVUNERABLE_TIME = 1f;

    private Animator animator;
    List<string> collidingTag;  // tag the player can collide with

    public GameObject spell;
    public GameObject heart;
    private bool castOnCoolDown;  // use to limit the number of cast per minute
    private bool invulnerable;  // use to limit the number of hit taken per minute
    private int _life;

    public int Life {
        get {
            return _life;
        }

        set {
            _life = value;
            DisplayLife(_life);
        }
    }

    // Use this for initialization
    protected override void Start () {
        animator = gameObject.GetComponent<Animator>();
        collidingTag = new List<string> { "BlockingBg" };
        castOnCoolDown = false;
        invulnerable = false;
        Life = 3;
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

        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("je cast une spirale");
            CastSpiralSpell();
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

    // Cast spell in all directions
    private void CastSpiralSpell() {
        int cpt = 0;
        for(float i = -0.5f; i <= 0.5f; i += UnityEngine.Random.Range(0.25f, 0.5f)) {
            for(float j = -0.5f; j <= 0.5f; j += UnityEngine.Random.Range(0.25f, 0.5f)) {
                cpt++;
                Debug.Log("cpt de spell : " + cpt + " i : " + i + " j : " + j);
                StartCoroutine(CastSpell(i, j));
            }
        }
    }

    private IEnumerator CastSpell(float horizontalFire, float verticalFire) {
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
            //Debug.Log("i can't move, i hit : " + gameObject.name);
        }
    }

    public void Hit() {
        //StartCoroutine(MyHit());
    }

    private IEnumerator MyHit() {
        if (!invulnerable) {
            invulnerable = true;
            Life--;
            if(Life == 0) {
                LevelManager levelManager = LevelManager.instance;
                levelManager.LoadScene("endGame");
            }
            yield return new WaitForSeconds(INVUNERABLE_TIME);
            invulnerable = false;
        }
    }

    public void DisplayLife(int lifeToDisplay) {
        RemoveLife(); // barbaric suppression
        //GameObject.Finds("Heart (clone)");
        Vector3 original = new Vector3(-0.35f, 1f) + gameObject.transform.position;
        for(int i=0; i < lifeToDisplay; i++) {
            Vector3 shift = new Vector3(0.35f*i, 0f);
            GameObject go = Instantiate(heart, original + shift , Quaternion.identity);
            //go.transform.parent = gameObject.transform;
            go.transform.SetParent(gameObject.transform);
        }
    }

    public void RemoveLife() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
}
