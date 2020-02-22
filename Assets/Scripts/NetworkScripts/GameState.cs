using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour
{
    public class PlayerDataSyncList : SyncListStruct<PlayerData> {}
    public class GameGridSyncList : SyncListStruct<Cell> {
        private Dictionary<Vector2Int, Cell> queuedUpdates = new Dictionary<Vector2Int, Cell>();
        private int width;
        private int height;
        public int GetQueueCount() {
            return queuedUpdates.Count;
        }

        public void SetDimensions(int width, int height) {
            this.width = width;
            this.height = height;
        }

        public Cell GetMostCurrentCell(int x, int y) {
            if (x < 0 || x >= width || y < 0 || y >= height) {
                throw new System.Exception("Cannot modify cell at " + x + ", " + y + " becuase it does not exist");
            }
            return queuedUpdates.ContainsKey(new Vector2Int(x, y)) ? queuedUpdates[new Vector2Int(x, y)]: this.GetItem(y * width + x);
        }

        public void QueueCellUpdate(Cell cell) {
            queuedUpdates[new Vector2Int(cell.x, cell.y)] = cell;
        }

        public void QueueUpdateObstacle(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetObstacle(value);
            QueueCellUpdate(newCell);
        }

        public void QueueUpdateOccupied(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetOccupied(value);
            QueueCellUpdate(newCell);
        }

        public void QueueUpdatePlayer(int x, int y, int value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetPlayer(value);
            QueueCellUpdate(newCell);
        }

        public void QueueUpdatePainted(int x, int y, bool value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetPainted(value);
            QueueCellUpdate(newCell);
        }

        public void QueueUpdateColor(int x, int y, Color value) {
            Cell newCell = GetMostCurrentCell(x, y);
            newCell.SetColor(value);
            QueueCellUpdate(newCell);
        }

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

    public SyncListInt players = new SyncListInt();
    public PlayerDataSyncList playerData = new PlayerDataSyncList();

    [SyncVar]
    bool gridCreated = false;

    GameGridSyncList data = new GameGridSyncList();

    public bool IsGridCreated() {
        return gridCreated;
    }

    public GameGridSyncList GetData() {
        return data;
    }

    int dummy = 0;
    
    public void SendInput(KeyCode input, NetworkIdentity playerId) {
        if (!isServer) {
            return;
        }
        int playerDataId = (int)playerId.netId.Value;
        int playerIndex = players.IndexOf(playerDataId);
        PlayerData inputClientData = playerData.GetItem(playerIndex);

        Vector2Int plannedMotion = new Vector2Int();
        if (input == KeyCode.W) {
            plannedMotion += new Vector2Int(0, -1);
        } else if (input == KeyCode.A) {
            plannedMotion += new Vector2Int(-1, 0);
        } else if (input == KeyCode.S) {
            plannedMotion += new Vector2Int(0, 1);
        } else if (input == KeyCode.D) {
            plannedMotion += new Vector2Int(1, 0);
        }
        if (plannedMotion != Vector2Int.zero) {
            Cell currentPos = data.GetMostCurrentCell(inputClientData.x, inputClientData.y);
            Cell targetPos = data.GetMostCurrentCell(inputClientData.x + plannedMotion.x, inputClientData.y + plannedMotion.y);
            
            if (!(targetPos.obstacle || targetPos.occupied)) {
                data.QueueUpdateOccupied(currentPos.x, currentPos.y, false);
                data.QueueUpdateOccupied(targetPos.x, targetPos.y, true);
                data.QueueUpdatePlayer(targetPos.x, targetPos.y, playerDataId);
                data.QueueUpdateColor(targetPos.x, targetPos.y, inputClientData.color);
                data.QueueUpdatePainted(targetPos.x, targetPos.y, true);

                PlayerData newPlayerData = inputClientData;
                newPlayerData.SetX(targetPos.x);
                newPlayerData.SetY(targetPos.y);
                playerData.RemoveAt(playerIndex);
                playerData.Insert(playerIndex, newPlayerData);
            }
        }

        data.ApplyUpdates();
        PlayerData currentData = playerData.GetItem(playerIndex);
        playerId.GetComponentInParent<PlayerConnectionComponent>().RpcUpdateCamera(new Vector2Int(currentData.x, currentData.y));
    }

    private Color[] teamColorsTemp = {
        Color.red,
        Color.blue,
        Color.green
    };

    public void CreatePlayer(NetworkIdentity playerId) {
        players.Add((int) playerId.netId.Value);
        Vector2Int newPlayerPos = new Vector2Int(2, 2 + dummy);
        playerData.Add(new PlayerData(newPlayerPos.x, newPlayerPos.y, teamColorsTemp[dummy % teamColorsTemp.Length]));
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
