using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

enum ExperimentState {
    WAITING,
    RUNNING
}

public class ExperimentManager : MonoBehaviour
{
    [Header("Experiment Parameters")]
    [SerializeField] float secondsPerExperiment;
    [SerializeField] int[] mapSizes;
    [SerializeField] float[] cellSizesScalars;
    [SerializeField] float[] leafCapacitiesScalars;
    [Header("References")]
    [SerializeField] SimulationParameters simParams;
    [SerializeField] BoidInfo boidInfo;
    [Header("Experiments Controls")]
    [Space(20)]
    [SerializeField] bool startExperiments;
    [Space(20)]
    [SerializeField] bool endExperiments;


    float interactionRadius;
    
    ExperimentState currState;

    public delegate void ExperimentStart();
    public static event ExperimentStart OnExperimentStart;
    void Start(){
        currState = ExperimentState.WAITING;


        interactionRadius = Mathf.Max(boidInfo.CohesionRadius,boidInfo.AlignmentRadius, boidInfo.SeparationRadius);
    }

    void Update() {
        if(currState == ExperimentState.WAITING && startExperiments) {
            startExperiments = false;
            currState = ExperimentState.RUNNING;
            StartCoroutine(runExperiments());
        }
        if(currState == ExperimentState.RUNNING && endExperiments) {
            endExperiments= false;
            currState = ExperimentState.WAITING;
            StopAllCoroutines();
        }
        
    }


    void initExperiment(SearchAlgos algoToUse, int mapSize, int leafCapacityOrCellSize) {
        simParams.CurrSearchAlgo = algoToUse;
        simParams.SimBoundRadius = mapSize;
        switch (algoToUse) {
            case (SearchAlgos.UNIFORMGRID): {
                simParams.CellSize = leafCapacityOrCellSize;
                break;
            }
            case (SearchAlgos.QUADTREE): {
                simParams.LeafCapacity = leafCapacityOrCellSize;
                break;       
            }
            default: {
                break;       
            }
        }
        OnExperimentStart?.Invoke();
    }
    

    IEnumerator runExperiments() {
        foreach(int size in mapSizes) {
            foreach(float cellSizesScalar in cellSizesScalars) {
                int cellSize = (int) (interactionRadius * cellSizesScalar);
                initExperiment(SearchAlgos.UNIFORMGRID,size,cellSize);
                yield return new WaitForSeconds(secondsPerExperiment);
            }

            foreach(float leafCapacitiesScalar in leafCapacitiesScalars) {
                int leafCapacity = (int) (interactionRadius * leafCapacitiesScalar);
                initExperiment(SearchAlgos.QUADTREE,size,leafCapacity);
                yield return new WaitForSeconds(secondsPerExperiment);
            }
        }
    }
}
