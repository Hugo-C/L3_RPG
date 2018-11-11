using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour {

    public GameObject menu;
    public GameObject option;
	
    private bool _paused = false;
    private AudioSource _click;

    private void Start() {
        _click = GetComponent<AudioSource>();
        Close();
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_paused)
            {
                Pause();
            } else
            {
                Resume();
            }
        }
    }

    void Pause()
    {
        _paused = true;
        DisplayMenu();
        Cursor.visible = true;
        Time.timeScale = 0f;
        var gameobjects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        if (gameobjects != null)
        {
            foreach (var go in gameobjects)
            {
                IPausable[] pausables = go.GetComponents<IPausable>();
                foreach (var pausable in pausables)
                {
                    pausable.OnPauseGame();
                }
            }
        }
    }
	
    public void Resume()
    {
        _paused = false;
        Close();
        Cursor.visible = false;
        Time.timeScale = 1f;
        var gameobjects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        if (gameobjects != null)
        {
            foreach (var go in gameobjects)
            {
                IPausable[] pausables = go.GetComponents<IPausable>();
                foreach (var pausable in pausables)
                {
                    pausable.OnResumeGame();
                }
            }
        }
    }

    public void DisplayMenu()
    {
        _click.Play();
        menu.SetActive(true);
        option.SetActive(false);
    }

    public void DisplayOption()
    {
        _click.Play();
        menu.SetActive(false);
        option.SetActive(true);
    }

    public void Close()
    {
        _click.Play();
        menu.SetActive(false);
        option.SetActive(false);
    }

    public void ReturnMainMenu()
    {
        LevelManager levelManager = LevelManager.Instance;
        levelManager.LoadScene("menu");
    }
}
