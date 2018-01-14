using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ennemy : MovingObject {

    const float MOVE_COEF = 0.0075f;

    private GameObject player;
    private int life;
    List<string> collidingTag;

    // Use this for initialization
    protected override void Start () {
        player = GameObject.Find("Player");
        life = 2;
        collidingTag = new List<string> { "Player", "BlockingBg" };
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
        float horizontal;
        float vertical;
        if (player.transform.position.x < transform.position.x) {
            horizontal = -1 * MOVE_COEF;
        } else if (player.transform.position.x > transform.position.x) {
            horizontal = +1 * MOVE_COEF;
        } else {
            horizontal = 0;
        }
        if (player.transform.position.y < transform.position.y) {
            vertical = -1 * MOVE_COEF;
        } else if (player.transform.position.y > transform.position.y) {
            vertical = +1 * MOVE_COEF;
        } else {
            vertical = 0;
        }

        AttemptMove(horizontal, vertical, collidingTag);
    }


    protected override void OnCantMove(GameObject gameObject) {
        if(gameObject != null && gameObject.tag == "Player") {
            Player player = gameObject.GetComponent<Player>();
            player.Hit();
        }
    }

    public void Hit() {
        life--;
        if(life <= 0) {
            Destroy(gameObject);
        }
    }
}
