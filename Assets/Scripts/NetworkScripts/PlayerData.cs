using UnityEngine;

public struct PlayerData {
    public int x;
    public int y;
    public Color color;

    public PlayerData(int x, int y, Color color) {
        this.x = x;
        this.y = y;
        this.color = color;
    }

    public void SetX(int x) {
        this.x = x;
    }

    public void SetY(int y) {
        this.y = y;
    }
}