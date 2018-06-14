using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    // const for generation
    const int MAP_WIDTH = 50;
    const int MAP_HEIGHT = 45;

    const int ROOM_SIZE_MIN = 5;
    const int ROOM_SIZE_MAX = 15;
    const int FAIL_MAX = 100;

    public static LevelManager instance = null;  // singleton patern
    public GameObject floor;
    public GameObject wall;

    private int[,] map;


    void Start() {
        //Check if instance already exists
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
        if (SceneManager.GetActiveScene().name == "main") {
            System.Random rnd = new System.Random();

            map = GenerateArray(MAP_WIDTH, MAP_HEIGHT, false);
            map = GenerateMap(map, rnd.Next());
            RenderMap(map);
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

    private static int[,] GenerateMap(int[,] map, int seed) {
        System.Random rnd = new System.Random(seed);
        // first we place some random room
        //map = GenerateRooms(map, rnd);

        // then we fill walls with a maze
        map = GenerateMaze(map, rnd);
        Debug.Log("map generated with seed : " + seed);
        return map;
    }

    // generate a new corridor and room from an existing room
    private static int[,] GenerateRooms(int[,] map, System.Random rnd) {
        int fail = 0;
        int x, y, width, heigth;
        while(fail < FAIL_MAX) {  // stop when the last room failed to generate
            fail = 0;
            width = rnd.Next(ROOM_SIZE_MIN, ROOM_SIZE_MAX);
            heigth = rnd.Next(ROOM_SIZE_MIN, ROOM_SIZE_MAX);
            bool success = false;  // indicate if the room is generating succesfully
            while (!success && fail < FAIL_MAX) {
                x = rnd.Next(1, MAP_WIDTH - width);
                y = rnd.Next(1, MAP_HEIGHT - heigth);
                if (IsMapFull(map, x - 1, y - 1, width + 1, heigth + 1)) { // check if the area is free
                    for (int i = x; i < x + width; i++) {
                        for (int j = y; j < y + heigth; j++) {
                            map[i, j] = 0;
                        }
                    }
                    success = true;
                } else {
                    fail++;
                }
            }
        }
        return map;
    }

    /**
     * Test if the given rect correspond to a full rect of wall (only 1) in the map
     */
    static bool IsMapFull(int[,] map, int x, int y, int width, int heigth) {
        if(x < 0 || y < 0 || x + width > MAP_WIDTH || y + heigth > MAP_HEIGHT) {
            Debug.LogWarning("PERFORMING OPERATION OUT OF THE MAP");
            return false;
        }

        bool res = true;
        for(int i = x; i <= x + width; i++) {
            for(int j = y; j <= y + heigth; j++) {
                if(map[i, j] != 1) {
                    res = false;
                }
            }
        }
        return res;
    }

    // Prim's algo
    private static int[,] GenerateMaze(int[,] map, System.Random rnd) {
        var wallsUnvisited = new List<System.Tuple<int, int>>();
        int x = rnd.Next(1, MAP_WIDTH-1);
        int y = rnd.Next(1, MAP_HEIGHT-1); // TODO check if x and y is a valid cell
        map[x, y] = 0;  // first cell of the maze
        wallsUnvisited.AddRange(GetNeighboringWall(map, x, y));

        while(wallsUnvisited.Count > 0) {
            System.Tuple<int, int> wall = wallsUnvisited[rnd.Next(wallsUnvisited.Count)];
            // open only if one of the two cells that the wall divides is visited
            if (IsValidWall(map, wall)) {
                map[wall.Item1, wall.Item2] = 0;
                wallsUnvisited.AddRange(GetNeighboringWall(map, wall.Item1, wall.Item2));
            }
            wallsUnvisited.Remove(wall);
        }
        return map;
    }

    private static List<System.Tuple<int, int>> GetNeighboringWall(int[,] map, int x, int y) {
        var list = new List<System.Tuple<int, int>>();
        for (int i = x - 1; i <= x + 1; i++) {
            for (int j = y - 1; j <= y + 1; j++) {
                if (i > 0 && i < MAP_WIDTH - 2 && j > 0 && j < MAP_HEIGHT - 2) {
                    if (!(i == x && j == y)) {
                        if  (map[i, j] == 1){
                            list.Add(System.Tuple.Create<int, int>(i, j));
                        }
                    }
                }
            }
        }
        return list;
    }        

    private static bool IsValidWall(int[,] map, System.Tuple<int, int> wall) {
        if(map[wall.Item1 - 1, wall.Item2 - 1] + map[wall.Item1 - 1, wall.Item2 + 1] + map[wall.Item1 + 1, wall.Item2 - 1] + map[wall.Item1 + 1, wall.Item2 + 1] >= 2) {
            return map[wall.Item1 - 1, wall.Item2] + map[wall.Item1 + 1, wall.Item2] == 1 && map[wall.Item1, wall.Item2 - 1] + map[wall.Item1, wall.Item2 + 1] == 2 ||
                   map[wall.Item1 - 1, wall.Item2] + map[wall.Item1 + 1, wall.Item2] == 2 && map[wall.Item1, wall.Item2 - 1] + map[wall.Item1, wall.Item2 + 1] == 1;
        }
        return false;
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
