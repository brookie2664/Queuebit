using UnityEngine;
using UnityEngine.Networking;

public class Grid {


    private Cell[,] gride;
    private SyncListStruct<Cell> grid;
    public int numRows;
    public int numCols;
    //Perhaps we add a list of the players on the map.

    public Grid(int rows, int columns) {
        if (rows < 3 || columns < 3) {
            Debug.LogError("Dimensions for the grid are too small");
            return;
        }
        numCols = columns;
        numRows = rows;
        grid = new SyncListStruct<Cell>();
        for (int i = 0; i < rows; i++) { //Floor everywhere else
            for (int j = 0; j < columns; j++) {
                if (i == 0 || j == 0 || i == rows-1 || j == columns-1) {
                    grid.Add(new Cell(true));
                } else {
                    grid.Add(new  Cell(false));
                }
            }
        }
    }

    //Gets a cell
    public Cell CellAt(int row, int column) {
        if (row >= numRows || row < 0 || column >= numCols || column < 0) {
            Debug.LogError("index out of bounds of the grid.");
        }
        return grid[row*numCols+column];
    }

    /*
    //Is called when a player attempts to move their character
    public void MoveQuery(int row, int col, int dr, int dc) {
        if (CellAt(row + dr, col + dc) != null && !CellAt(row + dr, col + dc).GetObstacle()) {
            //Move the player position to the new tile and update the entire chain.
        } else {
            //Play an animation of bonking into the wall.
        }
    }
    */

}