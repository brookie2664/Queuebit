using UnityEngine;

public class Cell {


    private int team; //The team "number" the tile is owned by, perhaps an enum could replace this
    private float score; //The strength of the color on the tile, 0 for no color
    private bool obstacle; //True if the tile is an obstacle tile, false if it is a floor tile
    private bool pickup; //Placeholder for when pickup objects exist
    private bool character; //Placeholder for when we figure out what to do with player collisiion and other things
    private GameObject render; //The physical unity object of the cell. Is given at runtime.

    public Cell() {
        team = 0;
        score = 0;
        obstacle = false;
        pickup = false;
        character = false;
    }

    public Cell(bool isObstacle) {
        team = 0;
        score = 0;
        obstacle = isObstacle;
        pickup = false;
        character = false;
    }

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

    public void SetRender(GameObject o) {
        render = o;
    }
    
    public int GetTeam() {
        return team;
    }

    public float GetScore() {
        return score;
    }

    public bool GetObstacle() {
        return obstacle;
    }

    public bool GetPickup() {
        return pickup;
    }

    public bool GetCharacter() {
        return character;
    }

    public GameObject GetRender() {
        return render;
    }
}