using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using QuadTree;
using UnityEngine.Experimental.AI;

public class SimManager : MonoBehaviour{
    [SerializeField] SimulationParameters simParams;
    [SerializeField] GameObject simBounds;

    float simBoundRadius;
    GizmoStruct gizmoStruct;
    [Header("Text Element")]
    [SerializeField] TMP_Text TMPtext;

    static int totalNeighbors;
    static int totalChecksRunningTotal;
    static int totalChecksText;
    static float averageNumChecks;
    static float density;
    static int numCounted;
    static int numBoids;
    static float totalMs;
    static float msAvg;
    // quadTree Nodes
    List<Node> nodes;

    void Start() {
        init();
        BoidManager.OnBoidSpawn += init;
    }

    void init() {
        simBoundRadius = simParams.SimBoundRadius;
        gizmoStruct = simParams.GizmoStruct;
        Vector3 simScale = new Vector3(simBoundRadius,simBoundRadius,simBoundRadius);
        simBounds.gameObject.transform.localScale = simScale;
        numBoids = simParams.NumBoids;
    }

    void Update() {
        initalizeText();
    }


    void initalizeText(){
        int numBoids = simParams.NumBoids;
        string text = "Simulation Stats\n";
        text += "Num Boids: " + numBoids + "\n";
        text += "Density: " + density + "\n";
        text += "Total Checks: " + totalChecksText + "\n";
        text += "Average Checks Per Boid: " + averageNumChecks + "\n";
        text += "MS taken to do neighbor search: " + msAvg + "\n"; 
        TMPtext.text = text;
    }


    public static void updateRunningTotals(int numNeighbors, int numChecks, float msTaken) {
        numCounted++;
        totalNeighbors += numNeighbors;
        totalChecksRunningTotal += numChecks;
        totalMs += msTaken;
        if(numCounted == numBoids) {
            msAvg = totalMs / numBoids;
            density = (float) totalNeighbors / numBoids;
            totalChecksText = totalChecksRunningTotal;
            averageNumChecks = (float) totalChecksRunningTotal / numBoids;

            totalChecksRunningTotal = 0;
            totalNeighbors = 0;
            totalMs = 0;
            numCounted = 0;
        }
    }

    void drawQuadTreeCells(int index, Vector2 boxDims, Vector2 offset) {
        if(index > numBoids || index < 0) {
            return;
        }

        Vector2 newDims = new Vector2(boxDims.x / 2, boxDims.y / 2);

        Vector2 tl = offset;
        Vector2 tr = new Vector2(offset.x + newDims.x, offset.y);
        Vector2 bl = new Vector2(offset.x, offset.y + newDims.y);
        Vector2 br = new Vector2(offset.x + newDims.x, offset.y + newDims.y);


        Vector2 halfNewDim = Vector2.Scale(newDims, new Vector2(.5f,.5f));

        Vector2 centerTL = tl + halfNewDim;
        Vector2 centerTR = tr + halfNewDim;
        Vector2 centerBL = bl + halfNewDim;
        Vector2 centerBR = br + halfNewDim;


        Vector3 centerTL3 = new Vector3(centerTL.x, 0, centerTL.y);
        Vector3 centerTR3 = new Vector3(centerTR.x, 0, centerTR.y);
        Vector3 centerBL3 = new Vector3(centerBL.x, 0, centerBL.y);
        Vector3 centerBR3 = new Vector3(centerBR.x, 0, centerBR.y);
        Vector3 newDims3 = new Vector3(newDims.x, 0, newDims.y);


        Gizmos.DrawWireCube(centerTL3, newDims3);
        Gizmos.DrawWireCube(centerTR3, newDims3);
        Gizmos.DrawWireCube(centerBL3, newDims3);
        Gizmos.DrawWireCube(centerBR3, newDims3);

        Node currNode = nodes[index];
        // Leaf Node
        int firstChild = currNode.FirstChild;
        if(firstChild != -1) {
           drawQuadTreeCells(firstChild,newDims,tl);
           drawQuadTreeCells(firstChild + 1,newDims,tr); 
           drawQuadTreeCells(firstChild + 2,newDims,bl); 
           drawQuadTreeCells(firstChild + 3,newDims,br); 
        }
    }


    void OnDrawGizmos() {
        if(!gizmoStruct.showGrid) return;
        Gizmos.color = gizmoStruct.cellColor;
        if(simParams.CurrSearchAlgo == SearchAlgos.UNIFORMGRID) {
            int cellSize = simParams.CellSize;
            int numCellsRow = (int) Mathf.Ceil(simBoundRadius / cellSize) + 1;
            float sizePerCell = simBoundRadius / numCellsRow;
            for(int i = 0; i < numCellsRow * numCellsRow; i++) {
                int rowNumber = i / numCellsRow;
                int colNumber = i % numCellsRow;
                float xPos = (-simBoundRadius / 2f) + (rowNumber * sizePerCell) + (sizePerCell / 2f);
                float zPos = (-simBoundRadius / 2f) + (colNumber * sizePerCell) + (sizePerCell / 2f);
                Vector3 finalPosition = new Vector3(xPos, 0, zPos);
                Gizmos.DrawWireCube(finalPosition, new Vector3(sizePerCell, 0, sizePerCell));
            }
            Gizmos.color = gizmoStruct.permiterColor;
            Gizmos.DrawWireCube(Vector3.zero,new Vector3(simBoundRadius, 0, simBoundRadius));
        }
        if(simParams.CurrSearchAlgo == SearchAlgos.QUADTREE) {
            nodes = simParams.Nodes;
            if(nodes == null || nodes.Count == 0) {
                return;
            }
            drawQuadTreeCells(0,new Vector2(simBoundRadius,simBoundRadius), new Vector2(-simBoundRadius / 2,-simBoundRadius / 2));
        }
    }
//  0 -> 1 (1,2,3,4)
//  4 -> 20 (20,21,22,23)
//  3 -> 5


// start at root
//  if Branch
//      Draw 4 quadrents
//      Recurse to 4 children

}
