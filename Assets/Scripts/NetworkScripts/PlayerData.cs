using UnityEngine;
using UnityEngine.Networking;
using System;

public struct PlayerData {
    public NetworkIdentity id; // An int representing the player, as derived from the PlayerController's NetworkIdentity
    public int length; // The length of the player's snake
    public int x; // The x position of the player's head
    public int y; // The y position of the player's head
    public int color; // The color/team of the player
    public bool spawned; // If player is in game
    public int atkCharge; // Charge of attack power
    public const int MAX_ATK_CHARGE = 15;
    public const int START_LENGTH = 5;

    public PlayerData(NetworkIdentity id, int x, int y, int color) {
        this.id = id;
        this.x = x;
        this.y = y;
        this.color = color;
        this.length = START_LENGTH;
        this.spawned = false;
        this.atkCharge = 0;
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

    public void SetAtkCharge(int value) {
        atkCharge = Math.Min(MAX_ATK_CHARGE, value);
    }

    public void AtkChargeUp() {
        atkCharge = Math.Min(MAX_ATK_CHARGE, atkCharge + 1);
    }
}