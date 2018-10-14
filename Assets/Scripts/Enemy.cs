using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

    public GameObject Loot;
    protected const float MoveCoef = 0.0075f;

    protected GameObject Player;
    private int _life;
    [SerializeField]
    protected bool IsAgro;
    protected List<string> CollidingTag;

    // Use this for initialization
    protected override void Start () {
        Player = GameObject.Find("Player");
        _life = 2;
        CollidingTag = new List<string> { "Player", "BlockingBg" };
        InitAnimation();
        InitSfx();
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
        if (!IsAgro && Vector2.Distance(Player.transform.position, transform.position) < 5f)
            IsAgro = true;

	    float horizontal, vertical;
	    NextMove(out horizontal, out vertical);
	    if(horizontal != 0f || vertical != 0f)
	        AttemptMove(horizontal, vertical, CollidingTag);
    }

    private void NextMove(out float horizontal, out float vertical) {
        if (IsAgro) {
            if (Player.transform.position.x < transform.position.x) {
                horizontal = -1 * MoveCoef;
            } else if (Player.transform.position.x > transform.position.x) {
                horizontal = +1 * MoveCoef;
            } else {
                horizontal = 0f;
            }
            if (Player.transform.position.y < transform.position.y) {
                vertical = -1 * MoveCoef;
            } else if (Player.transform.position.y > transform.position.y) {
                vertical = +1 * MoveCoef;
            } else {
                vertical = 0f;
            }
        } else {
            horizontal = 0f;
            vertical = 0f;
        }
    }

    private void InitAnimation() {
        var animator = gameObject.GetComponent<Animator>();
        MyRandom rnd = new MyRandom();
        AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);
        animator.Update(asi.length / (float)rnd.NextDouble());
    }

    /// <summary>
    /// Init the sound effect of the enemy in order to have an offset with the other enemy
    /// </summary>
    private void InitSfx() {
        var myAudioSource = gameObject.GetComponent<AudioSource>();
        if (myAudioSource == null) {
            Debug.LogWarning("no audio source attached to the enemy");
        } else {
            MyRandom rnd = new MyRandom();
            myAudioSource.time = (float)rnd.NextDouble() * myAudioSource.clip.length;
            myAudioSource.Play();
        }
    }

    protected override void OnCantMove(GameObject go) {
        if(go != null && go.CompareTag("Player")) {
            go.GetComponent<Player>().Hit();
        }
    }

    public void Hit() {
        _life--;
        if(_life <= 0) {
            if (Loot != null)
                Instantiate(Loot, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
