using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Escape)){
            Application.Quit();
        }
    }

    // DEBUG
    void OnApplicationQuit() {
        PlayerPrefs.SetInt("Screenmanager Resolution Width", 800);
        PlayerPrefs.SetInt("Screenmanager Resolution Height", 600);
        PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 0);
    }
}
