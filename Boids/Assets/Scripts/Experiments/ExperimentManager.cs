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
    [SerializeField] int[] entityCounts;
    [SerializeField] float[] cellSizesScalars;
    [SerializeField] int[] leafCapacities;

    [SerializeField] float mapSize = 500;
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
    ExperimentSettings experimentSettings;
    ExperimentState currState;

    public delegate void ExperimentStart();
    public static event ExperimentStart OnExperimentStart;
    string fileDir;

    int recordCount;
    void Start(){
        experimentRecords = new List<ExperimentRecord>();
        fileDir = Application.dataPath + "/Scripts/Results";
        filePath = $"{fileDir}/ExperimentResults.csv";
        Directory.CreateDirectory(fileDir);
        recordCount = 0;
        currState = ExperimentState.IDLE;
        totalExperimentTime = warmupSeconds + recordingSeconds;
        interactionRadius = Mathf.Max(boidInfo.CohesionRadius,boidInfo.AlignmentRadius, boidInfo.SeparationRadius);
        int seed = Random.Range(int.MinValue,int.MaxValue);
        Random.InitState(seed);
        experimentSettings = new ExperimentSettings(simParams.NumBoids,seed,boidInfo);
        SimManager.OnSimStatsReady += recordData;

    }

    void Update() {
        if(currState == ExperimentState.RECORD) {
            return;
        }
        if(currState == ExperimentState.IDLE && startExperiments) {
            startExperiments = false;
            currState = ExperimentState.WARMUP;
            // clear file
            File.WriteAllText(filePath, string.Empty);
            simParams.SimBoundRadius = mapSize;
            StartCoroutine(runExperiments());
        }
        if(currState != ExperimentState.IDLE && endExperiments) {
            endExperiments= false;
            currState = ExperimentState.IDLE;
            StopAllCoroutines();
        }
        
    }


    void initExperiment(SearchAlgos algoToUse, int numBoids, int leafCapacityOrCellSize) {
        simParams.CurrSearchAlgo = algoToUse;
        simParams.NumBoids = numBoids;
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


    void addNewRecord(SearchAlgos currSearchAlgo, int numBoids, int leafCapacityOrCellSize) {
        averageDensity /= recordCount;
        averageTotalMS /=  recordCount;
        averageChecks /= recordCount;
        averageChecksPerBoid /= recordCount;

        ExperimentRecord experimentRecord = new ExperimentRecord(averageDensity,averageTotalMS,averageChecks,averageChecksPerBoid, currSearchAlgo, numBoids, leafCapacityOrCellSize);
        experimentRecords.Add(experimentRecord);
        SaveExperiment(experimentRecord,filePath);
        recordCount = 0;
        averageDensity = 0;
        averageTotalMS = 0;
        averageChecks = 0;
        averageChecksPerBoid = 0;
    }

    void SaveSettings(ExperimentSettings settings, string filePath) {
        string jsonData = JsonUtility.ToJson(settings,true);
        File.WriteAllText(filePath,jsonData);
    }


    void SaveExperiment(ExperimentRecord record, string filePath) {
        bool isFirstWrite = !File.Exists(filePath) || new FileInfo(filePath).Length == 0;
        
        using (StreamWriter writer = new StreamWriter(filePath, true)) {
            
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                HasHeaderRecord = isFirstWrite
            };

            using (CsvWriter csv = new CsvWriter(writer, config)) {
                if (isFirstWrite) {
                    csv.WriteHeader<ExperimentRecord>();
                    csv.NextRecord();
                    SaveSettings(experimentSettings,$"{fileDir}/ExperimentSettings.JSON");
                }

                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }
    }
    

    IEnumerator runExperiments() {
        foreach(int numBoids in entityCounts) {
            // Uniform grid
            foreach(float cellSizesScalar in cellSizesScalars) {
                int cellSize = (int) (interactionRadius * cellSizesScalar);
                initExperiment(SearchAlgos.UNIFORMGRID,numBoids,cellSize);
                currState = ExperimentState.WARMUP;
                yield return new WaitForSeconds(warmupSeconds);
                currState = ExperimentState.RECORD;
                yield return new WaitForSeconds(recordingSeconds);
                addNewRecord(SearchAlgos.UNIFORMGRID,numBoids,cellSize);
                

            }
            // QuadTree
            foreach(int leafCapacity in leafCapacities) {
                initExperiment(SearchAlgos.QUADTREE,numBoids,leafCapacity);
                currState = ExperimentState.WARMUP;
                yield return new WaitForSeconds(warmupSeconds);
                currState = ExperimentState.RECORD;
                yield return new WaitForSeconds(recordingSeconds);
                addNewRecord(SearchAlgos.QUADTREE,numBoids,leafCapacity);
            }

            initExperiment(SearchAlgos.BF,numBoids,0);
            currState = ExperimentState.WARMUP;
            yield return new WaitForSeconds(warmupSeconds);
            currState = ExperimentState.RECORD;
            yield return new WaitForSeconds(recordingSeconds);
            addNewRecord(SearchAlgos.BF,numBoids,0);


        }

    }
}
