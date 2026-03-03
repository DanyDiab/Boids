using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
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
    int numCells;

    BoidCellPair[] cells;
    int[] cellStartOffsets;
    int[] cellSizes;
    float simBoundRadius;

    public UniformGridSearch(int numBoids, int cellSize, float simBoundRadius) {
        this.numBoids = numBoids;
        currNeighbors = new Boid[numBoids - 1];
        boidPositions = new Vector3[numBoids];
        boids = new Boid[numBoids];
        this.cellSize = cellSize;
        int numCellsRow = (int) Mathf.Ceil(simBoundRadius / cellSize) + 1;
        this.simBoundRadius = simBoundRadius + (cellSize / 2);
        numCellsPerRow = numCellsRow;
        numCells = numCellsPerRow * numCellsPerRow;
        cells = new BoidCellPair[numBoids];
        cellStartOffsets = new int[numCells];
        cellSizes = new int[numCells];
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

        int xCell = (int) (position.x + (simBoundRadius / 2))  / cellSize;
        int zCell = (int) (position.z + (simBoundRadius / 2)) / cellSize;
        int cellID = (zCell * numCellsPerRow) + xCell;
        return cellID;
    }

    void updateCells() {
        for(int i = 0; i < numBoids; i++) {
            // grab boid in this cell position
            BoidCellPair boidCellPair = cells[i];
            //grab references
            int boidID = boidCellPair.BoidID;
            Vector3 pos = boidPositions[boidID];
            // update cell based on position
            cells[i].CellID = getCellID(pos);
        }
    }

    void updateCellInfo() {
        int cellStreakID = cells[0].CellID;
        int count = 1;
        for(int i = 1; i < numBoids; i++) {
            int currCell = cells[i].CellID;
            if(currCell == cellStreakID) {
                count++;
                continue;
            }
            cellSizes[cellStreakID] = count;
            cellStartOffsets[cellStreakID] = i - count;
            cellStreakID = currCell;
            count = 1;
        }
        cellSizes[cellStreakID] = count;
        cellStartOffsets[cellStreakID] = numBoids - count;


    }
// returns {curr, top, bottom, left, right, top left, top right, bottom left, bottom right}
    int[] getNeighboringCellIDs(int cellID) {
        int t = cellID - numCellsPerRow;
        int b = cellID + numCellsPerRow;

        int tl = t - 1;
        int tr = t + 1;

        int bl = b - 1;
        int br = b + 1;

        int r = cellID + 1;
        int l = cellID - 1;
        int[] neighboringCellIDs = {cellID,t,b,l,r,tl,tr,bl,br};
        return neighboringCellIDs;
    }

    public (int, Boid[]) FindNeighbors(int index, float radius) {
        // update boid cellIDs
        updateCells();
        // sort based on cellID
        Array.Sort(cells,(x,y) => x.CellID.CompareTo(y.CellID));
        // sort array containing cell starts
        updateCellInfo();
        BoidCellPair currBoidPair = Array.Find(cells, p => p.BoidID == index);
        // get cell that its in
        int myCellID = currBoidPair.CellID;
        List<Boid> boidNeighbors = new List<Boid>(numBoids);
        Vector3 currPos = boidPositions[index];
        int numNeighbors = 0;
        // get neighboring cells

        int[] neighboringCellIDs = getNeighboringCellIDs(myCellID);
        for(int i = 0; i < neighboringCellIDs.Length; i++) {
            int currNeighborCellID = neighboringCellIDs[i];
            // skip if the neighborCellID is invalid
            if(currNeighborCellID < 0 || currNeighborCellID > numCells - 1) continue;
            int start = cellStartOffsets[currNeighborCellID];
            int cellCount = cellSizes[currNeighborCellID];
            for(int j = start; j < cellCount + start; j++) {
                BoidCellPair currBoidCellPair = cells[j];
                // skip myself
                int boidID = currBoidCellPair.BoidID;
                Boid currBoid = boids[boidID];
                if(boidID == index) continue;
                float radiusSq = radius * radius;
                Vector3 distance = currPos - boidPositions[boidID];
                if(distance.sqrMagnitude <= radiusSq) {
                    boidNeighbors.Add(currBoid);
                    numNeighbors++;
                }
            }
        }
        currNeighbors = boidNeighbors.ToArray();
        return (numNeighbors, currNeighbors);
    }
}