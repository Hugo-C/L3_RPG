using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    // const for generation
    const int MAP_WIDTH = 101;
    const int MAP_HEIGHT = 61;

    const int ROOM_SIZE_MIN = 5;
    const int ROOM_SIZE_MAX = 15;
    const int FAIL_MAX = 25;

    enum Direction { N, S, E, W };

    public static LevelManager instance = null;  // singleton patern
    public GameObject floor;
    public GameObject wall;

    private int[,] map;


    void Awake() {
        //Check if instance already exists
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
        if (SceneManager.GetActiveScene().name == "main") {
            LoadLevel();
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

    /// <summary>
    /// Load the specified scene 
    /// </summary>
    /// <param name="scene">the scene to load</param>
    public void LoadScene(string scene) {
        SceneManager.LoadScene(scene);
        if(scene == "main") {
            LoadLevel();
        }
    }

    /// <summary>
    /// Load a random dungeon level
    /// </summary>
    private void LoadLevel() {
        System.Random rnd = new System.Random();

        map = GenerateArray(MAP_WIDTH, MAP_HEIGHT, false);
        map = GenerateMap(map, rnd.Next());
        RenderMap(map);
    }

    /// <summary>
    /// Generate a 2D array either of 1 or 0
    /// </summary>
    /// <param name="width">width of the 2D array</param>
    /// <param name="height">height of the 2D array</param>
    /// <param name="empty">if empty => the 2D array if full of 0 else full of 1</param>
    /// <returns>the 2D array generated</returns>
    public static int[,] GenerateArray(int width, int height, bool empty) {
        int[,] map = new int[width + 1, height + 1];
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

    /// <summary>
    /// Generate a map representing a rogue-like level
    /// </summary>
    /// <param name="map">the map composed of wall (1) to transform</param>
    /// <param name="seed">the seed used in RNG</param>
    /// <returns>the generated map</returns>
    private static int[,] GenerateMap(int[,] map, int seed) {
        MyRandom rnd = new MyRandom(seed);
        // first we place some random room
        map = GenerateRooms(map, rnd);

        // then we fill walls with a maze
        map = GenerateMaze(map, rnd);
        List<System.Tuple<int, int>> connectors = FindConnectors(map);
        Debug.Log("connectors retrieved : " + connectors.Count);
        Shuffle(rnd, connectors);
        map = OpenConnector(map, connectors, rnd);
        map = Uncarve(map);  // remove dead ends
        Debug.Log("map generated with seed : " + seed);
        return map;
    }

    /// <summary>
    /// Generate several room inside the map
    /// rooms don't collide and are placed at odd coordinates
    /// </summary>
    /// <param name="map">the map rooms will be placed into</param>
    /// <param name="rnd">the random genrator used</param>
    /// <returns>the map containing rooms</returns>
    private static int[,] GenerateRooms(int[,] map, MyRandom rnd) {
        int fail = 0;
        int x, y, width, heigth;
        int nbrRooms = 0;
        while(fail < FAIL_MAX) {  // stop when the last room failed to generate
            fail = 0;
            width = rnd.NextOdd(ROOM_SIZE_MIN, ROOM_SIZE_MAX);
            heigth = rnd.NextOdd(ROOM_SIZE_MIN, ROOM_SIZE_MAX);
            bool success = false;  // indicate if the room is generating succesfully
            while (!success && fail < FAIL_MAX) {
                x = rnd.NextOdd(1, MAP_WIDTH - width);  // select only odd size
                y = rnd.NextOdd(1, MAP_HEIGHT - heigth);
                if (IsMapFull(map, x - 1, y - 1, width + 1, heigth + 1)) { // check if the area is free
                    for (int i = x; i < x + width; i++) {
                        for (int j = y; j < y + heigth; j++) {
                            map[i, j] = nbrRooms + 2;
                        }
                    }
                    success = true;
                    nbrRooms++;
                } else {
                    fail++;
                }
            }
        }
        return map;
    }

    /// <summary>
    /// Test if the given rectangle correspond to a full rectangle of wall (only 1) in the map
    /// </summary>
    /// <param name="map"></param>
    /// <param name="x">x coordinate of the rectangle</param>
    /// <param name="y">y coordinate of the rectangle</param>
    /// <param name="width">width of the rectangle</param>
    /// <param name="heigth">heigth of the rectangle</param>
    /// <returns>a boolean indicating if the map contains only wall(1) in the rectangle</returns>
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

    /// <summary>
    /// Generate a maze using the Recursive Backtracking's Algo
    /// the maze won't overlapse with rooms if they are present
    /// </summary>
    /// <param name="map">the map where the maze will be generated</param>
    /// <param name="rnd">the random genrator used</param>
    /// <returns>the map containing a maze</returns>
    private static int[,] GenerateMaze(int[,] map, MyRandom rnd) {
        map = PierceArray(map);  // we first pierce the array with unvisited cell
        for (int x = 1; x < map.GetUpperBound(0); x += 2) {
            for (int y = 1; y < map.GetUpperBound(1); y += 2) {
                if(map[x, y] == -1) {
                    map[x, y] = 0;  // first cell of the maze : 0 => explored
                    map = CarvePassageFrom(map, rnd, x, y);
                }
            }
        }
        return map;
    }

    /// <summary>
    /// Pierce a 2D array at odd coordinates if it's not occupied by a room
    /// those coordinates are replaced by -1
    /// </summary>
    /// <param name="map">the map to transform</param>
    /// <returns>the transformed map</returns>
    public static int[,] PierceArray(int[,] map) {
        for (int x = 1; x < map.GetUpperBound(0); x += 2) {
            for (int y = 1; y < map.GetUpperBound(1); y += 2) {
                if (map[x, y] <= 1) {
                    map[x, y] = -1; // -1 => unvisited by maze generation
                }
            }
        }
        return map;
    }

    /// <summary>
    /// Try to carve a passage from the given coordinates int the map in order to expend the maze
    /// </summary>
    /// <param name="map">the map used</param>
    /// <param name="rnd">the random genrator used</param>
    /// <param name="x">the x coordinate</param>
    /// <param name="y">the y coordinate</param>
    /// <returns>the carved map</returns>
    private static int[,] CarvePassageFrom(int[,] map, MyRandom rnd, int x, int y) {
        System.Array directions = System.Enum.GetValues(typeof(Direction));
        Shuffle<Direction>(rnd, directions);
        foreach (Direction dir in directions) {
            System.Tuple<int, int> wall = System.Tuple.Create(x + DX(dir), y + DY(dir));
            if (IsValidWall(map, wall, dir)) {  // open only if one of the two cells that the wall divides is visited      
                map[wall.Item1, wall.Item2] = 0;
                map[wall.Item1 + DX(dir), wall.Item2 + DY(dir)] = 0;
                CarvePassageFrom(map, rnd, wall.Item1 + DX(dir), wall.Item2 + DY(dir));
            }
        }
        return map;
    }

    /// <summary>
    /// Check if the wall can be carved to expend the maze
    /// </summary>
    /// <param name="map">the map used</param>
    /// <param name="wall">the wall to test</param>
    /// <param name="dir">the direction from where we want to carve the wall</param>
    /// <returns>boolean indicating if the wall can be carved</returns>
    private static bool IsValidWall(int[,] map, System.Tuple<int, int> wall, Direction dir) {
        if(0 < wall.Item1 && wall.Item1 < MAP_WIDTH - 1 && 0 < wall.Item2 && wall.Item2 < MAP_HEIGHT - 1) {
            return map[wall.Item1, wall.Item2] == 1 && map[wall.Item1 + DX(dir), wall.Item2 + DY(dir)] == -1; // a wall going to a place not already visited
        }
        return false;
    }

    /// <summary>
    /// Get all the connectors in the map
    /// 
    /// a connector is wall between a room and a wall or another room
    /// </summary>
    /// <param name="map">the map where to retrieve the connector</param>
    /// <returns>a List of tuple, containing coorinates for those connectors</returns>
    private static List<System.Tuple<int, int>> FindConnectors(int[,] map) {
        //System.Tuple<int, int> wall = System.Tuple<int, int>.Create(x + DX(dir), y + DY(dir));
        List<System.Tuple<int, int>> connectors = new List<System.Tuple<int, int>>();
        for (int x = 1; x < map.GetUpperBound(0) - 1; x++) {
            for (int y = 1; y < map.GetUpperBound(1) - 1; y++) {
                if (IsValidConnector(map, x, y)){
                    System.Tuple<int, int> connector = System.Tuple.Create(x, y);
                    connectors.Add(connector);
                }
            }
        }
        return connectors;
    }

    /// <summary>
    /// Check if a coordinate in the map is a valid connector
    /// </summary>
    /// <param name="map">the map where to check the connector</param>
    /// <param name="x">x coordinate of the case to check</param>
    /// <param name="y">y coordinate of the case to check</param>
    /// <returns></returns>
    private static bool IsValidConnector(int[,] map, int x, int y) {
        if (map[x, y] == 1) {
            if(map[x + 1, y] != 1 && map[x - 1, y] != 1 &&  map[x + 1, y] != map[x - 1, y] ||
               map[x, y + 1] != 1 && map[x, y - 1] != 1 && map[x, y + 1] != map[x, y - 1]) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Open some connectors in order to allow access to rooms
    /// </summary>
    /// <param name="map"></param>
    /// <param name="connectors">the list of all the connectors of the map</param>
    /// <param name="rnd">the random genrator used</param>
    /// <returns>the trnsformed map</returns>
    private static int[,] OpenConnector(int[,] map, List<System.Tuple<int, int>> connectors, MyRandom rnd) {
        foreach (System.Tuple<int, int> connector in connectors) {
            Direction dir = Direction.E;
            if (map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)] == 1) {
                dir = Direction.N;
            }
            // if the connector is on an already open area
            if (map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)] == 0 && map[connector.Item1 + DX(Oppose(dir)), connector.Item2 + DY(Oppose(dir))] == 0) {
                Debug.Log("already open !");  // TODO
            } else {
                //Debug.Log("direction : " + dir.ToString() + " : " + map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)] + " " + map[connector.Item1 + DX(Oppose(dir)), connector.Item2 + DY(Oppose(dir))]);
                if(map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)] == 0) {
                    dir = Oppose(dir);
                }
                int prevValue = map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)];
                if(prevValue > 1) {
                    map = FloodFill(map, connector.Item1, connector.Item2, prevValue, 0);
                }
            }
        }
        return map;
    }

    /// <summary>
    /// Change the given coordinate to currentValue and try to change adjacent case of prevValue to currentValue
    /// </summary>
    /// <param name="map"></param>
    /// <param name="x">x coordinate of the case to set to currentvalue and from where floodfill will spread</param>
    /// <param name="y">y coordinate of the case to set to currentvalue and from where floodfill will spread</param>
    /// <param name="prevValue">the value used to spread floodfill</param>
    /// <param name="currentValue">the value spread by floodfill on adjacent case of value prevValue</param>
    /// <returns>the transformed map</returns>
    private static int[,] FloodFill(int[,] map, int x, int y, int prevValue, int currentValue) {
        map[x, y] = currentValue;
        //Debug.Log("flood fill : " + x + ", " + y);
        System.Array directions = System.Enum.GetValues(typeof(Direction));
        foreach (Direction dir in directions) {
            if(x + DX(dir) > 0 && x + DX(dir) < map.GetUpperBound(0) && y + DY(dir) > 0 && y + DY(dir) < map.GetUpperBound(1) && map[x + DX(dir), y + DY(dir)] == prevValue) {
                map = FloodFill(map, x + DX(dir), y + DY(dir), prevValue, currentValue);
            }
        }
        return map;
    }

    /// <summary>
    /// Replace dead ends by walls
    /// </summary>
    /// <param name="map"></param>
    /// <returns>the transformed map</returns>
    private static int[,] Uncarve(int[,] map) {
        for (int x = 1; x < map.GetUpperBound(0) - 1; x++) {
            for (int y = 1; y < map.GetUpperBound(1) - 1; y++) {
                // Check if it's a dead end
                if (IsDeadEnd(map, x, y)) {
                    Debug.Log("dead end find ! : " + x + ", " + y);
                    map = UncarveDeadEnd(map, x, y);
                }
            }
        }
        return map;
    }

    /// <summary>
    /// Uncarve a dead end starting from the given coordinates
    /// </summary>
    /// <param name="map"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>the transformed map</returns>
    private static int[,] UncarveDeadEnd(int[,] map, int x, int y) {
        map[x, y] = 1;  // fill the dead end with wall
        // then we look if an adjacent case is a dead end
        System.Array directions = System.Enum.GetValues(typeof(Direction));
        foreach (Direction dir in directions) {
            if (x + DX(dir) > 0 && x + DX(dir) < map.GetUpperBound(0) && y + DY(dir) > 0 && y + DY(dir) < map.GetUpperBound(1) && IsDeadEnd(map, x + DX(dir), y + DY(dir))) {
                map = UncarveDeadEnd(map, x + DX(dir), y + DY(dir));
                //break;  // there can be only one adjacent dead end (since the previous case was a dead end) 
            }
        }
        return map;
    }

    /// <summary>
    /// Check if the coordinate is a dead end
    /// 
    /// a dead end is a floor with three or more adjacent walls
    /// </summary>
    /// <param name="map"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static bool IsDeadEnd(int[,] map, int x, int y) {
        return map[x, y] == 0 && map[x - 1, y] + map[x + 1, y] + map[x, y - 1] + map[x, y + 1] >= 3;
    }

    /// <summary>
    /// Indicate the value to be added to the x axis by the direction d
    /// </summary>
    /// <param name="d">the direction</param>
    /// <returns>the integer value of the direction on the x axis</returns>
    private static int DX(Direction d) {
        switch (d) {
            case Direction.E:
                return 1;
            case Direction.W:
                return -1;
            case Direction.N:
                return 0;
            case Direction.S:
                return 0;
            default:
                Debug.LogWarning("ERROR : NOT A DIRECTION");
                return 0;
        }
    }

    /// <summary>
    /// Indicate the value to be added to the y axis by the direction d
    /// </summary>
    /// <param name="d">the direction</param>
    /// <returns>the integer value of the direction on the y axis</returns>
    private static int DY(Direction d) {
        switch (d) {
            case Direction.E:
                return 0;
            case Direction.W:
                return 0;
            case Direction.N:
                return 1;
            case Direction.S:
                return -1;
            default:
                Debug.LogWarning("ERROR : NOT A DIRECTION");
                return 0;
        }
    }

    private static Direction Oppose(Direction d) {
        switch (d) {
            case Direction.E:
                return Direction.W;
            case Direction.W:
                return Direction.E;
            case Direction.N:
                return Direction.S;
            case Direction.S:
                return Direction.N;
            default:
                Debug.LogWarning("ERROR : NOT A DIRECTION");
                return 0;
        }
    }

    /// <summary>
    /// Shuffle the array
    /// </summary>
    /// <typeparam name="T">the type of the elements in the array</typeparam>
    /// <param name="rng">the random generator used</param>
    /// <param name="array">the array to shuffle</param>
    public static void Shuffle<T>(MyRandom rng, System.Array array) {  // TODO : return tab ?
        int n = array.Length;
        while (n > 1) {
            int k = rng.Next(n--);
            T tmp = (T)array.GetValue(n);
            array.SetValue(array.GetValue(k), n);
            array.SetValue(tmp, k);
        }
    }

    /// <summary>
    /// Shuffle the list
    /// </summary>
    /// <typeparam name="T">the type of the elements in the list</typeparam>
    /// <param name="rng">the random generator used</param>
    /// <param name="list">the list to shuffle</param>
    public static void Shuffle<T>(MyRandom rng, List<T> list) {
        int n = list.Count;
        while (n > 1) {
            int k = rng.Next(n--);
            T tmp = (T)list[n];
            list[n] = list[k];
            list[k] = tmp;
        }
    }

    /// <summary>
    /// Render the map
    /// </summary>
    /// <param name="map">the map to be render</param>
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
                    Debug.LogWarning("corrupted map : " + map[x, y] + " x : " + x + " y : " + y);
                    clone = Instantiate(floor, new Vector3(x, y), Quaternion.identity);
                }
                clone.transform.parent = gameObject.transform;  // organize the editor view TODO change since it won't destroy itself on load => 
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
