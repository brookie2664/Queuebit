﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject settingsObject;
    private GameSettings gameSettings;
    public GameObject gameStateObject;
    private GameState gameState;
    
    private int mapHeight;
    private int mapWidth;
    private bool gridUpdateConnected;

    private GameObject[,] map;
    public GameObject floorPrefab;
    public GameObject obstaclePrefab;

    //Used by callback to update the rendering of an individual cell based on cell data
    void UpdateCell(Cell data) {
        GameObject sprite = map[data.x, data.y];
        SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
        if (data.obstacle == true) {
            renderer.color = Color.white;
        } else if (data.isHead) {
            renderer.color = data.color;
        } else if (data.occupied) {
            renderer.color = data.color * .9f;
        } else if (data.painted) {
            renderer.color = data.color * .66f;
        } else {
            renderer.color = Color.gray;
        }
    }

    //Used for updating camera position
    public GameObject GetCellRenderAt(Vector2 position) {
        return map[(int) position.x, (int) position.y];
    }

    // Start is called before the first frame update
    void Start()
    {
        gameSettings = settingsObject.GetComponent<GameSettings>();
        gameState = gameStateObject.GetComponent<GameState>();
        mapHeight = gameSettings.mapHeight;
        mapWidth = gameSettings.mapWidth;

        map = new GameObject[mapHeight, mapWidth];
        for (int i = 0; i < mapHeight; i++) {
            for (int j = 0; j < mapWidth; j++) {
                map[i, j] = Instantiate(cellPrefab, new Vector3(1.2f * i, -1.2f * j, 0), Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Wait for grid to be started on the server before setting up callback and performing intial update
        if (!gridUpdateConnected && gameState.IsGridCreated()) {
            gridUpdateConnected = true;
            foreach (Cell entry in gameState.GetData()) {
                UpdateCell(entry);
            }
            //Set callback for when a cell gets updated on the server
            gameState.GetData().Callback = (op, index) => {
                //Only if the update is an addition to the list
                if (op.Equals(GameState.GameGridSyncList.Operation.OP_INSERT)) {
                    Cell cellData = gameState.GetData().GetItem(index);
                    UpdateCell(cellData);
                }
            };
        }
    }
}
