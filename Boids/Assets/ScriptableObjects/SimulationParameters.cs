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
    [SerializeField] List<int> highlightedNodeIndices = new List<int>();
    [SerializeField] List<int> highlightedBoidIndices = new List<int>();
    [Range(0.1f, 10f)]
    [SerializeField] float searchVisLerpSpeed = 2f;
    [SerializeField] Color highlightCellColor = Color.yellow;
    [SerializeField] Color highlightBoidColor = Color.green;
    [SerializeField] Color highlightLineColor = Color.green;
    [SerializeField] Color distanceCheckLineColor = Color.red;
    [SerializeField] Color separationRadiusColor = Color.red;
    [SerializeField] Color alignmentRadiusColor = Color.blue;
    [SerializeField] Color cohesionRadiusColor = Color.green;
    [SerializeField] float quadTreeZoomOrthographicSize = 40f;
    [SerializeField] float uniformGridZoomOrthographicSize = 10f;

    public int NumBoids { get => numBoids; set => numBoids = value; }
    public int LeafCapacity { get => leafCapacity; set => leafCapacity = value; }

    public float SimBoundRadius { get => simBoundRadius; set => simBoundRadius = value; }
    public int CellSize { get => cellSize; set => cellSize = value; }
    public List<Node> Nodes { get => nodes; set => nodes = value; }
    public List<int> HighlightedNodeIndices { get => highlightedNodeIndices; set => highlightedNodeIndices = value; }
    public List<int> HighlightedBoidIndices { get => highlightedBoidIndices; set => highlightedBoidIndices = value; }


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
    public Color SeparationRadiusColor { get => separationRadiusColor; set => separationRadiusColor = value; }
    public Color AlignmentRadiusColor { get => alignmentRadiusColor; set => alignmentRadiusColor = value; }
    public Color CohesionRadiusColor { get => cohesionRadiusColor; set => cohesionRadiusColor = value; }
    public float QuadTreeZoomOrthographicSize { get => quadTreeZoomOrthographicSize; set => quadTreeZoomOrthographicSize = value; }
    public float UniformGridZoomOrthographicSize { get => uniformGridZoomOrthographicSize; set => uniformGridZoomOrthographicSize = value; }
    public float CurrentZoomSize => currSearchAlgo == SearchAlgos.QUADTREE ? quadTreeZoomOrthographicSize : uniformGridZoomOrthographicSize;
}
