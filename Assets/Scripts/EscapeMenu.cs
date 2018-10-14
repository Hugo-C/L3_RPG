using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour {

    public GameObject menu;
    public GameObject option;
	
    private bool paused = false;

    private void Start()
    {
        Close();
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
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
        paused = true;
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
        paused = false;
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
        menu.SetActive(true);
        option.SetActive(false);
    }

    public void DisplayOption()
    {
        menu.SetActive(false);
        option.SetActive(true);
    }

    public void Close()
    {
        menu.SetActive(false);
        option.SetActive(false);
    }

    public void ReturnMainMenu()
    {
        LevelManager levelManager = LevelManager.Instance;
        levelManager.LoadScene("menu");
    }
}
