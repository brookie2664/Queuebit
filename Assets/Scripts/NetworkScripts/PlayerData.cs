using UnityEngine;

public struct PlayerData {
    public int id;
    public int length;
    public int x;
    public int y;
    public Color color;

    public PlayerData(int id, int x, int y, Color color) {
        this.id = id;
        this.x = x;
        this.y = y;
        this.color = color;
        this.length = 5;
    }

    public void SetX(int x) {
        this.x = x;
    }

    public void SetY(int y) {
        this.y = y;
    }
}