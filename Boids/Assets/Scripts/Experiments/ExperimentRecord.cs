public class ExperimentRecord {
    public float averageDensity { get; set; }
    public float averageTotalMS { get; set; }
    public float averageChecks { get; set; }
    public float averageChecksPerBoid { get; set; }

    public int mapSize {get; set;}
    public int leafCapacityOrCellSize {get; set;}

    public SearchAlgos searchAlgo {get; set; }

    public ExperimentRecord(float averageDensity, float averageTotalMS, float averageChecks, float averageChecksPerBoid, SearchAlgos searchAlgo, int mapSize, int leafCapacityOrCellSize) {
        this.averageDensity = averageDensity;
        this.averageTotalMS = averageTotalMS;
        this.averageChecks = averageChecks;
        this.averageChecksPerBoid = averageChecksPerBoid;
        this.searchAlgo = searchAlgo;
        this.mapSize = mapSize;
        this.leafCapacityOrCellSize = leafCapacityOrCellSize;
    }
}