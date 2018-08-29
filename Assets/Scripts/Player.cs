using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MovingObject, IPausable {
    private const float MoveCoef = 0.1f;
    private const float SpellCooldown = 0.5f;
    private const float SpiralSpellCooldown = 1f;
    private const float InvunerableTime = 1f;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private List<string> _collidingTag;  // tag the player can collide with
    private Text _coinCount;
    private AudioSource _audioSourceFootsteps;

    public GameObject Spell;
    public GameObject Heart;
    public bool Invulnerable;  // use to limit the number of hit taken per minute

    private bool _spellOnCoolDown;  // use to limit the number of cast per minute
    private bool _spiralSpellOnCoolDown;
    private int _coins;
    private int _life;

    private bool _gamePaused;

    public int Coins {
        get {
            return _coins;
        }
        
        set {
            _coins = value;
            _coinCount.text = _coins.ToString();
        }
    }

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
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collidingTag = new List<string> { "BlockingBg" };
        _coinCount = GameObject.Find("CoinCount").GetComponent<Text>();
        _audioSourceFootsteps = GetComponent<AudioSource>();
        _spellOnCoolDown = false;
        _gamePaused = false;
        LevelManager levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        if (levelManager.LevelCompleted == 0) {
            Life = 3;
            Coins = 0;
        } else {
            Life = PlayerPrefs.GetInt("life", 3);
            Coins = PlayerPrefs.GetInt("coins", 0);
        }
        StartCoroutine(MakeInvunerable(InvunerableTime * 2f));
        base.Start();
    }
	
	// Update is called once per frame
	void Update () {
	    if (!_gamePaused) {
	        float horizontal = Input.GetAxisRaw("Horizontal") * MoveCoef;
	        float vertical = Input.GetAxisRaw("Vertical") * MoveCoef;
	        if (horizontal != 0) {
	            vertical = 0f; // the player can't move diagnoly (for now at least)
	        }

	        if (horizontal != 0 || vertical != 0) {
	            AttemptMove(horizontal, vertical, _collidingTag);
	            if (!_audioSourceFootsteps.isPlaying)
	                _audioSourceFootsteps.Play();
	        }
	        else {
	            if (_audioSourceFootsteps.isPlaying)
	                _audioSourceFootsteps.Pause();
	        }

	        float horizontalFire = Input.GetAxisRaw("HorizontalFire");
	        float verticalFire = Input.GetAxisRaw("VerticalFire");
	        HandleAnimation(horizontal, vertical, horizontalFire);
	        if ((horizontalFire != 0 || verticalFire != 0) && !_spellOnCoolDown) {
	            StartCoroutine(CastSpell(horizontalFire, verticalFire, true));
	        }

	        if (Input.GetKeyDown(KeyCode.Space) && !_spiralSpellOnCoolDown) {
	            StartCoroutine(CastSpiralSpell());
	        }

	        if (Input.GetKeyDown(KeyCode.E)) { // Handle interraction
	            var interractiveGameObjects = GameObject.FindGameObjectsWithTag("Interractive");
	            Debug.Log("nb : " + interractiveGameObjects.Length);
	            foreach (var go in interractiveGameObjects) {
	                if (Vector3.Distance(go.transform.position, transform.position) < 5) {
	                    var interractiveGameObject = go.GetComponent(typeof(MonoBehaviour)) as IInterractiveGameObject;
	                    interractiveGameObject?.Interract();
	                }
	            }
	        }
	    }
	}

    private void HandleAnimation(float horizontal, float vertical, float horizontalFire) {
        if (vertical == 0 && horizontal == 0) {
            _animator.SetBool("walking", false);  // we don't have yet an idle animation
            
            // the player face the direction where he is firing
            if (horizontalFire < 0) {
                _spriteRenderer.flipX = true;
            } else if (horizontalFire > 0) {
                _spriteRenderer.flipX = false;
            }
        } else {
            _animator.SetBool("walking", true);  // we don't have yet an idle animation
            
            // the player face the direction where he is going
            if (horizontal > 0) {
                _spriteRenderer.flipX = false;
            } else if (horizontal < 0) {
                _spriteRenderer.flipX = true;
            }
        }
    }

    // Cast spell in all directions
    private IEnumerator CastSpiralSpell() {
        _spiralSpellOnCoolDown = true;
        for (float i = -1f; i <= 1f; i += UnityEngine.Random.Range(0.25f, 0.5f)) {
            // we fire in both direction (up and down)
            if(i != -1f) {
                StartCoroutine(CastSpell(i, -1f, false));
                StartCoroutine(CastSpell(i, 1f, false));
            } else {
                StartCoroutine(CastSpell(i, -1f, false));
                StartCoroutine(CastSpell(-i, 1f, false));
            }
        }
        yield return new WaitForSeconds(SpellCooldown);
        _spiralSpellOnCoolDown = false;
    }

    /**
     * Cast a single spell
     * horizontalFire and verticalFire indicate the orientation
     * the cooldown is handled if castedByPlayer is true   
     */
    private IEnumerator CastSpell(float horizontalFire, float verticalFire, bool castedByPlayer) {
        if (castedByPlayer) {
            _spellOnCoolDown = true;
        }

        float z = (float)(Math.Acos((double) (horizontalFire)) * 180 / Math.PI);
        if ((float)(Math.Asin((double)(verticalFire)) * 180 / Math.PI) > 0) {
            z = -z;
        }
        Vector3 v = new Vector3(0f, 0f, z + 90f);
        Instantiate(Spell, gameObject.transform.position, Quaternion.Euler(v));
        yield return new WaitForSeconds(SpellCooldown);
        if(castedByPlayer) {
            _spellOnCoolDown = false;
        }
    }

    protected override void OnCantMove(GameObject go) {
        if (go != null) {
            //Debug.Log("i can't move, i hit : " + gameObject.name);
        }
    }

    public void Hit() {
        StartCoroutine(MyHit());
    }

    private IEnumerator MyHit() {
        if (!Invulnerable) {
            Invulnerable = true;
            Life--;
            if(Life == 0) {
                Coins = 0;
                Save();
                LevelManager levelManager = LevelManager.Instance;
                levelManager.LoadScene("endGame");
            }
            yield return new WaitForSeconds(InvunerableTime);
            Invulnerable = false;
        }
    }

    private IEnumerator MakeInvunerable(float delay) {  // TODO check if the player isn't already invulnerable
        Invulnerable = true;
        yield return new WaitForSeconds(delay);
        Invulnerable = false;
    }

    public void PickCoin(int value) {
        Coins += value;
    }

    public void Save() {
        PlayerPrefs.SetInt("coins", Coins);
        PlayerPrefs.SetInt("life", Life);
    }
    
    private void DisplayLife(int lifeToDisplay) {
        RemoveLife(); // barbaric suppression
        //GameObject.Finds("Heart (clone)");
        Vector3 original = new Vector3(-0.35f,0.75f) + gameObject.transform.position;
        for(int i=0; i < lifeToDisplay; i++) {
            Vector3 shift = new Vector3(0.35f*i, 0f);
            GameObject go = Instantiate(Heart, original + shift , Quaternion.identity);
            go.name = Heart.name;
            go.transform.position = original + shift;
            go.transform.SetParent(gameObject.transform);
        }
    }

    private void RemoveLife() {
        foreach (Transform child in transform) {
            if (child.gameObject.name == Heart.name)
                Destroy(child.gameObject);
        }
    }

    private void Reset() {
        Life = 3;
        Coins = 0;
        Save();
    }

    public void OnPauseGame() {
        _gamePaused = true;
    }

    public void OnResumeGame() {
        _gamePaused = false;
    }
}
