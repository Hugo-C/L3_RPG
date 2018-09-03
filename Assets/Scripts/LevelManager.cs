using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

    public static LevelManager Instance; // singleton patern
    public static GameObject UiInstance; // singleton pattertn
    public GameObject UI;

    private Text _levelCompletedCount;
    
    private bool _loading;
    private int _levelCompleted;
    
    public int LevelCompleted {
        get {
            return _levelCompleted;
        }

        set {
            _levelCompleted = value;
            if (_levelCompletedCount == null) {
                _levelCompletedCount = GameObject.Find("LevelCount").GetComponent<Text>();
            }
            _levelCompletedCount.text = _levelCompleted + "/∞";
        }
    }

    void Awake() {
        //Check if instance already exists
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
            return;
        }
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
        if (SceneManager.GetActiveScene().name == "main") {
            InitUi();
            _levelCompletedCount = GameObject.Find("LevelCount").GetComponent<Text>();
            LevelCompleted = 0;
        }

        Application.targetFrameRate = 90;
        _loading = false;
    }

    /// <summary>
    /// Load the specified scene 
    /// </summary>
    /// <param name="scene">the scene to load</param>
    public void LoadScene(string scene) {
        if (scene == "main") {
            Cursor.visible = false;
            InitUi();
        } else {
            if (SceneManager.GetActiveScene().name == "main") {
                LevelCompleted = 0;
                Destroy(UiInstance);
            }
            Cursor.visible = true;
        }
        SceneManager.LoadScene(scene);
        GameObject myAudioManager = GameObject.Find("AudioManager");
        myAudioManager.GetComponent<AudioManager>().LoadMusic(scene);
        Time.timeScale = 1f;
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

    private void InitUi() {
        if (UiInstance == null) {
            UiInstance = Instantiate(UI);
            DontDestroyOnLoad(UiInstance);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    // DEBUG
    void OnApplicationQuit() {
        PlayerPrefs.SetInt("Screenmanager Resolution Width", 1920);
        PlayerPrefs.SetInt("Screenmanager Resolution Height", 1080);
        PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 1);
    }
}
