using UnityEngine;
using UnityEngine.Networking;

public struct PlayerData {
    public NetworkIdentity id; //An int representing the player, as derived from the PlayerController's NetworkIdentity
    public int length; //The length of the player's snake
    public int x; //The x position of the player's head
    public int y; //The y position of the player's head
    public Color color; //The color/team of the player
    public bool spawned; //If player is in game

    public PlayerData(NetworkIdentity id, int x, int y, Color color) {
        this.id = id;
        this.x = x;
        this.y = y;
        this.color = color;
        this.length = 5;
        this.spawned = false;
    }

    public void SetX(int x) {
        this.x = x;
    }

    public void SetY(int y) {
        this.y = y;
    }
    
    public void SetLength(int value) {
        this.length = value;
    }

    public void SetSpawned(bool value) {
        spawned = value;
    }
}