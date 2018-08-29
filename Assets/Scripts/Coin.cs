using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {
	
	private const int Value = 1;
	
	public AudioClip[] Sfxs;

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			Player player = other.GetComponent<Player>();
			player.PickCoin(Value);
			StartCoroutine(HandleDeath());
		}
	}

	private IEnumerator HandleDeath() {
		// we disable the collider to prevent the player from picking the coin twice
		BoxCollider2D bc = GetComponent<BoxCollider2D>();
		bc.enabled = false;
		
		// we play a random sound
		MyRandom rnd = new MyRandom();
		AudioClip pick = Sfxs[rnd.Next(Sfxs.Length)];
		PlaySfx(pick);
		
		// we hide the gameobject
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		sr.enabled = false;
		
		Light myLight = gameObject.GetComponentInChildren<Light>();
		myLight.enabled = false;
		
		yield return new WaitForSeconds(pick.length);
		Destroy(gameObject);
	}

	private void PlaySfx(AudioClip audioClip) {
		AudioSource audioSource = GetComponent<AudioSource>();
		audioSource.clip = audioClip;
		// we set the pitch between 0.95 and 1.05
		audioSource.pitch = 0.95f + (float) new MyRandom().NextDouble() * 0.1f;
		audioSource.Play();
	}
}
