[System.Serializable]
public class ExperimentSettings {

    public int NumBoids;
    public int RandomSeed;
    public float speed;
    public float separationRadius;
    public float alignmentRadius;
    public float cohesionRadius;
    public float separationForceWeight;
    public float alignmentForceWeight;
    public float cohesionForceWeight;
    public float centerForceWeight;
    public ExperimentSettings(int numBoids, int randomSeed, BoidInfo boidInfo)
    {
        NumBoids = numBoids;
        RandomSeed = randomSeed;
        this.speed = boidInfo.Speed;
        this.separationRadius = boidInfo.SeparationRadius;
        this.alignmentRadius = boidInfo.AlignmentRadius;
        this.cohesionRadius = boidInfo.CohesionRadius;
        this.separationForceWeight = boidInfo.SeparationForceWeight;
        this.alignmentForceWeight = boidInfo.AlignmentForceWeight;
        this.cohesionForceWeight = boidInfo.CohesionForceWeight;
        this.centerForceWeight = boidInfo.CenterForceWeight;
    }
}