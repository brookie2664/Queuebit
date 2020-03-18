using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour
{
    public class PlayerDataSyncList : SyncListStruct<PlayerData> {
        public PlayerData GetPlayer(NetworkIdentity id) {
            foreach (PlayerData data in this) {
                if (data.id.Equals(id)) {
                    return data;
                }
            }
            throw new System.Exception("Could not find specified player");
        }

        public void UpdatePlayer(PlayerData data) {
            foreach (PlayerData listData in this) {
                if (listData.id.Equals(data.id)) {
                    this.Remove(listData);
                    this.Add(data);
                    return;
                }
            }
            throw new System.Exception("Could not find specified player");
        }
    }
    
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
                throw new System.Exception("Cannot get cell at " + x + ", " + y + " becuase it does not exist");
            }
            return queuedUpdates.ContainsKey(new Vector2Int(x, y)) ? queuedUpdates[new Vector2Int(x, y)]: this.GetItem(y * width + x);
        }

        //Used by Queue_x_Update methods to add the update to the queue
        private void QueueCellUpdate(Cell cell) {
            queuedUpdates[new Vector2Int(cell.x, cell.y)] = cell;
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateType(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetType(value);
            QueueCellUpdate(newCell);
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateOccupied(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetOccupied(value);
            QueueCellUpdate(newCell);
        }

        //This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdatePlayer(int x, int y, NetworkIdentity value) {
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

        public void QueueUpdateCache(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetCache(value);
            QueueCellUpdate(newCell);
        }

        public void QueueUpdateCache(int x, int y) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.IncrementCache();
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
    
    public PlayerDataSyncList playerData = new PlayerDataSyncList();

    [SyncVar]
    bool gridCreated = false; //Used by GridRenderer to know when the grid has been filled so
    //it can start checking for updates

    System.Random rand = new System.Random();

    List<Vector2Int> cacheLocations = new List<Vector2Int>();
    List<Vector2Int> spawnPoints = new List<Vector2Int>();

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
        PlayerData inputSourceData = playerData.GetPlayer(playerId);

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
            if (targetCell.type != 1 && !targetCell.occupied) {
                //Make all the cell updates needed to move the head of the snake
                data.QueueUpdateColor(targetCell.x, targetCell.y, inputSourceData.color);
                data.QueueUpdateOccupied(targetCell.x, targetCell.y, true);
                data.QueueUpdateIsHead(targetCell.x, targetCell.y, true);
                data.QueueUpdateDistToTail(targetCell.x, targetCell.y, currHeadCell.distToTail + 1);
                data.QueueUpdatePainted(targetCell.x, targetCell.y, true);
                data.QueueUpdatePlayer(targetCell.x, targetCell.y, inputSourceData.id);
                data.QueueUpdateIsHead(currHeadCell.x, currHeadCell.y, false);

                if (targetCell.type == 4 && targetCell.cache >= 15) {
                    inputSourceData.SetLength(inputSourceData.length + 3);
                    data.QueueUpdateCache(targetCell.x, targetCell.y, 0);
                }

                //Update the PlayerData with the new head position
                inputSourceData.SetX(targetCell.x);
                inputSourceData.SetY(targetCell.y);

                //Update variable for later use
                playerHeadPos += moveDirection;
            } else {
                inputSourceData.SetLength(System.Math.Max(inputSourceData.length - 1, 0));
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
        playerData.UpdatePlayer(inputSourceData);
        
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
        playerData.Add(new PlayerData(playerId, 0, 0, teamColorsTemp[dummy++ % teamColorsTemp.Length]));
        SpawnPlayer(playerId);
    }

    public void SpawnPlayer(NetworkIdentity playerId) {
        PlayerData player = playerData.GetPlayer(playerId);
        if (player.spawned) {
            return;
        }

        int randStart = rand.Next(spawnPoints.Count);
        for (int i = 0; i < spawnPoints.Count; i++) {
            Vector2Int spawn = spawnPoints[(randStart + i) % spawnPoints.Count];
            if (!data.GetMostCurrentCell(spawn).occupied) {
                Debug.Log("Spawning Player at: " + spawn);
                player.SetSpawned(true);
                player.SetX(spawn.x);
                player.SetY(spawn.y);
                data.QueueUpdateIsHead(spawn.x, spawn.y, true);
                data.QueueUpdateColor(spawn.x, spawn.y, player.color);
                data.QueueUpdateOccupied(spawn.x, spawn.y, true);
                data.QueueUpdatePlayer(spawn.x, spawn.y, playerId);
                data.QueueUpdatePainted(spawn.x, spawn.y, true);
                data.ApplyUpdates();
                playerData.UpdatePlayer(player);
                playerId.GetComponentInParent<PlayerConnectionComponent>().RpcUpdateCamera(new Vector2Int(spawn.x, spawn.y));
        
                return;
            }
        }
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (!isServer) {
            return;
        }
        
        gameSettings = settingsObject.GetComponent<GameSettings>();
        int[, ] map = gameSettings.map;
        int gridHeight = map.GetLength(0);
        int gridWidth = map.GetLength(1);
        data.SetDimensions(gridWidth, gridHeight);

        for (int i = 0; i < gridHeight; i++) {
            for (int j = 0; j < gridWidth; j++) {
                Cell newCell = new Cell(j, i);
                int type = map[i, j];
                newCell.type = type;
                if (type == 3) spawnPoints.Add(new Vector2Int(j, i));
                if (type == 4) cacheLocations.Add(new Vector2Int(j, i));
                data.Add(newCell);
            }
        }

        gridCreated = true;
    }

    float counter = 0;
    float updateInterval = 1f;

    // Update is called once per frame
    void Update()
    {
        if (!isServer || !gridCreated) {
            return;
        }
        
        counter += Time.deltaTime;

        if (counter >= updateInterval) {
            counter -= updateInterval;
            Debug.Log("Updating serverside");

            //Run server managed updates
            foreach(Vector2Int cache in cacheLocations) {
                Cell cacheCell = data.GetMostCurrentCell(cache);
                if (!cacheCell.occupied) {
                    data.QueueUpdateCache(cache.x, cache.y);
                    Debug.Log("Charging at " + cache);
                }
            }
            data.ApplyUpdates();

        }
    }
}
