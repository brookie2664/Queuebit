using UnityEngine;

public struct PlayerData {
    public int id; //An int representing the player, as derived from the PlayerController's NetworkIdentity
    public int length; //The length of the player's snake
    public int x; //The x position of the player's head
    public int y; //The y position of the player's head
    public Color color; //The color/team of the player

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