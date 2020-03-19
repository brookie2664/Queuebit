using UnityEngine;
using UnityEngine.Networking;

public struct Cell {

    public int x; // X position of cell
    public int y; // Y position of cell

    // 0 for empty
    // 1 for obstacle
    // 2 for ground
    // 3 for spawn
    // 4 for cache
    public int type;
    public bool occupied; // True if occupied by a player
    public NetworkIdentity player; // Player occupying the cell, if occupied
    public int distToTail; // Number of cells to traverse to head of snake
    public bool isHead; // True if the cell is the head of a snake
    public bool painted; // True if the tile is painted
    public int color; // Color painted in cell, or of occupying player
    public int cache; // Value of cache if cell is a cache, between 0-15

    public Cell(int x, int y) : this() {
        this.x = x;
        this.y = y;
    }

    public void SetType(int value) {
        type = value;
    }

    public void SetOccupied(bool value) {
        occupied = value;
    }

    public void SetPlayer(NetworkIdentity value) {
        player = value;
    }

    public void SetDistToTail(int value) {
        distToTail = value;
    }

    public void SetIsHead(bool value) {
        isHead = value;
    }

    public void SetPainted(bool value) {
        painted = value;
    }

    public void SetColor(int value) {
        color = value;
    }

    public void SetCache(int value) {
        cache = value;
    }

    public void IncrementCache() {
        cache = System.Math.Min(cache + 1, 15);
    }

    // public static bool operator ==(Cell lhs, Cell rhs) {
    //     // Check for null on left side.
    //     return lhs.x == rhs.x && lhs.y == rhs.y;
    // }

    // public static bool operator !=(Cell lhs, Cell rhs) {
    //     return !(lhs == rhs);
    // }

    // public override int GetHashCode() {
    //     int prime = 13;
    //     int result = 1;
    //     result = result * prime + x;
    //     result = result * prime + y;
    //     return result;
    // }
}