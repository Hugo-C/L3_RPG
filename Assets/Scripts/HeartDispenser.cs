using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartDispenser : MonoBehaviour, IInterractiveGameObject {

	public Sprite DisabledSprite;

	private Image _uiNotification;
	private bool _ready;  // TODO : add to the interface
	private const int Cost = 3;

	// Use this for initialization
	void Start () {
		_uiNotification = GetComponentInChildren<Image>();
		if (_uiNotification == null) {
			Debug.Log("error : ui notif can't be found");
		} else {
			_uiNotification.enabled = false;
		}
		_ready = true;
	}
	
	private void OnTriggerEnter2D(Collider2D other) {
		if (_ready && other.CompareTag("Player")) {
			_uiNotification.enabled = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			_uiNotification.enabled = false;
		}
	}

	public void Interract() {
		if(!_ready)
			return;
		GameObject go = GameObject.Find("Player");
		Player player = go.GetComponent<Player>();
		if (player.Coins >= Cost) {
			player.Coins -= Cost;
			player.Life++;
			
			var lightComponent = transform.GetComponentInChildren<Light>();
			lightComponent.enabled = false;
			gameObject.GetComponent<SpriteRenderer>().sprite = DisabledSprite;
			_ready = false;
			_uiNotification.enabled = false;
		}
		
	}
}
