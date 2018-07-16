using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {


    // const for generation
    private const int MapWidth = 33; // map size must be odd numbers
    private const int MapHeight = 33;

    private const int RoomSizeMin = 5;
    private const int RoomSizeMax = 15;
    private const int FailMax = 25;
    private const double ConnectorRatio = 0.05;

    private enum Direction { N, S, E, W }


    public GameObject FloorPrefab;
    public GameObject WallPrefab;
    public GameObject ExitPrefab;

    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;

    private int[,] _map;
    private MyRandom _rnd;

    private void Awake() {
        _rnd = new MyRandom();
        LoadLevel();
    }


    /// <summary>
    /// Load a random dungeon level
    /// </summary>
    void LoadLevel() {
        GenerateMap();
        RenderMap();
        GameObject player = Spawn(_map, PlayerPrefab);
        player.name = "Player";

        for (int i = _rnd.Next(5, 10); i >= 0; i--) {
            Spawn(_map, EnemyPrefab);  // TODO : don't spawn enemy on the player
        }
        GameObject exit = SpawnExit(_map, ExitPrefab, player);
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
    /// <returns>the generated map</returns>
    private void GenerateMap() {
        _map = GenerateArray(MapWidth, MapHeight, false);
        // first we place some random room
        GenerateRooms();

        // then we fill walls with a maze
        GenerateMaze();
        List<Tuple<int, int>> connectors = FindConnectors(_map);
        Shuffle(_rnd, connectors);
        _map = OpenConnectors(_map, connectors);
        Uncarve();  // remove dead ends
        OpenConnectorsRandom();
        Debug.Log("map generated with seed : " + _rnd.Seed);
    }

    /// <summary>
    /// Generate several room inside the map
    /// rooms don't collide and are placed at odd coordinates
    /// </summary>
    private void GenerateRooms() {
        int fail = 0;
        int nbrRooms = 0;  // used to mark an area
        while (fail < FailMax) {  // stop when the last room failed to generate
            fail = 0;
            int width = _rnd.NextOdd(RoomSizeMin, RoomSizeMax);
            int heigth = _rnd.NextOdd(RoomSizeMin, RoomSizeMax);
            bool success = false;  // indicate if the room is generating succesfully
            while (!success && fail < FailMax) {
                int x = _rnd.NextOdd(1, MapWidth - width);
                int y = _rnd.NextOdd(1, MapHeight - heigth);
                if (IsMapFull(_map, x - 1, y - 1, width + 1, heigth + 1)) { // check if the area is free
                    for (int i = x; i < x + width; i++) {
                        for (int j = y; j < y + heigth; j++) {
                            _map[i, j] = nbrRooms + 2; // area 0 and 1 are reserved
                        }
                    }
                    success = true;
                    nbrRooms++;
                } else {
                    fail++;
                }
            }
        }
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
        if (x < 0 || y < 0 || x + width > MapWidth || y + heigth > MapHeight) {
            Debug.LogWarning("PERFORMING OPERATION OUT OF THE MAP");
            return false;
        }

        bool res = true;
        for (int i = x; i <= x + width; i++) {
            for (int j = y; j <= y + heigth; j++) {
                if (map[i, j] != 1) {
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
    private void GenerateMaze() {
        _map = PierceArray(_map);  // we first pierce the array with unvisited cell
        for (int x = 1; x < _map.GetUpperBound(0); x += 2) {
            for (int y = 1; y < _map.GetUpperBound(1); y += 2) {
                if (_map[x, y] == -1) {
                    _map[x, y] = 0;  // first cell of the maze : 0 => explored
                    _map = CarvePassageFrom(_map, _rnd, x, y);
                }
            }
        }
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
        Array directions = Enum.GetValues(typeof(Direction));
        Shuffle<Direction>(rnd, directions);
        foreach (Direction dir in directions) {
            Tuple<int, int> wall = Tuple.Create(x + DX(dir), y + DY(dir));
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
    private static bool IsValidWall(int[,] map, Tuple<int, int> wall, Direction dir) {
        if (0 < wall.Item1 && wall.Item1 < MapWidth - 1 && 0 < wall.Item2 && wall.Item2 < MapHeight - 1) {
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
    private static List<Tuple<int, int>> FindConnectors(int[,] map) {
        var connectors = new List<Tuple<int, int>>();
        for (int x = 1; x < map.GetUpperBound(0) - 1; x++) {
            for (int y = 1; y < map.GetUpperBound(1) - 1; y++) {
                if (IsValidConnector(map, x, y)) {
                    Tuple<int, int> connector = Tuple.Create(x, y);
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
            if (map[x + 1, y] != 1 && map[x - 1, y] != 1 && map[x + 1, y] != map[x - 1, y] ||
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
    /// <returns>the trnsformed map</returns>
    private static int[,] OpenConnectors(int[,] map, IEnumerable<Tuple<int, int>> connectors) {
        foreach (var connector in connectors) {
            // First we find the direction from where the connector open to an already visited area
            Direction dir = Direction.E;
            if (map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)] == 1)
                dir = Direction.N;
            if (map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)] != 0)
                dir = Oppose(dir);

            // Then we open the connector if the connector link a visited and an unvisited area
            if (map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)] == 0 &&
                map[connector.Item1 + DX(Oppose(dir)), connector.Item2 + DY(Oppose(dir))] != 0) {
                if (map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)] == 0) {
                    dir = Oppose(dir);
                }

                int prevValue = map[connector.Item1 + DX(dir), connector.Item2 + DY(dir)];
                if (prevValue > 1) {
                    map = FloodFill(map, connector.Item1, connector.Item2, prevValue, 0);
                }
            }
        }
        return map;
    }

    /// <summary>
    /// Open random connectors in order to allow more access to rooms and between corridor
    /// </summary>
    /// <returns>the trnsformed map</returns>
    private void OpenConnectorsRandom() {
        for (int x = 2; x < _map.GetUpperBound(0) - 1; x += 2) {
            for (int y = 2; y < _map.GetUpperBound(1) - 1; y += 2) {
                if (_map[x, y] == 1 && ((_map[x + 1, y] == 0 && _map[x + 1, y] == _map[x - 1, y]) || (_map[x, y + 1] == 0 && _map[x, y + 1] == _map[x, y - 1])) &&_rnd.NextDouble() < ConnectorRatio) {
                    _map[x, y] = 0;
                    //Debug.Log("random connector open @ x : " + x + ", y: " + y);
                }
            }
        }
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
        Array directions = Enum.GetValues(typeof(Direction));
        foreach (Direction dir in directions) {
            if (x + DX(dir) > 0 && x + DX(dir) < map.GetUpperBound(0) && y + DY(dir) > 0 && y + DY(dir) < map.GetUpperBound(1) && map[x + DX(dir), y + DY(dir)] == prevValue) {
                map = FloodFill(map, x + DX(dir), y + DY(dir), prevValue, currentValue);
            }
        }
        return map;
    }

    /// <summary>
    /// Replace dead ends by walls
    /// </summary>
    private void Uncarve() {
        for (int x = 1; x < _map.GetUpperBound(0) - 1; x++) {
            for (int y = 1; y < _map.GetUpperBound(1) - 1; y++) {
                // Check if it's a dead end
                if (IsDeadEnd(_map, x, y)) {
                    _map = UncarveDeadEnd(_map, x, y);
                }
            }
        }
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
        Array directions = Enum.GetValues(typeof(Direction));
        foreach (Direction dir in directions) {
            if (x + DX(dir) > 0 && x + DX(dir) < map.GetUpperBound(0) && y + DY(dir) > 0 && y + DY(dir) < map.GetUpperBound(1) && IsDeadEnd(map, x + DX(dir), y + DY(dir))) {
                map = UncarveDeadEnd(map, x + DX(dir), y + DY(dir));
                break;  // there can be only one adjacent dead end (since the previous case was a dead end) 
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
            var tmp = list[n];
            list[n] = list[k];
            list[k] = tmp;
        }
    }

    /// <summary>
    /// Render the map
    /// </summary>
    private void RenderMap() {
        //Loop through the width of the map
        for (int x = 0; x < _map.GetUpperBound(0); x++) {
            //Loop through the height of the map
            for (int y = 0; y < _map.GetUpperBound(1); y++) {
                // 1 = wall, 0 = floor
                GameObject clone = null;
                if (_map[x, y] == 1) {
                    clone = Instantiate(WallPrefab, new Vector3(x, y, 1f), Quaternion.identity);
                } else if (_map[x, y] == 0) {
                    clone = Instantiate(FloorPrefab, new Vector3(x, y, 1f), Quaternion.identity);
                } else {
                    Debug.LogWarning("corrupted map : " + _map[x, y] + " x : " + x + " y : " + y);
                    //clone = Instantiate(FloorPrefab, new Vector3(x, y), Quaternion.identity);
                }
                if (clone != null) 
                    clone.transform.parent = gameObject.transform; // organize the editor view
            }
        }
    }

    /// <summary>
    /// Spawn a GameObject int the map at a clear area
    /// 
    /// a clear area is a floor case with 4 adjacent floors
    /// </summary>
    /// <param name="map"></param>
    /// <param name="go">the gameObject to spawn</param>
    /// <returns>a clone of the gameObject, who spawned in the map</returns>
    public GameObject Spawn(int[,] map, GameObject go) {
        int x, y;
        do {
            x = _rnd.Next(1, map.GetUpperBound(0));
            y = _rnd.Next(1, map.GetUpperBound(1));
        } while (map[x, y] + map[x + 1, y] + map[x - 1, y] + map[x, y + 1] + map[x, y - 1] != 0);
        return Instantiate(go, new Vector2(x, y), Quaternion.identity);
    }

    /// <summary>
    /// Spawn a GameObject int the map at a clear area
    /// 
    /// a clear area is a floor case with 4 adjacent floors
    /// </summary>
    /// <param name="map2"></param>
    /// <param name="exit">the gameObject to spawn</param>
    /// <param name="player">the gameobject of a player, used to spawn the exit a bit far from him</param>
    /// <returns>a clone of the gameObject, who spawned in the map</returns>
    private GameObject SpawnExit(int[,] map2, GameObject exit, GameObject player) {
        float minDistance = new Vector2(MapWidth, MapHeight).magnitude / 2f;
        int x, y;
        Vector2 position;
        int nbTry = 0;
        do {
            nbTry++;
            if (nbTry % 10 == 0) {
                minDistance--;  // minDistance is reduced to allow small maps
            }
            x = _rnd.Next(1, map2.GetUpperBound(0));
            y = _rnd.Next(1, map2.GetUpperBound(1));
            position = new Vector2(x, y);
        } while (map2[x, y] + map2[x + 1, y] + map2[x - 1, y] + map2[x, y + 1] + map2[x, y - 1] != 0 ||
                 Vector2.Distance(position, player.transform.position) < minDistance);
        return Instantiate(exit, position, Quaternion.identity);
    }
}
