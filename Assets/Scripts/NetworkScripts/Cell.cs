using UnityEngine;

public struct Cell {

    public int x; //X position of cell
    public int y; //Y position of cell
    public bool obstacle; //True if the tile is an obstacle tile, false if it is a floor tile
    public bool occupied; //True if occupied by a player
    public int player; //Player occupying the cell, if occupied
    public bool painted; //True if the tile is painted
    public Color color; //Color painted in cell, or of occupying player

    public Cell(int x, int y) : this() {
        this.x = x;
        this.y = y;
    }

    public void SetObstacle(bool value) {
        obstacle = value;
    }

    public void SetOccupied(bool value) {
        occupied = value;
    }

    public void SetPlayer(int value) {
        player = value;
    }

    public void SetPainted(bool value) {
        painted = value;
    }

    public void SetColor(Color value) {
        color = value;
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