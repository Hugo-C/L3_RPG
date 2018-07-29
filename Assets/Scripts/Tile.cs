
public enum Type {
    Wall,
    Floor
}

public class Tile {
    
    public readonly int X;
    public readonly int Y;
    public Type Type;
    private int _rank;
    private Tile _parent;
    public bool Room;

    /// <summary>
    /// Create a new Tile with coordinates
    /// </summary>
    /// <param name="x">coordinates x of the tile</param>
    /// <param name="y">coordinates y of the tile</param>
    /// <param name="type">type of the tile (floor, wall, ...)</param>
    /// <param name="room">is the tile a part of a room</param>
    public Tile(int x, int y, Type type, bool room = false) {
        X = x;
        Y = y;
        Type = type;
        _parent = this; 
        _rank = 0;  // a new tile is the only member of it's group
        Room = room;
    }

    /// <summary>
    /// Check if the tile is a wall
    /// </summary>
    /// <returns></returns>
    public bool IsWall() {
        return Type == Type.Wall;
    }
    
    /// <summary>
    /// Check if the tile is a floor
    /// </summary>
    /// <returns></returns>
    public bool IsFloor() {
        return Type == Type.Floor;
    }
    
    /// <summary>
    /// Find the head of the group of the tile
    /// </summary>
    /// <param name="t">The tile from which we want his representative member</param>
    /// <returns>The representative member of the group</returns>
    public static Tile Find(Tile t) {
        if (t != t._parent)
            t._parent = Find(t._parent);
        return t._parent;
    }

    /// <summary>
    /// Regroup both heads of group where tiles t1 and t2 are
    /// </summary>
    /// <param name="t1">a Tile of the first group to be merged</param>
    /// <param name="t2">a Tile of the second group to be merged</param>
    public static void Union(Tile t1, Tile t2) {
        Tile parent1 = Find(t1);
        Tile parent2 = Find(t2);
        if (parent1 != parent2)
            Link(parent1, parent2);
    }
    
    /// <summary>
    /// Link two tiles directly, those tiles HAVE to be heads of theirs groups
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
     private static void Link(Tile t1, Tile t2) {
        if (t1._rank > t2._rank) {
            t2._parent = t1;
        } else {
            t1._parent = t2;
            if (t1._rank == t2._rank)
                t2._rank++;
        }
    }
}
