using UnityEngine;

public struct Cell {


    public int team; //The team "number" the tile is owned by, perhaps an enum could replace this
    public float score; //The strength of the color on the tile, 0 for no color
    public bool obstacle; //True if the tile is an obstacle tile, false if it is a floor tile
    public bool pickup; //Placeholder for when pickup objects exist
    public bool character; //Placeholder for when we figure out what to do with player collisiion and other things
    public GameObject render; //The physical unity object of the cell. Is given at runtime.

    /*
    public Cell() {
        team = 0;
        score = 0;
        obstacle = false;
        pickup = false;
        character = false;
    }
    */

    public Cell(bool isObstacle) {
        team = 0;
        score = 0;
        obstacle = isObstacle;
        pickup = false;
        character = false;
        render = null;
    }

    /*
    //Method called when a something attempts to ink a tile
    public void AddColor(int faction, float strength) {
        if (obstacle) {
            return;
        }
        if (team == faction) {
            score = System.Math.Max(score, strength);
        } else {
            team = faction;
            score = strength;
        }
    }
    */
}