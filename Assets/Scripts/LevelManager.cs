using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    public static LevelManager Instance;  // singleton patern

    private bool _loading;

    void Awake() {
        //Check if instance already exists
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
        _loading = false;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKey(KeyCode.Escape)){
            Application.Quit();
        }
    }

    /// <summary>
    /// Load the specified scene 
    /// </summary>
    /// <param name="scene">the scene to load</param>
    public void LoadScene(string scene) {
        SceneManager.LoadScene(scene);
    }

    public void LoadScene(string scene, int delay) {
        if(!_loading) {
            StartCoroutine(LoadSceneWithDelay(scene, delay));
        }
    }

    private IEnumerator LoadSceneWithDelay(string scene, int delay) {
        _loading = true;
        yield return new WaitForSeconds(delay);
        LoadScene(scene);
        _loading = false;
    }

    // DEBUG
    void OnApplicationQuit() {
        PlayerPrefs.SetInt("Screenmanager Resolution Width", 1920);
        PlayerPrefs.SetInt("Screenmanager Resolution Height", 1080);
        PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 1);
    }
}
