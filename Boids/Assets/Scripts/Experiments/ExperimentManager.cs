using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using CsvHelper;
using System.Collections.Generic;
using UnityEditor;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;

enum ExperimentState {
    IDLE,
    WARMUP,
    RECORD,
    COMPLETE
}


public class ExperimentManager : MonoBehaviour
{
    [Header("Experiment Parameters")]
    [SerializeField] float recordingSeconds;
    [SerializeField] float warmupSeconds;
    float totalExperimentTime;
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


    float averageDensity;
    float averageTotalMS;
    float averageChecks;
    float averageChecksPerBoid;
    
    string filePath;

    List<ExperimentRecord> experimentRecords;
    float interactionRadius;
    
    ExperimentState currState;

    public delegate void ExperimentStart();
    public static event ExperimentStart OnExperimentStart;

    int recordCount;
    void Start(){
        experimentRecords = new List<ExperimentRecord>();
        filePath = "./ExperimentResults.csv";
        recordCount = 0;
        currState = ExperimentState.IDLE;
        totalExperimentTime = warmupSeconds + warmupSeconds;
        interactionRadius = Mathf.Max(boidInfo.CohesionRadius,boidInfo.AlignmentRadius, boidInfo.SeparationRadius);
        SimManager.OnSimStatsReady += recordData;
    }

    void Update() {
        if(currState == ExperimentState.IDLE && startExperiments) {
            startExperiments = false;
            currState = ExperimentState.WARMUP;
            StartCoroutine(runExperiments());
        }
        if(currState != ExperimentState.IDLE && endExperiments) {
            endExperiments= false;
            currState = ExperimentState.IDLE;
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


    void recordData(float density, float totalMS, float totalChecks, float avgChecks) {
        if(currState != ExperimentState.RECORD) return;
        recordCount++;
        averageDensity += density;
        averageTotalMS += totalMS;
        averageChecks += totalChecks;
        averageChecksPerBoid += avgChecks;
    }


    void addNewRecord() {
        averageDensity /= recordCount;
        averageTotalMS /=  recordCount;
        averageChecks /= recordCount;
        averageChecksPerBoid /= recordCount;

        ExperimentRecord experimentRecord = new ExperimentRecord(averageDensity,averageTotalMS,averageChecks,averageChecksPerBoid);
        experimentRecords.Add(experimentRecord);
        SaveExperiment(experimentRecord,filePath);
    }


public void SaveExperiment(ExperimentRecord record, string filePath) {
    bool fileExists = File.Exists(filePath);

    using (StreamWriter writer = new StreamWriter(filePath, true)) {
        
        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = !fileExists 
        };

        using (CsvWriter csv = new CsvWriter(writer, config)) {
            if (!fileExists) {
                csv.WriteHeader<ExperimentRecord>();
                csv.NextRecord();
            }

            csv.WriteRecord(record);
            csv.NextRecord();
        }
    }
}
    

    IEnumerator runExperiments() {
        foreach(int size in mapSizes) {
            foreach(float cellSizesScalar in cellSizesScalars) {
                int cellSize = (int) (interactionRadius * cellSizesScalar);
                initExperiment(SearchAlgos.UNIFORMGRID,size,cellSize);
                currState = ExperimentState.WARMUP;
                yield return new WaitForSeconds(warmupSeconds);
                currState = ExperimentState.RECORD;
                yield return new WaitForSeconds(recordingSeconds);
                addNewRecord();
                

            }

            foreach(float leafCapacitiesScalar in leafCapacitiesScalars) {
                int leafCapacity = (int) (interactionRadius * leafCapacitiesScalar);
                initExperiment(SearchAlgos.QUADTREE,size,leafCapacity);
                currState = ExperimentState.WARMUP;
                yield return new WaitForSeconds(warmupSeconds);
                currState = ExperimentState.RECORD;
                yield return new WaitForSeconds(recordingSeconds);
                addNewRecord();
            }

        }

    }
}
