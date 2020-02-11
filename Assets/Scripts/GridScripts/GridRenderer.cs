using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    private Grid grid;
    
    public int rows = 25;
    public int columns = 25;
    public int horizontalCenter = 0; //Where to center the grid in the xz plane
    public int verticalCenter = 0;
    public GameObject floorPrefab;
    public GameObject obstaclePrefab;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Grid(rows, columns);
        Cell current;
        GameObject render;
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < columns; j++) {
                current = grid.CellAt(i, j);
                if (current.GetObstacle()) { //Placing a gameobject at every cell on the grid and assigning it to the Cell object
                    render = Instantiate(obstaclePrefab, new Vector3(verticalCenter + i - rows/2, 0, horizontalCenter + j - columns/2), Quaternion.identity);
                } else {
                    render = Instantiate(floorPrefab, new Vector3(verticalCenter + i - rows/2, -1, horizontalCenter + j - columns/2), Quaternion.identity);
                }
                current.SetRender(render);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
