﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
[RequireComponent(typeof(AudioSource))]
public class GameState : NetworkBehaviour
{
    public class PlayerDataSyncList : SyncListStruct<PlayerData> {}
    
    //Move to another file later
    public class GameGridSyncList : SyncListStruct<Cell> {
        //Used to hold cell updates not ready to be pushed to the clients
        private Dictionary<Vector2Int, Cell> queuedUpdates = new Dictionary<Vector2Int, Cell>();
        private int width;
        private int height;

        public void SetDimensions(int width, int height) {
            this.width = width;
            this.height = height;
        }

        //Used to access most current version of a cell, from either the SyncList or the queued updates 
        public Cell GetMostCurrentCell(Vector2 cellPos) {
            return GetMostCurrentCell((int) cellPos.x, (int) cellPos.y);
        }

        //Used to access most current version of a cell, from either the SyncList or the queued updates
        public Cell GetMostCurrentCell(int x, int y) {
            if (x < 0 || x >= width || y < 0 || y >= height) {
                throw new System.Exception("Cannot modify cell at " + x + ", " + y + " becuase it does not exist");
            }
            return queuedUpdates.ContainsKey(new Vector2Int(x, y)) ? queuedUpdates[new Vector2Int(x, y)]: this.GetItem(y * width + x);
        }

