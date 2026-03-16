using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;

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
    static float totalMSText;

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
        text += "AVG MS: " + msAvg + "\n"; 
        text += "Total MS: " + totalMSText + "\n";
        TMPtext.text = text;
    }


    public static void updateRunningTotals(int numNeighbors, int numChecks, float msTaken) {
        numCounted++;
        totalNeighbors += numNeighbors;
        totalChecksRunningTotal += numChecks;
        totalMs += msTaken;
        if(numCounted == numBoids) {
            totalMSText = totalMs;
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

    void OnDrawGizmos() {
        if(!gizmoStruct.showGrid || simParams.CurrSearchAlgo != SearchAlgos.UNIFORMGRID) return;
        Gizmos.color = gizmoStruct.cellColor;
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



}
