using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {
	
	public static AudioManager Instance; // singleton patern

	public AudioClip MainTheme; 
	public AudioClip MenuTheme; 

	private AudioSource _audioSource;

	// Use this for initialization
	void Start () {
		//Check if instance already exists
		if (Instance == null) {
			Instance = this;
		} else if (Instance != this) {
			Destroy(gameObject);
			return;
		}
		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);
		_audioSource = GetComponent<AudioSource>();
		LoadMusic(SceneManager.GetActiveScene().name);
	}

	public void LoadMusic(string scene) {
		_audioSource.Pause();
		if (scene == "main") {
			_audioSource.clip = MainTheme;
		} else if(scene == "menu") {
			_audioSource.clip = MenuTheme;
		}
		_audioSource.Play();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
