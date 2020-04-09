using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour
{
    // public static GameState gameState {get; private set;}

    // void Awake() {
    //     if (gameState == null) {
    //         gameState = this;
    //     } else {
    //         Destroy(gameObject);
    //     }
    // }
    
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
    
    // Move to another file later
    public class GameGridSyncList : SyncListStruct<Cell> {
        // Used to hold cell updates not ready to be pushed to the clients
        private Dictionary<Vector2Int, Cell> queuedUpdates = new Dictionary<Vector2Int, Cell>();
        private int width;
        private int height;

        // Must be called for the init grid initialization to work
        public void SetDimensions(int width, int height) {
            this.width = width;
            this.height = height;
        }

        // Boolean to test if a cell exists
        public bool HasCellAt(Vector2Int pos) {
            return HasCellAt(pos.x, pos.y);
        }

        // Boolean to test if a cell exists
        public bool HasCellAt(int x, int y) {
            return !(x < 0 || x >= width || y < 0 || y >= height);
        }

        // Used to access most current version of a cell, from either the SyncList or the queued updates 
        public Cell GetMostCurrentCell(Vector2 cellPos) {
            return GetMostCurrentCell((int) cellPos.x, (int) cellPos.y);
        }

        // Used to access most current version of a cell, from either the SyncList or the queued updates
        public Cell GetMostCurrentCell(int x, int y) {
            if (x < 0 || x >= width || y < 0 || y >= height) {
                throw new System.Exception("Cannot get cell at " + x + ", " + y + " becuase it does not exist");
            }
            return queuedUpdates.ContainsKey(new Vector2Int(x, y)) ? queuedUpdates[new Vector2Int(x, y)]: this.GetItem(y * width + x);
        }

        // Used by Queue_x_Update methods to add the update to the queue
        private void QueueCellUpdate(Cell cell) {
            queuedUpdates[new Vector2Int(cell.x, cell.y)] = cell;
        }

        // This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateType(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetType(value);
            QueueCellUpdate(newCell);
        }

        // This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateOccupied(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetOccupied(value);
            QueueCellUpdate(newCell);
        }

        // This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdatePlayer(int x, int y, NetworkIdentity value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetPlayer(value);
            QueueCellUpdate(newCell);
        }

        // This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateDistToTail(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetLife(value);
            QueueCellUpdate(newCell);
        }

        // This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateIsHead(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetIsHead(value);
            QueueCellUpdate(newCell);
        }

        // This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdatePainted(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetPainted(value);
            QueueCellUpdate(newCell);
        }

        // This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateColor(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetColor(value);
            QueueCellUpdate(newCell);
        }

        // This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateCache(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetCache(value);
            QueueCellUpdate(newCell);
        }

        // This should be used to update cells, in conjunction with ApplyUpdates()
        public void QueueUpdateCache(int x, int y) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.IncrementCache();
            QueueCellUpdate(newCell);
        }

        public void QueueUpdateWeaponTimer(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetWeaponTimer(value);
            QueueCellUpdate(newCell);
        }

        public void QueueTickWeaponTimer(int x, int y) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.TickWeaponTimer();
            QueueCellUpdate(newCell);
        }

        // Pushes updates in the queued updates to the synclist so they propogate to clients
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
    bool gameLoaded = false; // Used by GridRenderer to know when the grid has been filled so
    // it can start checking for updates

    System.Random rand = new System.Random();

    List<Vector2Int> cacheLocations = new List<Vector2Int>();
    List<Vector2Int> spawnPoints = new List<Vector2Int>();
    List<Vector2Int> weaponPoints = new List<Vector2Int>();

    GameGridSyncList data = new GameGridSyncList();

    [SerializeField]
    private bool debugAllowSinglePlayer = false;

    // Used by other objects to determine whether they can start using the grid
    public bool IsGameLoaded() {
        return gameLoaded;
    }

    public GameGridSyncList GetData() {
        return data;
    }

    // Sets player to not spawned and removes from board
    // Can also be used on disconnect
    // Need to run data.ApplyUpdates() to be shown to network
    private void KillPlayer(NetworkIdentity playerId) {
        PlayerData player = playerData.GetPlayer(playerId);
        bool finishedKillUpdate = false;
        Vector2Int updateLocation = new Vector2Int(player.x, player.y);
        
        // Update each segment of snake to be unoccupied
        while (!finishedKillUpdate) {

            int nextSearch = data.GetMostCurrentCell(updateLocation).life - 1;
            data.QueueUpdateOccupied(updateLocation.x, updateLocation.y, false);

            // Look for next further segment in all directions
            Vector2Int[] directions = new Vector2Int[]{Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right};
            bool foundNext = false;
            foreach (Vector2Int searchDir in directions) {
                Vector2Int searchPos = updateLocation + searchDir;
                Cell checkCell = data.GetMostCurrentCell(searchPos);
                if (checkCell.occupied && checkCell.player == player.id && checkCell.life == nextSearch) {
                    foundNext = true;
                    updateLocation = searchPos;
                    break;
                }
            }
            if (!foundNext) {
                finishedKillUpdate = true;
            }
        }

        player.SetSpawned(false);
        player.beatsToRespawn = 8;
        PlayerConnectionComponent playerComponent = playerId.GetComponent<PlayerConnectionComponent>();
        playerComponent.RpcShowWarning("You died!");
        playerComponent.RpcStartTimer("Respawn in..", MusicManager.musicManager.beatLength * 8);
        playerData.UpdatePlayer(player);
    }

    // Adjusts length of snake to match length property of its player
    // Need to run data.ApplyUpdates() to be shown to network
    private void updateSnakeCells(PlayerData player) {
        bool finishedTailUpdate = false;
        List<Vector2Int> cellsUpdated = new List<Vector2Int>(); 
        Vector2Int updateLocation = new Vector2Int(player.x, player.y);

        // Update distance for each segment of snake
        while (!finishedTailUpdate) {

            int nextSearch = data.GetMostCurrentCell(updateLocation).life - 1;
            int newLife = player.length - cellsUpdated.Count;
            data.QueueUpdateDistToTail(updateLocation.x, updateLocation.y, newLife);
            if (newLife < 0) {
                data.QueueUpdateOccupied(updateLocation.x, updateLocation.y, false);
            }
            cellsUpdated.Add(updateLocation);

            // Look for next further segment in all directions
            Vector2Int[] directions = new Vector2Int[]{Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right};
            bool foundNext = false;
            foreach (Vector2Int searchDir in directions) {
                Vector2Int searchPos = updateLocation + searchDir;
                Cell checkCell = data.GetMostCurrentCell(searchPos);
                if (!cellsUpdated.Contains(searchPos) && checkCell.occupied && checkCell.player == player.id && checkCell.life == nextSearch) {
                    foundNext = true;
                    updateLocation = searchPos;
                    break;
                }
            }
            if (!foundNext) {
                finishedTailUpdate = true;
            }
        }
    }
    
    // Called by commands from clients to process key input
    public void SendInput(KeyCode input, NetworkIdentity playerId) {
        if (!isServer) {
            return;
        }
        
        if (!MusicManager.musicManager.beatsStarted) {
            return;
        }
        
        // Finds the player's data by cross-ref the playerId with the list of player id's
        PlayerData inputSourceData = playerData.GetPlayer(playerId);

        PlayerConnectionComponent playerComponent = playerId.GetComponent<PlayerConnectionComponent>();

        if (!inputSourceData.spawned) return;

        if (!inputSourceData.movedThisTurn) {

            // Creates a vector to represent intended move direction of player
            Vector2Int moveDirection = Vector2Int.zero;
            Vector2Int playerHeadPos = new Vector2Int(inputSourceData.x, inputSourceData.y);
            if (input == KeyCode.W) moveDirection = Vector2Int.down;
            else if (input == KeyCode.A) moveDirection = Vector2Int.left;
            else if (input == KeyCode.S) moveDirection = Vector2Int.up;
            else if (input == KeyCode.D) moveDirection = Vector2Int.right;
            else if (input == KeyCode.Space && inputSourceData.atkCharge != 0) {
                // Attack Handling
                
                List<Vector2Int> cellsToDamage = new List<Vector2Int>();

                switch (inputSourceData.weapon) {
                
                    case 0: // Find attack cells for splash

                        int atkRadius = AttackTable.getAtkLevel(inputSourceData.weapon, inputSourceData.length, inputSourceData.atkCharge);
                        
                        // Find all the cells to paint, paint if unoccupied, record the cell otherwise
                        for (int i = -atkRadius; i <= atkRadius; i++) {
                            int height = atkRadius - Math.Abs(i);
                            for (int j = -height; j <= height; j++) {
                                Vector2Int testPos = playerHeadPos + new Vector2Int(i, j);
                                if (data.HasCellAt(testPos)) {
                                    if (!data.GetMostCurrentCell(testPos).occupied) {
                                        data.QueueUpdateColor(testPos.x, testPos.y, inputSourceData.color);
                                    } else {
                                        cellsToDamage.Add(testPos);
                                    }
                                }
                            }
                        }
                        break;
                    case 1: // Find attack cells for sniper
                        
                        int atkLength = AttackTable.getAtkLevel(inputSourceData.weapon, inputSourceData.length, inputSourceData.atkCharge);
                        
                        if (atkLength < 1) break;

                        // Find all the cells to paint, paint if unoccupied, record the cell otherwise
                        for (int i = -1; i <= 1; i++) {
                            for (int j = 0 - 1 + Math.Abs(i); j <= atkLength - Math.Abs(i); j++) {
                                Vector2Int directionalizedVector;
                                Vector2 lastDirection = inputSourceData.lastMoveDirection;
                                if (lastDirection == Vector2.up) {
                                    directionalizedVector = new Vector2Int(i, j);
                                } else if (lastDirection == Vector2.right) {
                                    directionalizedVector = new Vector2Int(j, i);
                                } else if (lastDirection == Vector2.down) {
                                    directionalizedVector = new Vector2Int(i, -j);
                                } else if (lastDirection == Vector2.left) {
                                    directionalizedVector = new Vector2Int(-j, i);
                                } else {
                                    directionalizedVector = Vector2Int.zero;
                                }
                                Vector2Int testPos = playerHeadPos + directionalizedVector;
                                if (data.HasCellAt(testPos)) {
                                    if (!data.GetMostCurrentCell(testPos).occupied) {
                                        data.QueueUpdateColor(testPos.x, testPos.y, inputSourceData.color);
                                    } else {
                                        cellsToDamage.Add(testPos);
                                    }
                                }
                            }
                        }
                        break;
                }

                Dictionary<NetworkIdentity, int> damages = new Dictionary<NetworkIdentity, int>();

                // Loop through painted cells that are occupied and tally damages to appropriate players
                foreach (Vector2Int pos in cellsToDamage) {
                    Cell cell = data.GetMostCurrentCell(pos);
                    // Check cell is enemy
                    if (cell.color != inputSourceData.color) {
                        if (!damages.ContainsKey(cell.player)) {
                            damages[cell.player] = 0;
                        }
                        damages[cell.player] = damages[cell.player] + 1;
                    }
                }

                // Apply tallied damages
                foreach (NetworkIdentity damagedPlayerId in damages.Keys) {
                    PlayerData player = playerData.GetPlayer(damagedPlayerId);
                    if (damages[damagedPlayerId] <= player.length) {
                        player.SetLength(player.length - damages[damagedPlayerId]);
                        playerData.UpdatePlayer(player);
                        updateSnakeCells(player);
                    } else {
                        KillPlayer(damagedPlayerId);
                    }
                    player.id.GetComponent<PlayerConnectionComponent>().RpcUpdateAtkIndicator(AttackTable.getAtkLevel(0, player.length, player.atkCharge), player.lastMoveDirection);
                }

                // Recheck cells to paint if no longer occupied
                foreach (Vector2Int pos in cellsToDamage) {
                    Cell cell = data.GetMostCurrentCell(pos);
                    if (!cell.occupied) {
                        data.QueueUpdateColor(pos.x, pos.y, inputSourceData.color);
                    }
                }

                // Reset charge of attacking player
                inputSourceData.SetAtkCharge(0);

                // Play attack sfx
                inputSourceData.id.GetComponent<PlayerConnectionComponent>().RpcPlayClipAtCellOnClients(inputSourceData.weapon + 1, playerHeadPos);
            }
            
            // Calculate movement if input is movement
            if (moveDirection != Vector2.zero) {
                Cell currHeadCell = data.GetMostCurrentCell(playerHeadPos);
                Cell targetCell = data.GetMostCurrentCell(playerHeadPos + moveDirection);
                if (targetCell.type != 1 && !targetCell.occupied) {
                    // Make all the cell updates needed to move the head of the snake
                    data.QueueUpdateColor(targetCell.x, targetCell.y, inputSourceData.color);
                    data.QueueUpdateOccupied(targetCell.x, targetCell.y, true);
                    data.QueueUpdateIsHead(targetCell.x, targetCell.y, true);
                    data.QueueUpdateDistToTail(targetCell.x, targetCell.y, currHeadCell.life + 1);
                    data.QueueUpdatePainted(targetCell.x, targetCell.y, true);
                    data.QueueUpdatePlayer(targetCell.x, targetCell.y, inputSourceData.id);
                    data.QueueUpdateIsHead(currHeadCell.x, currHeadCell.y, false);

                    // Raise attack charge
                    inputSourceData.AtkChargeUp();
                    inputSourceData.SetLastMoveDirection(moveDirection);
                    
                    if (targetCell.type == 4 && targetCell.cache >= Cell.MAX_CACHE) {
                        inputSourceData.SetLength(inputSourceData.length + 3);
                        data.QueueUpdateCache(targetCell.x, targetCell.y, 0);
                    }

                    if (targetCell.type == 5 && targetCell.weaponTimer == 0) {
                        if (inputSourceData.weapon != targetCell.weapon) {
                            playerComponent.RpcSetAtkIndicatorWeapon(targetCell.weapon, AttackTable.getAtkLevel(targetCell.weapon, inputSourceData.length, inputSourceData.atkCharge), inputSourceData.lastMoveDirection);
                        }
                        inputSourceData.SetWeapon(targetCell.weapon);
                        data.QueueUpdateWeaponTimer(targetCell.x, targetCell.y, Cell.WEAPON_DROP_TIME);
                    }

                    // Update the PlayerData with the new head position
                    inputSourceData.SetX(targetCell.x);
                    inputSourceData.SetY(targetCell.y);

                    // Update variable for later use
                    playerHeadPos += moveDirection;
                } else {
                    inputSourceData.SetLength(System.Math.Max(inputSourceData.length - 1, 0));
                    inputSourceData.SetAtkCharge(0);
                    playerId.GetComponent<PlayerConnectionComponent>().RpcPlayOneShotOnClients(0);
                    playerId.GetComponent<PlayerConnectionComponent>().RpcUpdateAtkIndicator(AttackTable.getAtkLevel(inputSourceData.weapon, inputSourceData.length, inputSourceData.atkCharge), inputSourceData.lastMoveDirection);
                    playerId.GetComponent<PlayerConnectionComponent>().RpcShowWarning("Streak Lost");
                }

                // Update length of tail after movement
                updateSnakeCells(inputSourceData);
            }
        } else {
            inputSourceData.SetAtkCharge(0);
            playerId.GetComponent<PlayerConnectionComponent>().RpcPlayOneShotOnClients(0);
            playerId.GetComponent<PlayerConnectionComponent>().RpcUpdateAtkIndicator(AttackTable.getAtkLevel(inputSourceData.weapon, inputSourceData.length, inputSourceData.atkCharge), inputSourceData.lastMoveDirection);
            playerId.GetComponent<PlayerConnectionComponent>().RpcShowWarning("Streak Lost");
        }

        // VERY IMPORTANT
        // Up until this point, all updates to the grid are being stored in a temporary hashmap.
        // This takes those updates and applies them to the synclist so that any cell updates
        // are applied with only one actual network update per cell
        data.ApplyUpdates();

        inputSourceData.movedThisTurn = true;

        // Updates the data of the player that produced the input across network
        playerData.UpdatePlayer(inputSourceData);
        
        
        // Update's player's camera to focus on new head cell
        playerComponent.RpcUpdateCamera(new Vector2Int(inputSourceData.x, inputSourceData.y));
        playerComponent.RpcUpdateAtkIndicator(AttackTable.getAtkLevel(inputSourceData.weapon, inputSourceData.length, inputSourceData.atkCharge), inputSourceData.lastMoveDirection);
    }

    // Used to assign teams
    private int[] teamColorsTemp = {
        1,
        2,
        3
    };

    // Dummy variable for team assignment and spawn placement
    int dummy = 0;

    List<NetworkIdentity> joiningPlayers = new List<NetworkIdentity>();
    
    private bool loadTimerRunning = false;
    private float loadTimer = 15f;

    // Called by command from client to register when a player joins
    public void CreatePlayer(NetworkIdentity playerId) {
        if (gameLoaded) return;

        joiningPlayers.Add(playerId);

        if (loadTimerRunning) {
            playerId.GetComponent<PlayerConnectionComponent>().RpcStartTimer("Game starting in:", loadTimer);
        }

        if (!loadTimerRunning && joiningPlayers.Count > (debugAllowSinglePlayer ? 0 : 1)) {
            loadTimerRunning = true;
            foreach (NetworkIdentity id in joiningPlayers) {
                id.GetComponent<PlayerConnectionComponent>().RpcStartTimer("Game starting in:", loadTimer);
            }
        }

    }



    // Spawns player at a random spawn point, unless none are available
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
                player.SetLength(PlayerData.START_LENGTH);
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

    public void EndBeatUpdate() {
        List<PlayerData> updatedPlayers = new List<PlayerData>();
        List<NetworkIdentity> toSpawn = new List<NetworkIdentity>();
        foreach (PlayerData player in playerData) {
            PlayerData newPlayer = player;
            if (!newPlayer.spawned) {
                if (newPlayer.beatsToRespawn > 0) {
                    newPlayer.beatsToRespawn--;
                } else {
                    toSpawn.Add(newPlayer.id);
                }
            }
            else if (!newPlayer.movedThisTurn) {
                newPlayer.SetAtkCharge(0);
                PlayerConnectionComponent playerCon = newPlayer.id.GetComponent<PlayerConnectionComponent>();
                playerCon.RpcShowWarning("Streak Lost");
                playerCon.RpcUpdateAtkIndicator(AttackTable.getAtkLevel(newPlayer.weapon, newPlayer.length, newPlayer.atkCharge), newPlayer.lastMoveDirection);
                playerCon.RpcPlayOneShotOnClients(0);
            }
            newPlayer.movedThisTurn = false;
            updatedPlayers.Add(newPlayer);
        }
        foreach (PlayerData player in updatedPlayers) {
            playerData.UpdatePlayer(player);
        }
        foreach (NetworkIdentity id in toSpawn) {
            SpawnPlayer(id);
        }

        // Run cell updates
        foreach(Vector2Int cache in cacheLocations) {
            Cell cacheCell = data.GetMostCurrentCell(cache);
            if (!cacheCell.occupied) {
                data.QueueUpdateCache(cache.x, cache.y);
            }
        }
        foreach(Vector2Int weaponSpot in weaponPoints) {
            Cell weaponCell = data.GetMostCurrentCell(weaponSpot);
            if (!weaponCell.occupied) {
                data.QueueTickWeaponTimer(weaponSpot.x, weaponSpot.y);
            }
        }
        data.ApplyUpdates();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private int teamCount = 0;

    // Update is called once per frame
    void Update()
    {
        if (!isServer) {
            return;
        }

        if (!gameLoaded && loadTimerRunning) {
            loadTimer -= Time.deltaTime;
            if (loadTimer <= 0) {
                // Build grid from settings        
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
                        if (type == 5) weaponPoints.Add(new Vector2Int(j, i));
                        data.Add(newCell);
                    }
                }


                switch(joiningPlayers.Count) {
                    case 1:
                    case 2:
                    case 4:
                    case 5:
                        teamCount = 2;
                        break;
                    default:
                        teamCount = 3;
                        break;
                }

                foreach (NetworkIdentity id in joiningPlayers) {
                    PlayerData newPlayer = new PlayerData(id, 0, 0, teamColorsTemp[dummy++ % teamCount]);
                    playerData.Add(newPlayer);
                    SpawnPlayer(id);
                    id.GetComponent<PlayerConnectionComponent>().RpcSetPlayerColor(newPlayer.color);
                }

                MusicManager m = MusicManager.musicManager;
                m.RpcStartPlaying(m.GetRandomSong());

                gameLoaded = true;
            }
        }
    }

}
