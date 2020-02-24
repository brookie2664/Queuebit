using UnityEngine;

public class Grid {


    private Cell[,] grid;
    //Perhaps we add a list of the players on the map.

    public Grid(int rows, int columns) {
        if (rows < 3 || columns < 3) {
            Debug.LogError("Dimensions for the grid are too small");
            return;
        }
        grid = new Cell[rows, columns];
        for (int i = 0; i < columns; i++) { //Obstacles at the perimeter
            grid[0, i] = new Cell(true);
            grid[rows-1, i] = new Cell(true);
        }
        for (int i = 1; i < rows; i++) {
            grid[i, 0] = new Cell(true);
            grid[i, columns-1] = new Cell(true);
        }
        for (int i = 1; i < rows-1; i++) { //Floor everywhere else
            for (int j = 1; j < columns-1; j++) {
                grid[i, j] = new Cell(false);
            }
        }
    }

    //Gets a cell
    public Cell CellAt(int row, int column) {
        if (row >= grid.GetLength(0) || row < 0 || column >= grid.GetLength(1) || column < 0) {
            Debug.LogError("index out of bounds of the grid.");
            return null;
        }
        return grid[row, column];
    }

    //Is called when a player attempts to move their character
    public void MoveQuery(int row, int col, int dr, int dc) {
        if (CellAt(row + dr, col + dc) != null && !CellAt(row + dr, col + dc).GetObstacle()) {
            //Move the player position to the new tile and update the entire chain.
        } else {
            //Play an animation of bonking into the wall.
        }
    }

}