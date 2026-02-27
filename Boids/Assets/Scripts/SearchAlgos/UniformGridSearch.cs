using System;
using Unity.Mathematics;
using UnityEngine;

struct BoidCellPair {
    int cellID;
    int boidID;

    public BoidCellPair(int cellID, int boidID) {
        this.cellID = cellID;
        this.boidID = boidID;
    }

    public int CellID {get => cellID; set => cellID = value; }
    public int BoidID {get => boidID; set => boidID = value; }

}
public class UniformGridSearch : IBoidSearch {
    Vector3[] boidPositions;
    Boid[] currNeighbors;
    Boid[] boids;
    int numBoids;

    int cellSize;
    int numCellsPerRow;

    BoidCellPair[] cells;
    int[] cellStartOffsets;

    public UniformGridSearch(int numBoids, int cellSize, float simBoundRadius) {
        this.numBoids = numBoids;

        currNeighbors = new Boid[numBoids - 1];
        boidPositions = new Vector3[numBoids];
        boids = new Boid[numBoids];
        cellSize = this.cellSize;
        
        int numCellsRow = (int) Mathf.Ceil(simBoundRadius / cellSize); 
        numCellsPerRow = numCellsRow;
        cells = new BoidCellPair[numBoids];
        cellStartOffsets = new int[numCellsPerRow * numCellsPerRow];
    } 
    public void UpdatePosition(int idx, Vector3 newPos) {
        boidPositions[idx] = newPos;
    }
    public void AddBoid(int index, Vector3 position, Boid boid) {
        boidPositions[index] = position;
        boids[index] = boid;

        int cellID = getCellID(position);
        BoidCellPair cellPair = new BoidCellPair(cellID,index);
        // insert at boidsID
        cells[index] = cellPair;
    }

    public void RemoveBoid(int index) {
        boids[index] = null;
    }

    // returns cellID
    int getCellID(Vector3 position) {
        int xCell = (int) position.x / numCellsPerRow;
        int zCell = (int) position.z / numCellsPerRow;

        int cellID = (zCell * numCellsPerRow) + xCell;
        return cellID;
    }

    void updateCellStarts() {
        int cellStreakID = cells[0].CellID;
        int count = 1;
        for(int i = 1; i < numBoids; i++) {
            int currCell = cells[i].CellID;
            if(currCell == cellStreakID) {
                count++;
                continue;
            }
            cellStartOffsets[cellStreakID] = count;
            cellStreakID = currCell;
            count = 1;
        }
    }

    public (int, Boid[]) FindNeighbors(int index, float radius) {
        Span<Vector3> positions = boidPositions;
        // sort based on CellID
        Array.Sort(cells,(x,y) => x.CellID.CompareTo(y.CellID));
        updateCellStarts();
        Span<BoidCellPair> boidCellPairs = cells;

        return (0, null);
    }
}