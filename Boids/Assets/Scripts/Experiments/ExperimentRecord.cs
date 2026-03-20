public class ExperimentRecord {
    public float AverageDensity { get; set; }
    public float AverageTotalMS { get; set; }
    public float AverageChecks { get; set; }
    public float AverageChecksPerBoid { get; set; }

    public ExperimentRecord(float averageDensity, float averageTotalMS, float averageChecks, float averageChecksPerBoid) {
        this.AverageDensity = averageDensity;
        this.AverageTotalMS = averageTotalMS;
        this.AverageChecks = averageChecks;
        this.AverageChecksPerBoid = averageChecksPerBoid;
    }
}