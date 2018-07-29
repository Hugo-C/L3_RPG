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
    private const float OverlappingMinDistance = 2f;

    private enum Direction { N, S, E, W }


    public GameObject FloorPrefab;
    public GameObject WallPrefab;
    public GameObject ExitPrefab;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject HeartDispenser;

    private List<GameObject> _spawnedGameObjects;

    private Tile[,] _map;
    private MyRandom _rnd;

    private void Awake() {
        _rnd = new MyRandom();
        LoadLevel();
    }


    /// <summary>
    /// Load a random dungeon level
    /// </summary>
    public void LoadLevel() {
        GenerateMap();
        RenderMap();
        _spawnedGameObjects = new List<GameObject>();
        GameObject player = Spawn(PlayerPrefab);
        player.name = "Player";
        SpawnExit(ExitPrefab, player);
        for (int i = _rnd.Next(5, 10); i >= 0; i--) {
            Spawn(EnemyPrefab);
        }
        
        int level = GameObject.Find("LevelManager").GetComponent<LevelManager>().LevelCompleted;
        if (level % 2 == 0)
            Spawn(HeartDispenser);
    }

    /// <summary>
    /// Generate a 2D array either of 1 or 0
    /// </summary>
    /// <param name="width">width of the 2D array</param>
    /// <param name="height">height of the 2D array</param>
    /// <param name="empty">if empty => the 2D array if full of 0 else full of 1</param>
    /// <returns>the 2D array generated</returns>
    public static Tile[,] GenerateArray(int width, int height, bool empty) {
        Tile[,] map = new Tile[width + 1, height + 1];
        for (int x = 0; x <= map.GetUpperBound(0); x++) {
            for (int y = 0; y <= map.GetUpperBound(1); y++) {
                if (x == 0 || x == map.GetUpperBound(0) || y == 0 || y == map.GetUpperBound(1)) {
                    map[x, y] = new Tile(x, y, Type.Wall);
                } else if (empty) {
                    map[x, y] = new Tile(x, y, Type.Floor);
                } else {
                    map[x, y] = new Tile(x, y, Type.Wall);
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
        List<Tile> connectors = FindConnectors(_map);
        connectors = Shuffle(_rnd, connectors);
        OpenConnectors(connectors);
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
                            _map[i, j].Type = Type.Floor;
                            _map[i, j].Room = true;
                            Tile.Union(_map[x, y], _map[i, j]);  // we link all tiles of a room together
                        }
                    }
                    success = true;
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
    static bool IsMapFull(Tile[,] map, int x, int y, int width, int heigth) {
        if (x < 0 || y < 0 || x + width > MapWidth || y + heigth > MapHeight) {
            Debug.LogWarning("PERFORMING OPERATION OUT OF THE MAP");
            return false;
        }

        bool res = true;
        for (int i = x; i <= x + width; i++) {
            for (int j = y; j <= y + heigth; j++) {
                if (map[i, j].Type != Type.Wall) {
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
                Tile t = _map[x, y];
                if (Tile.Find(t) == t && !t.Room) {
                    CarvePassageFrom(t);
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
    public static Tile[,] PierceArray(Tile[,] map) {
        for (int x = 1; x < map.GetUpperBound(0); x += 2) {
            for (int y = 1; y < map.GetUpperBound(1); y += 2) {
                map[x, y].Type = Type.Floor;
            }
        }
        return map;
    }

    /// <summary>
    /// Try to carve a passage from the given coordinates in the map in order to expend the maze
    /// </summary>
    /// <param name="origin">the starting point to carve</param>
    /// <returns>the carved map</returns>
    private void CarvePassageFrom(Tile origin) {
        Array directions = Enum.GetValues(typeof(Direction));
        directions = Shuffle(_rnd, directions);
        foreach (Direction dir in directions) {
            if (0 < origin.X + 2*DX(dir) && origin.X + 2*DX(dir) < MapWidth - 1 && 0 < origin.Y + 2*DY(dir) &&
                origin.Y + 2*DY(dir) < MapHeight - 1) {
                Tile wall = _map[origin.X + DX(dir), origin.Y + DY(dir)];
                Tile goal = _map[wall.X + DX(dir), wall.Y + DY(dir)];
                if (IsValidWallForMazeExpansion(wall, origin, goal)) {
                    // open only if one of the two cells that the wall divides is visited
                    wall.Type = Type.Floor;
                    Tile.Union(origin, wall);
                    Tile.Union(origin, goal);
                    CarvePassageFrom(goal);
                }
            }
        }
    }

    /// <summary>
    /// Check if the wall can be carved to expend the maze
    /// </summary>
    /// <param name="wall">the wall to test</param>
    /// <param name="floor1"></param>
    /// <param name="floor2"></param>
    /// <returns>boolean indicating if the wall can be carved</returns>
    private bool IsValidWallForMazeExpansion(Tile wall, Tile floor1, Tile floor2) {
        return wall.IsWall() && floor1.IsFloor() && floor2.IsFloor() && !floor1.Room && !floor2.Room &&
               Tile.Find(floor1) != Tile.Find(floor2);
    }

    /// <summary>
    /// Get all the connectors in the map
    /// 
    /// a connector is wall between a room and a wall or another room
    /// </summary>
    /// <param name="map">the map where to retrieve the connector</param>
    /// <returns>a List of tuple, containing coorinates for those connectors</returns>
    private static List<Tile> FindConnectors(Tile[,] map) {
        var connectors = new List<Tile>();
        for (int x = 1; x < map.GetUpperBound(0) - 1; x++) {
            for (int y = 1; y < map.GetUpperBound(1) - 1; y++) {
                if (IsValidConnector(map, x, y)) {
                    var connector = map[x, y];
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
    private static bool IsValidConnector(Tile[,] map, int x, int y) {
        if (map[x, y].IsWall()) {
            if (map[x + 1, y].Type == Type.Floor && map[x - 1, y].Type == Type.Floor && map[x + 1, y].Room != map[x - 1, y].Room ||
               map[x, y + 1].Type == Type.Floor && map[x, y - 1].Type == Type.Floor && map[x, y + 1].Room != map[x, y - 1].Room) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Open some connectors on the map in order to allow access to rooms
    /// </summary>
    /// <param name="connectors">the list of all the connectors of the map</param>
    private void OpenConnectors(IEnumerable<Tile> connectors) {
        foreach (var connector in connectors) {
            // First we find the direction from where the connector open to an already visited area
            Direction dir = Direction.E;
            if (_map[connector.X + DX(dir), connector.Y + DY(dir)].IsWall() &&
                _map[connector.X - DX(dir), connector.Y - DY(dir)].IsWall())
                // we check both wall since previously adjacent connector might have been previously opened
                dir = Direction.N;

            Tile t1 = _map[connector.X + DX(dir), connector.Y + DY(dir)];
            Tile t2 = _map[connector.X - DX(dir), connector.Y - DY(dir)];
            // Then we open the connector if the connector link a visited and an unvisited area
            if (t1.IsFloor() && t2.IsFloor() && Tile.Find(t1) != Tile.Find(t2)) {
                Tile.Union(t1, t2);
                connector.Type = Type.Floor;
                Tile.Union(t1, connector);
            }
        }
    }

    /// <summary>
    /// Open random connectors in order to allow more access to rooms and between corridor
    /// </summary>
    /// <returns>the trnsformed map</returns>
    private void OpenConnectorsRandom() {
        for (int x = 2; x < _map.GetUpperBound(0); x += 2) {
            for (int y = 2; y < _map.GetUpperBound(1); y += 2) {
                if (_map[x, y].IsWall() && IsValidConnector(_map, x, y) && _rnd.NextDouble() < ConnectorRatio) {
                    _map[x, y].Type = Type.Floor;
                }
            }
        }
    }

    /// <summary>
    /// Replace dead ends by walls
    /// </summary>
    private void Uncarve() {
        for (int x = 1; x < _map.GetUpperBound(0); x++) {
            for (int y = 1; y < _map.GetUpperBound(1); y++) {
                if (IsDeadEnd(x, y)) {
                    UncarveDeadEnd(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Uncarve a dead end starting from the given coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>the transformed map</returns>
    private void UncarveDeadEnd(int x, int y) {
        _map[x, y].Type = Type.Wall;  // fill the dead end with wall
        // then we look if an adjacent case is a dead end
        Array directions = Enum.GetValues(typeof(Direction));
        foreach (Direction dir in directions) {
            int x2 = x + DX(dir);
            int y2 = y + DY(dir);
            if (x2 > 0 && x2 < _map.GetUpperBound(0) && y2 > 0 && y2 < _map.GetUpperBound(1) &&
                IsDeadEnd(x2, y2)) {
                UncarveDeadEnd(x2, y2);
                break; // there can be only one adjacent dead end (since the previous tile was a dead end) 
            }
        }
    }

    /// <summary>
    /// Check if the coordinate is a dead end
    /// 
    /// a dead end is a floor with three or more adjacent walls
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsDeadEnd(int x, int y) {
        if (_map[x, y].Room)
            return false;
        try {
            int nbAdjacentWall = NbAdjacentWall(x, y);
            return _map[x, y].IsFloor() && nbAdjacentWall >= 3;
        }
        catch (Exception e) {
            Debug.LogWarning("error in isdeadend, x : " + x + ", y : " + y + " " + e.Message);
            return false;
        }
    }

    private int NbAdjacentWall(int x, int y) {
        return (_map[x - 1, y].IsWall()? 1 : 0) + (_map[x + 1, y].IsWall()? 1 : 0) +
            (_map[x, y - 1].IsWall()? 1 : 0) + (_map[x, y + 1].IsWall()? 1 : 0);
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

    /// <summary>
    /// Shuffle the array
    /// </summary>
    /// <param name="rng">the random generator used</param>
    /// <param name="array">the array to shuffle</param>
    public static Array Shuffle(MyRandom rng, Array array) {
        int n = array.Length;
        while (n > 1) {
            int k = rng.Next(n--);
            var tmp = array.GetValue(n);
            array.SetValue(array.GetValue(k), n);
            array.SetValue(tmp, k);
        }
        return array;
    }

    /// <summary>
    /// Shuffle the list
    /// </summary>
    /// <typeparam name="T">the type of the elements in the list</typeparam>
    /// <param name="rng">the random generator used</param>
    /// <param name="list">the list to shuffle</param>
    public static List<T> Shuffle<T>(MyRandom rng, List<T> list) {
        int n = list.Count;
        while (n > 1) {
            int k = rng.Next(n--);
            var tmp = list[n];
            list[n] = list[k];
            list[k] = tmp;
        }
        return list;
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
                if (_map[x, y].IsWall()) {
                    clone = Instantiate(WallPrefab, new Vector3(x, y, 1f), Quaternion.identity);
                } else if (_map[x, y].IsFloor()) {
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
    /// <param name="go">the gameObject to spawn</param>
    /// <returns>a clone of the gameObject, who spawned in the map</returns>
    public GameObject Spawn(GameObject go) {
        int x, y;
        bool overlapping;
        // find a spot with only floor around
        do {
            x = _rnd.Next(1, _map.GetUpperBound(0));
            y = _rnd.Next(1, _map.GetUpperBound(1));

            var closestGameObject = ClosestGameObject(x, y);
            if (closestGameObject == null) { // there isn't even a gameobject to overlap
                overlapping = false;
            } else {
                overlapping = Vector2.Distance(closestGameObject.transform.position, new Vector2(x, y)) < OverlappingMinDistance;
            }
        } while (_map[x, y].IsWall() || !_map[x, y].Room || NbAdjacentWall(x, y) > 0 || overlapping);

        var clone = Instantiate(go, new Vector2(x, y), Quaternion.identity);
        _spawnedGameObjects.Add(clone);
        return clone;
    }

    /// <summary>
    /// Find the closest gameobject, from previously spawned gameobject, to a point
    /// </summary>
    /// <param name="x">the x coordinates of the point</param>
    /// <param name="y">the y coordinates of the point</param>
    /// <returns>the closest gameobject or null if no gameobjects were previously placed</returns>
    private GameObject ClosestGameObject(int x, int y) {
        if (_spawnedGameObjects.Count == 0)
            return null;
        
        Vector2 position = new Vector2(x, y);
        GameObject res = _spawnedGameObjects[0];
        float minDistance = Vector2.Distance(position, res.transform.position);
        foreach (var go in _spawnedGameObjects) {
            float tmp = Vector2.Distance(position, go.transform.position);
            if (tmp < minDistance) {
                minDistance = tmp;
                res = go;
            }
        }
        return res;
    }
    

    /// <summary>
    /// Spawn a GameObject int the map at a clear area
    /// 
    /// a clear area is a floor case with 4 adjacent floors
    /// </summary>
    /// <param name="exit">the gameObject to spawn</param>
    /// <param name="player">the gameobject of a player, used to spawn the exit a bit far from him</param>
    /// <returns>a clone of the gameObject, who spawned in the map</returns>
    private GameObject SpawnExit(GameObject exit, GameObject player) {
        float minDistance = new Vector2(MapWidth, MapHeight).magnitude / 2f;
        GameObject exitClone = null;
        int nbTry = 0;
        do {
            if(exitClone != null)
                Destroy(exitClone);
            nbTry++;
            if (nbTry % 5 == 0) {
                minDistance--; // minDistance is reduced to allow small maps
            }
            exitClone = Spawn(exit);
        } while(Vector2.Distance(exitClone.transform.position, player.transform.position) < minDistance);
        return exitClone;
    }
}
