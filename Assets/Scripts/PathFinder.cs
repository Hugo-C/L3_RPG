using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinder {
    private static Tile[,] _map;

    public static void Init(Tile[,] map) {
        _map = map;
    }


    private static double Heuristic(Tile a, Tile b) {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    /// <summary>
    /// Calculate the path from a tile to another using the A* algorithm
    /// </summary>
    /// <param name="start"></param>
    /// <param name="goal"></param>
    /// <returns>an empty list if no path was found else the list of tiles from the start tile to the foal tile </returns>
    public static List<Tile> Path(Tile start, Tile goal) {
        if (_map == null)
            Debug.LogError("the pathfinder need to be initialized before the first use");


        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, double> costSoFar = new Dictionary<Tile, double>();
        var frontier = new PriorityQueue<Tile>();
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0) {
            var current = frontier.Dequeue();

            if (current.Equals(goal)) {
                return DictToPath(cameFrom, start, goal);  // we found a path
            }

            foreach (var next in Neighbors(current)) {
                double newCost = costSoFar[current] + Cost(current, next);
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next]) {
                    costSoFar[next] = newCost;
                    double priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        return new List<Tile>();  // no path were found
    }

    private static List<Tile> DictToPath(Dictionary<Tile, Tile> cameFrom, Tile start, Tile goal) {
        var res = new List<Tile> {goal};
        var current = goal;
        
        while (current != start) {
            current = cameFrom[current];
            res.Add(current);
        }

        res.Reverse();
        return res;
    }

    /// <summary>
    /// Return the list of T's neighbors
    /// </summary>
    /// <param name="T"></param>
    /// <returns></returns>
    private static List<Tile> Neighbors(Tile T) {
        var res = new List<Tile>();
        if (T.X + 1 <= _map.GetUpperBound(0) && _map[T.X + 1, T.Y].IsFloor())
            res.Add(_map[T.X + 1, T.Y]);
        if (T.X - 1 >= _map.GetLowerBound(0) && _map[T.X - 1, T.Y].IsFloor())
            res.Add(_map[T.X - 1, T.Y]);
        if (T.Y + 1 <= _map.GetUpperBound(1) && _map[T.X, T.Y + 1].IsFloor())
            res.Add(_map[T.X, T.Y + 1]);
        if (T.Y - 1 >= _map.GetLowerBound(1) && _map[T.X, T.Y - 1].IsFloor())
            res.Add(_map[T.X, T.Y - 1]);
        
        // diagonal
        if(T.X + 1 <= _map.GetUpperBound(0) && T.Y + 1 <= _map.GetUpperBound(1) && 
        res.Contains(_map[T.X + 1, T.Y]) && res.Contains(_map[T.X, T.Y + 1]) && _map[T.X + 1, T.Y + 1].IsFloor())
            res.Add(_map[T.X + 1, T.Y + 1]);
        if(T.X + 1 <= _map.GetUpperBound(0) && T.Y - 1 >= _map.GetLowerBound(1) && 
        res.Contains(_map[T.X + 1, T.Y]) && res.Contains(_map[T.X, T.Y - 1]) && _map[T.X + 1, T.Y - 1].IsFloor())
            res.Add(_map[T.X + 1, T.Y - 1]);
        if(T.X - 1 >= _map.GetLowerBound(0) && T.Y - 1 >= _map.GetLowerBound(1) && 
           res.Contains(_map[T.X - 1, T.Y]) && res.Contains(_map[T.X, T.Y - 1]) && _map[T.X - 1, T.Y - 1].IsFloor())
            res.Add(_map[T.X - 1, T.Y - 1]);
        if(T.X - 1 >= _map.GetLowerBound(0) && T.Y + 1 <= _map.GetUpperBound(1) && 
        res.Contains(_map[T.X - 1, T.Y]) && res.Contains(_map[T.X, T.Y + 1]) &&_map[T.X - 1, T.Y + 1].IsFloor())
            res.Add(_map[T.X - 1, T.Y + 1]);
        return res;
    }

    /// <summary>
    /// Return the cost to go from the start tile to the end tile
    ///
    /// The cost return is the distance power 2
    /// </summary>
    /// <param name="start">The start tile</param>
    /// <param name="end">the end tile, supposed to be a neighbor of start</param>
    /// <returns>The movement cost</returns>
    private static double Cost(Tile start, Tile end) {
        return (start.X - end.X) * (start.X - end.X) + (start.Y - end.Y) * (start.Y - end.Y);
    }
}