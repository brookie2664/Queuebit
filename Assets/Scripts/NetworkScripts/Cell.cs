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
    // 5 for weapon
    public int type;
    public bool occupied; // True if occupied by a player
    public NetworkIdentity player; // Player occupying the cell, if occupied
    public int life; // Distance to tail for length purposes
    public bool isHead; // True if the cell is the head of a snake
    public bool painted; // True if the tile is painted
    public int color; // Color painted in cell, or of occupying player
    public int cache; // Value of cache if cell is a cache, between 0-15
    public int weaponTimer; // Time until weapon is available
    public int weapon; // Available weapon
    // 0 for splash
    // 1 for sniper
    public const int MAX_CACHE = 15;
    public const int WEAPON_DROP_TIME = 40;
    public const int NUM_WEAPONS = 2;

    public Cell(int x, int y) : this() {
        this.x = x;
        this.y = y;
        this.weaponTimer = WEAPON_DROP_TIME;
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

    public void SetLife(int value) {
        life = value;
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
        cache = System.Math.Min(cache + 1, MAX_CACHE);
    }

    public void SetWeaponTimer(int value) {
        bool available = weaponTimer == 0;
        weaponTimer = System.Math.Max(value, 0);
        if (!available && weaponTimer == 0) {
            PickRandWeapon();
        }
    }

    public void TickWeaponTimer() {
        SetWeaponTimer(weaponTimer - 1);
    }

    public void PickRandWeapon() {
        weapon = Util.random.Next(NUM_WEAPONS);
    }
}