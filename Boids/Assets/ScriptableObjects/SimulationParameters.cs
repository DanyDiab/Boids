using System;
using UnityEngine;
using NaughtyAttributes;
using QuadTree;
using System.Collections.Generic;

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

    [ShowIf("currSearchAlgo", SearchAlgos.QUADTREE)]
    [SerializeField] List<Node> nodes;
    
    [SerializeField] UniformGridSearch search;

    [Header("Gizmo Options")]
    [SerializeField] GizmoStruct gizmoStruct;

    [Header("Search Visualization")]
    [SerializeField] bool isVisualizingSearch;
    [SerializeField] int targetBoidID;
    [Range(0.1f, 10f)]
    [SerializeField] float searchVisLerpSpeed = 2f;
    [SerializeField] Color highlightCellColor = Color.yellow;
    [SerializeField] Color highlightBoidColor = Color.green;
    [SerializeField] Color highlightLineColor = Color.green;
    [SerializeField] Color distanceCheckLineColor = Color.red;
    [SerializeField] float zoomOrthographicSize = 10f;

    public int NumBoids { get => numBoids; set => numBoids = value; }
    public int LeafCapacity { get => leafCapacity; set => leafCapacity = value; }

    public float SimBoundRadius { get => simBoundRadius; set => simBoundRadius = value; }
    public int CellSize { get => cellSize; set => cellSize = value; }
    public List<Node> Nodes { get => nodes; set => nodes = value; }

    public SearchAlgos CurrSearchAlgo {get => currSearchAlgo; set => currSearchAlgo = value; }
    public UniformGridSearch Search { get => search; set => search = value; }
    public GizmoStruct GizmoStruct {get => gizmoStruct; set => gizmoStruct = value; }

    public bool IsVisualizingSearch { get => isVisualizingSearch; set => isVisualizingSearch = value; }
    public int TargetBoidID { get => targetBoidID; set => targetBoidID = value; }
    public float SearchVisLerpSpeed { get => searchVisLerpSpeed; set => searchVisLerpSpeed = value; }
    public Color HighlightCellColor { get => highlightCellColor; set => highlightCellColor = value; }
    public Color HighlightBoidColor { get => highlightBoidColor; set => highlightBoidColor = value; }
    public Color HighlightLineColor { get => highlightLineColor; set => highlightLineColor = value; }
    public Color DistanceCheckLineColor { get => distanceCheckLineColor; set => distanceCheckLineColor = value; }
    public float ZoomOrthographicSize { get => zoomOrthographicSize; set => zoomOrthographicSize = value; }
}
