using System;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "SimulationParameters", menuName = "Scriptable Objects/SimulationParameters")]
public class SimulationParameters : ScriptableObject{
    [Header("Number of Boids")]
    [SerializeField] int numBoids;
    [Header("Simulation Radius")]
    [SerializeField] float simBoundRadius; 
    [Header("Search Algorithm Parameters")]
    [SerializeField] SearchAlgos currSearchAlgo;

    [ShowIf("currSearchAlgo", SearchAlgos.UNIFORMGRID)]
    [SerializeField] int cellSize;

    [ShowIf("currSearchAlgo", SearchAlgos.QUADTREE)]
    [SerializeField] int leafCapacity;
    
    [SerializeField] UniformGridSearch search;

    [Header("Gizmo Options")]
    [SerializeField] GizmoStruct gizmoStruct;

    public int NumBoids { get => numBoids; set => numBoids = value; }
    public int LeafCapacity { get => leafCapacity; set => leafCapacity = value; }

    public float SimBoundRadius { get => simBoundRadius; set => simBoundRadius = value; }
    public int CellSize { get => cellSize; set => cellSize = value; }

    public SearchAlgos CurrSearchAlgo {get => currSearchAlgo; set => currSearchAlgo = value; }
    public UniformGridSearch Search { get => search; set => search = value; }
    public GizmoStruct GizmoStruct {get => gizmoStruct; set => gizmoStruct = value; }

}
