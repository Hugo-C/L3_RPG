using System.Collections.Generic;
using UnityEngine;

public class Ennemy : MovingObject {

    public GameObject Coin;
    private const float MoveCoef = 0.0075f;

    private GameObject _player;
    private int _life;
    private bool _isAgro;
    private List<string> _collidingTag;

    // Use this for initialization
    protected override void Start () {
        _isAgro = false;
        _player = GameObject.Find("Player");
        _life = 2;
        _collidingTag = new List<string> { "Player", "BlockingBg" };
        InitAnimation();
        InitSfx();
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
        float horizontal;
        float vertical;
        if (_isAgro || Vector2.Distance(_player.transform.position, transform.position) < 5) {
            if (_player.transform.position.x < transform.position.x) {
                horizontal = -1 * MoveCoef;
            } else if (_player.transform.position.x > transform.position.x) {
                horizontal = +1 * MoveCoef;
            } else {
                horizontal = 0;
            }
            if (_player.transform.position.y < transform.position.y) {
                vertical = -1 * MoveCoef;
            } else if (_player.transform.position.y > transform.position.y) {
                vertical = +1 * MoveCoef;
            } else {
                vertical = 0;
            }
            AttemptMove(horizontal, vertical, _collidingTag);
            if (!_isAgro) {
                _isAgro = true;
            }
        }
    }

    private void InitAnimation() {
        var animator = gameObject.GetComponent<Animator>();
        MyRandom rnd = new MyRandom();
        AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);
        animator.Update(asi.length / (float)rnd.NextDouble());
    }

    /// <summary>
    /// Init the sound effect of the ennemy in order to be desynchonised with the other ennemy
    /// </summary>
    private void InitSfx() {
        var myAudioSource = gameObject.GetComponent<AudioSource>();
        if (myAudioSource == null) {
            Debug.LogWarning("no audio source attached to the ennemy");
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
            Instantiate(Coin, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