        //Used by Queue_x_Update methods to add the update to the queue
        private void QueueCellUpdate(Cell cell) {
            queuedUpdates[new Vector2Int(cell.x, cell.y)] = cell;
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateObstacle(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetObstacle(value);
            QueueCellUpdate(newCell);
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateOccupied(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetOccupied(value);
            QueueCellUpdate(newCell);
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdatePlayer(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetPlayer(value);
            QueueCellUpdate(newCell);
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateDistToTail(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetDistToTail(value);
            QueueCellUpdate(newCell);
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateIsHead(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetIsHead(value);
            QueueCellUpdate(newCell);
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdatePainted(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetPainted(value);
            QueueCellUpdate(newCell);
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateColor(int x, int y, Color value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetColor(value);
            QueueCellUpdate(newCell);
        }

        //Pushes updates in the queued updates to the synclist so they propogate to clients
        public void ApplyUpdates() {
            foreach (KeyValuePair<Vector2Int, Cell> pair in queuedUpdates) {
                Cell cell = pair.Value;
                int index = cell.y * width + cell.x;
                RemoveAt(index);
                Insert(index, cell);
            }
            queuedUpdates.Clear();
        }
    }

    public GameObject settingsObject;
    private GameSettings gameSettings;
    public GameObject localCamera;
    public GameObject localRenderer;

    private int gridWidth;
    private int gridHeight;

    public SyncListInt players = new SyncListInt(); //Holds list of player id's, to be used to reference the list of their data
    public PlayerDataSyncList playerData = new PlayerDataSyncList();

    [SyncVar]
    bool gridCreated = false; //Used by GridRenderer to know when the grid has been filled so
    //it can start checking for updates

    GameGridSyncList data = new GameGridSyncList();

    public bool IsGridCreated() {
        return gridCreated;
    }

    public GameGridSyncList GetData() {
        return data;
    }
    
    //Called by commands from clients to process key input
    public void SendInput(KeyCode input, NetworkIdentity playerId) {
        if (!isServer) {
            return;
        }
        
        //Finds the player's data by cross-ref the playerId with the list of player id's
        int playerDataId = (int)playerId.netId.Value;
        int playerIndex = players.IndexOf(playerDataId);
        PlayerData inputSourceData = playerData.GetItem(playerIndex);

        //Creates a vector to represent intended move direction of player
        Vector2 moveDirection = Vector2.zero;
        if (input == KeyCode.W) moveDirection = Vector2.down;
        else if (input == KeyCode.A) moveDirection = Vector2.left;
        else if (input == KeyCode.S) moveDirection = Vector2.up;
        else if (input == KeyCode.D) moveDirection = Vector2.right; 

        Vector2 playerHeadPos = new Vector2(inputSourceData.x, inputSourceData.y);
        Cell currHeadCell = data.GetMostCurrentCell(playerHeadPos);
        
        //Calculate movement if input is movement
        if (moveDirection != Vector2.zero) {
            Cell targetCell = data.GetMostCurrentCell(playerHeadPos + moveDirection);
            if (!targetCell.obstacle && !targetCell.occupied) {
                //Make all the cell updates needed to move the head of the snake
                data.QueueUpdateColor(targetCell.x, targetCell.y, inputSourceData.color);
                data.QueueUpdateOccupied(targetCell.x, targetCell.y, true);
                data.QueueUpdateIsHead(targetCell.x, targetCell.y, true);
                data.QueueUpdateDistToTail(targetCell.x, targetCell.y, currHeadCell.distToTail + 1);
                data.QueueUpdatePainted(targetCell.x, targetCell.y, true);
                data.QueueUpdatePlayer(targetCell.x, targetCell.y, inputSourceData.id);
                data.QueueUpdateIsHead(currHeadCell.x, currHeadCell.y, false);

                //Update the PlayerData with the new head position
                inputSourceData.SetX(targetCell.x);
                inputSourceData.SetY(targetCell.y);

                //Update variable for later use
                playerHeadPos += moveDirection;

                //Play SFX
                RpcPlayOneShotOnClients(0);
            }

            //Update length of tail after movement
            Cell updatedTargetCell = data.GetMostCurrentCell(playerHeadPos);
            if (updatedTargetCell.distToTail > inputSourceData.length) {
                bool finishedTailUpdate = false;
                Vector2 tailUpdateLocation = playerHeadPos;
                
                //Update distance for each segment of snake
                while (!finishedTailUpdate) {
                    int newDist = data.GetMostCurrentCell(tailUpdateLocation).distToTail - 1;
                    data.QueueUpdateDistToTail((int) tailUpdateLocation.x, (int) tailUpdateLocation.y, newDist);
                    if (newDist < 0) {
                        data.QueueUpdateOccupied((int) tailUpdateLocation.x, (int) tailUpdateLocation.y, false);
                    }

                    //Look for next further segment in all directions
                    Vector2[] directions = new Vector2[]{Vector2.up, Vector2.left, Vector2.down, Vector2.right};
                    bool foundNext = false;
                    foreach (Vector2 searchDir in directions) {
                        Vector2 searchPos = tailUpdateLocation + searchDir;
                        Cell checkCell = data.GetMostCurrentCell(searchPos);
                        if (checkCell.occupied && checkCell.player == inputSourceData.id && checkCell.distToTail == newDist) {
                            foundNext = true;
                            tailUpdateLocation = searchPos;
                            break;
                        }
                    }
                    if (!foundNext) {
                        finishedTailUpdate = true;
                    }
                }
            }
        }

        //VERY IMPORTANT
        //Up until this point, all updates to the grid are being stored in a temporary hashmap.
        //This takes those updates and applies them to the synclist so that any cell updates
        //are applied with only one actual network update per cell
        data.ApplyUpdates();

        //Updates the data of the player that produced the input across all instances of game
        playerData.RemoveAt(playerIndex);
        playerData.Insert(playerIndex, inputSourceData);
        
        //Update's player's camera to focus on new head cell
        playerId.GetComponentInParent<PlayerConnectionComponent>().RpcUpdateCamera(new Vector2Int(inputSourceData.x, inputSourceData.y));
    }

    private Color[] teamColorsTemp = {
        Color.red,
        Color.blue,
        Color.green
    };

    //Dummy variable for team assignment and spawn placement
    int dummy = 0;

    //Called by command from client to register when a player joins
    public void CreatePlayer(NetworkIdentity playerId) {
        int idNum = (int) playerId.netId.Value;
        players.Add(idNum);
        Vector2Int newPlayerPos = new Vector2Int(2, 2 + dummy);
        playerData.Add(new PlayerData(idNum, newPlayerPos.x, newPlayerPos.y, teamColorsTemp[dummy % teamColorsTemp.Length]));
        playerId.GetComponentInParent<PlayerConnectionComponent>().RpcUpdateCamera(new Vector2Int(newPlayerPos.x, newPlayerPos.y));
        dummy++;
    }
    
    // Start is called before the first frame update

    void Start()
    {
        if (!isServer) {
            return;
        }
        
        gameSettings = settingsObject.GetComponent<GameSettings>();
        gridWidth = gameSettings.mapWidth;
        gridHeight = gameSettings.mapHeight;
        data.SetDimensions(gridWidth, gridHeight);

        for (int i = 0; i < gridHeight; i++) {
            for (int j = 0; j < gridWidth; j++) {
                Cell newCell = new Cell(j, i);
                if (i == 0 || i == gridHeight - 1 || j == 0 || j == gridWidth - 1) {
                    newCell.obstacle = true;
                }
                data.Add(newCell);
            }
        }

        gridCreated = true;
        source = this.GetComponent<AudioSource>();
    }

    private AudioSource source;

    public AudioClip[] clips;
    public bool loopingAudio = false;
    public float bpm = 120;
    public double localBGMStartTime;
    //Clips reference:
    //0: Movement SFX
    //1: 
    [ClientRpc]
    void RpcPlayOneShotOnClients(int id) {
        source.PlayOneShot(clips[id]);
    }
    // [ClientRpc]
    // void RpcPlayClipAtPointOnClients(int id, Vector3 soundPosition) {
    //     GameState.source.PlayClipAtPoint(clips[id], soundPosition);
    //     //AudioClip[] must be the same on all clients.
    // }

    [ClientRpc]
    void RpcSendBGMStartToClients(bool loopingAudio, int id) {
        
        //Starts audio
        if (loopingAudio == true) {
            //Do loop logic here?
        } 
        else {
            source.PlayOneShot(clips[id]);
        }
        localBGMStartTime = AudioSettings.dspTime;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
