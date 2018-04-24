using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    const int MIN_SIZE_ROOM = 15;
    const int MAX_SIZE_ROOM = 20;
    public static LevelManager instance = null;
    public GameObject floor;
    public GameObject northWall;

    //Awake is always called before any Start functions
    void Awake() {
        if(SceneManager.GetActiveScene().name == "main") {
            GenerateLevel();
        }
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

    public void GenerateLevel() {
        // generate the floor
        int floorHeight = Random.Range(MIN_SIZE_ROOM, MAX_SIZE_ROOM);
        int floorWidth = Random.Range(MIN_SIZE_ROOM, MAX_SIZE_ROOM);
        GameObject myFloor = Instantiate(floor, new Vector3(0f, 0f, 1f), Quaternion.identity);
        SpriteRenderer srFloor = myFloor.GetComponent<SpriteRenderer>();
        srFloor.drawMode = SpriteDrawMode.Sliced;
        srFloor.size = new Vector2(floorWidth, floorHeight);

        // generate walls
        GameObject myNorthWall = Instantiate(northWall, new Vector3(0f, (floorHeight + northWall.GetComponent<SpriteRenderer>().size.y) / 2), Quaternion.identity);
        SpriteRenderer srNorthWall = myNorthWall.GetComponent<SpriteRenderer>();
        srNorthWall.drawMode = SpriteDrawMode.Tiled;
        srNorthWall.size = new Vector2(floorWidth / 1.9f, srNorthWall.size.y);
        BoxCollider2D myNorthbc = myNorthWall.GetComponent<BoxCollider2D>();
        myNorthbc.size = new Vector2(floorWidth / 1.9f, myNorthbc.size.y);

        GameObject myEastWall = Instantiate(northWall, new Vector3((floorWidth + northWall.GetComponent<SpriteRenderer>().size.y) / 2, 0f), Quaternion.Euler(0f, 0f, -90f));
        SpriteRenderer srEastWall = myEastWall.GetComponent<SpriteRenderer>();
        srEastWall.drawMode = SpriteDrawMode.Tiled;
        srEastWall.size = new Vector2(floorHeight / 1.9f, srEastWall.size.y);
        BoxCollider2D myEasthbc = myEastWall.GetComponent<BoxCollider2D>();
        myEasthbc.size = new Vector2(floorHeight / 1.9f, myEasthbc.size.y);

        GameObject mySouthWall = Instantiate(northWall, new Vector3(0f, - (floorHeight + northWall.GetComponent<SpriteRenderer>().size.y) / 2), Quaternion.Euler(0f, 0f, 180f));
        SpriteRenderer srSouthtWall = mySouthWall.GetComponent<SpriteRenderer>();
        srSouthtWall.drawMode = SpriteDrawMode.Tiled;
        srSouthtWall.size = new Vector2(floorWidth / 1.9f, srSouthtWall.size.y);
        BoxCollider2D mySouthbc = mySouthWall.GetComponent<BoxCollider2D>();
        mySouthbc.size = new Vector2(floorWidth / 1.9f, mySouthbc.size.y);

        GameObject myWestWall = Instantiate(northWall, new Vector3(- (floorWidth + northWall.GetComponent<SpriteRenderer>().size.y) / 2, 0f), Quaternion.Euler(0f, 0f, 90f));
        SpriteRenderer srWestWall = myWestWall.GetComponent<SpriteRenderer>();
        srWestWall.drawMode = SpriteDrawMode.Tiled;
        srWestWall.size = new Vector2(floorHeight / 1.9f, srWestWall.size.y);
        BoxCollider2D myWestbc = myWestWall.GetComponent<BoxCollider2D>();
        myWestbc.size = new Vector2(floorHeight / 1.9f, myWestbc.size.y);
    }

    // DEBUG
    void OnApplicationQuit() {
        PlayerPrefs.SetInt("Screenmanager Resolution Width", 1920);
        PlayerPrefs.SetInt("Screenmanager Resolution Height", 1080);
        PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 1);
    }
}
