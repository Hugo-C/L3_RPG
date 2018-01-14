using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    public static LevelManager instance = null;

    //Awake is always called before any Start functions
    void Awake() {
        //Check if instance already exists
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKey(KeyCode.Escape)){
            Application.Quit();
        }
    }

    public void LoadScene(string scene) {
        SceneManager.LoadScene(scene);
    }

    // DEBUG
    void OnApplicationQuit() {
        PlayerPrefs.SetInt("Screenmanager Resolution Width", 1920);
        PlayerPrefs.SetInt("Screenmanager Resolution Height", 1080);
        PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 1);
    }
}
