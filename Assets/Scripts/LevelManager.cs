using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    const int MIN_SIZE_ROOM = 10;
    const int MAX_SIZE_ROOM = 20;
    public static LevelManager instance = null;  // singleton patern
    public GameObject floor;
    public GameObject wall;

    private int[,] map;

    //Awake is always called before any Start functions
    void Awake() {
        if(SceneManager.GetActiveScene().name == "main") {
            System.Random rnd = new System.Random();
            float seed = (float)rnd.NextDouble() * 10f;

            map = GenerateArray(50, 50, true);
            map = PerlinNoise(map, seed);
            RenderMap(map);
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

    public static int[,] GenerateArray(int width, int height, bool empty) {
        int[,] map = new int[width, height];
        for (int x = 0; x < map.GetUpperBound(0); x++) {
            for (int y = 0; y < map.GetUpperBound(1); y++) {
                if (x == 0 || x == map.GetUpperBound(0) - 1 || y == 0 || y == map.GetUpperBound(1) - 1) {
                    map[x, y] = 1;
                } else if (empty) {
                    map[x, y] = 0;
                } else {
                    map[x, y] = 1;
                }
            }
        }
        return map;
    }

    public static int[,] PerlinNoise(int[,] map, float seed) {
        //Used to reduced the position of the Perlin point
        float reduction = 0.5f;
        //Create the Perlin
        for (int x = 1; x < map.GetUpperBound(0) - 1; x++) {
            for (int y = 1; y < map.GetUpperBound(1) - 1; y++) {
                float xSeed = (float)x / (float)map.GetUpperBound(0) * seed;
                float ySeed = (float)y / (float)map.GetUpperBound(1) * seed;
                int isWall = Mathf.FloorToInt(Mathf.PerlinNoise(xSeed, ySeed) + reduction);
                map[x, y] = isWall;
                //Debug.Log("x : " + xSeed + " y : " + ySeed + " ~~ " + Mathf.PerlinNoise(xSeed, ySeed) + " => " + isWall);
            }
        }
        return map;
    }

    public void RenderMap(int[,] map) {
        GameObject clone;
        //Loop through the width of the map
        for (int x = 0; x < map.GetUpperBound(0); x++) {
            //Loop through the height of the map
            for (int y = 0; y < map.GetUpperBound(1); y++) {
                // 1 = wall, 0 = floor
                if (map[x, y] == 1) {
                    clone = Instantiate(wall, new Vector3(x, y), Quaternion.identity);
                } else if (map[x, y] == 0) {
                    clone = Instantiate(floor, new Vector3(x, y), Quaternion.identity);
                } else {
                    Debug.LogWarning("corrupted map : " + map[x, y]);
                    clone = Instantiate(floor, new Vector3(x, y), Quaternion.identity);
                }
                clone.transform.parent = gameObject.transform;  // organize the editor view
            }
        }
    }

    // DEBUG
    void OnApplicationQuit() {
        PlayerPrefs.SetInt("Screenmanager Resolution Width", 1920);
        PlayerPrefs.SetInt("Screenmanager Resolution Height", 1080);
        PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 1);
    }
}
