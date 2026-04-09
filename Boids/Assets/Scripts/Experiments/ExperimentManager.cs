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
    [SerializeField] int[] leafCapacities;
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
    [SerializeField] int seed;
    [SerializeField] bool seedSet;

    int recordCount;
    void Start(){
        experimentRecords = new List<ExperimentRecord>();
        fileDir = Application.dataPath + "/Scripts/Results";
        filePath = $"{fileDir}/NewExperimentResults.csv";
        Directory.CreateDirectory(fileDir);
        recordCount = 0;
        currState = ExperimentState.IDLE;
        totalExperimentTime = warmupSeconds + warmupSeconds;
        interactionRadius = Mathf.Max(boidInfo.CohesionRadius,boidInfo.AlignmentRadius, boidInfo.SeparationRadius);
        seed = getSeed();
        Random.InitState(seed);
        experimentSettings = new ExperimentSettings(simParams.NumBoids,seed,boidInfo);
        SimManager.OnSimStatsReady += recordData;

    }

    int getSeed() {
        if(seedSet) return seed;
        return Random.Range(int.MinValue,int.MaxValue);
        
    }

    void Update() {
        if(currState == ExperimentState.IDLE && startExperiments) {
            startExperiments = false;
            currState = ExperimentState.WARMUP;
            // clear file
            File.WriteAllText(filePath, string.Empty);
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


    void addNewRecord(SearchAlgos currSearchAlgo, int mapSize, int leafCapacityOrCellSize) {
        averageDensity /= recordCount;
        averageTotalMS /=  recordCount;
        averageChecks /= recordCount;
        averageChecksPerBoid /= recordCount;

        ExperimentRecord experimentRecord = new ExperimentRecord(averageDensity,averageTotalMS,averageChecks,averageChecksPerBoid, currSearchAlgo, mapSize, leafCapacityOrCellSize);
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
        foreach(int size in mapSizes) {
            // Uniform grid
            foreach(float cellSizesScalar in cellSizesScalars) {
                int cellSize = (int) (interactionRadius * cellSizesScalar);
                initExperiment(SearchAlgos.UNIFORMGRID,size,cellSize);
                currState = ExperimentState.WARMUP;
                yield return new WaitForSeconds(warmupSeconds);
                currState = ExperimentState.RECORD;
                yield return new WaitForSeconds(recordingSeconds);
                addNewRecord(SearchAlgos.UNIFORMGRID,size,cellSize);
                

            }
            // QuadTree
            foreach(int leafCapacity in leafCapacities) {
                initExperiment(SearchAlgos.QUADTREE,size,leafCapacity);
                currState = ExperimentState.WARMUP;
                yield return new WaitForSeconds(warmupSeconds);
                currState = ExperimentState.RECORD;
                yield return new WaitForSeconds(recordingSeconds);
                addNewRecord(SearchAlgos.QUADTREE,size,leafCapacity);
            }

        }

    }
}
