using UnityEngine;
using TMPro;
using System;

public class SimManager : MonoBehaviour{
    [SerializeField] SimulationParameters simParams;
    [SerializeField] GameObject simBounds;

    float simBoundRadius;
    GizmoStruct gizmoStruct;
    [Header("Text Element")]
    [SerializeField] TMP_Text TMPtext;

    void Start() {
        init();
        BoidManager.OnBoidSpawn += init;
    }

    void init() {
        simBoundRadius = simParams.SimBoundRadius;
        gizmoStruct = simParams.GizmoStruct;
        Vector3 simScale = new Vector3(simBoundRadius,simBoundRadius,simBoundRadius);
        simBounds.gameObject.transform.localScale = simScale;
        initalizeText();
    }


    void initalizeText(){
        int numBoids = simParams.NumBoids;
        string text = "Simulation Stats\n";
        text += "Num Boids: " + numBoids + "\n";
        Debug.Log(text);
        TMPtext.text = text;
    }

    void OnDrawGizmos() {
        if(!gizmoStruct.showGrid) return;
        Gizmos.color = gizmoStruct.cellColor;
        float sizePerCell = simBoundRadius / 20f;
        for(int i = 0; i < 400; i++) {
            int rowNumber = i / 20;
            int colNumber = i % 20;
            float xPos = (-simBoundRadius / 2f) + (rowNumber * sizePerCell) + (sizePerCell / 2f);
            float zPos = (-simBoundRadius / 2f) + (colNumber * sizePerCell) + (sizePerCell / 2f);
            Vector3 finalPosition = new Vector3(xPos, 0, zPos);
            Gizmos.DrawWireCube(finalPosition, new Vector3(sizePerCell, 0, sizePerCell));
        }
        Gizmos.color = gizmoStruct.permiterColor;
        Gizmos.DrawWireCube(Vector3.zero,new Vector3(simBoundRadius, 0, simBoundRadius));
    }
}
