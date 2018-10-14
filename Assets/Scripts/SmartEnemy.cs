using System;
using System.Collections.Generic;
using UnityEngine;

public class SmartEnemy : Enemy {

    private Tile[,] _map;
    private const float Step = MoveCoef * 2f;
    
    private new void Start() {
        _map = FindObjectOfType<LevelGenerator>().Map;
        if(_map == null)
            Debug.LogWarning("no map found");
        base.Start();
    }

    private void Update() {
        if (!IsAgro && Vector2.Distance(Player.transform.position, transform.position) < 10f)
            IsAgro = true;

        float horizontal, vertical;
        NextMove(out horizontal, out vertical);
        //Debug.Log("hor : " + horizontal + ", ver : " + vertical);
        if (!Mathf.Approximately(horizontal, 0f) || !Mathf.Approximately(vertical, 0f)) {
            AttemptMove(horizontal, vertical, CollidingTag);
        }
    }

    private void NextMove(out float horizontal, out float vertical) {
        horizontal = 0f;
        vertical = 0f;
        if (IsAgro) {
            Tile playerTile = _map[Mathf.RoundToInt(Player.transform.position.x), Mathf.RoundToInt(Player.transform.position.y)];
            Tile myTile = _map[Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)];
            List<Tile> path = PathFinder.Path(myTile, playerTile);

            if (path.Count <= 1) {
                //Debug.LogWarning("no path found from : " + myTile + ", to : " + playerTile + ", both floor : " + (myTile.IsFloor() && playerTile.IsFloor()));
                return;
            }
            
            Tile next = path[1];  // the next tile to move to
            horizontal = SmootherMovement(next.X - transform.position.x, Step);
            vertical = SmootherMovement(next.Y - transform.position.y, Step);
        }
    }

    private static float SmootherMovement(float value, float bound) {
        if (Math.Abs(value) < bound)
            return 0f;
        return Mathf.Sign(value) * bound;
    }
}