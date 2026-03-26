[System.Serializable]
public class ExperimentSettings {

    public int MapSize;
    public int RandomSeed;
    public float speed;
    public float turnSpeed;
    public float separationRadius;
    public float alignmentRadius;
    public float cohesionRadius;
    public float separationForceWeight;
    public float alignmentForceWeight;
    public float cohesionForceWeight;
    public float centerForceWeight;

    public ExperimentSettings(int mapSize, int randomSeed, BoidInfo boidInfo)
    {
        MapSize = mapSize;
        RandomSeed = randomSeed;
        this.speed = boidInfo.Speed;
        this.separationRadius = boidInfo.SeparationRadius;
        this.alignmentRadius = boidInfo.AlignmentRadius;
        this.cohesionRadius = boidInfo.CohesionRadius;
        this.separationForceWeight = boidInfo.SeparationForceWeight;
        this.alignmentForceWeight = boidInfo.AlignmentForceWeight;
        this.cohesionForceWeight = boidInfo.CohesionForceWeight;
        this.centerForceWeight = boidInfo.CenterForceWeight;
        this.turnSpeed = boidInfo.TurnSpeed;
    }
}