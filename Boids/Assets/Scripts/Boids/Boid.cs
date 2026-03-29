using System;
using Unity.VisualScripting;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
public class Boid : MonoBehaviour{
    float speed;
    float turnSpeed;
    float seperationRadius;
    float alignmentRadius;
    float cohesionRadius;

    float seperationForceWeight;
    float alignmentForceWeight;
    float cohesionForceWeight;
    float centerForceWeight;
    float simBoundRadius;
    float outsideTimer;

    IBoidSearch search;
    [SerializeField] Vector3 currHeading;
    int myIndex;
    GameObject boid; 
    MeshRenderer boidRenderer;
    Color originalColor;

    [Header("SOs")]
    BoidInfo boidInfo;
    SimulationParameters simParams;
    
    GizmoStruct gizmoStruct;

    private void grabValuesFromSO() {
        speed = boidInfo.Speed;
        turnSpeed = boidInfo.TurnSpeed;
        seperationRadius = boidInfo.SeparationRadius;
        alignmentRadius = boidInfo.AlignmentRadius;
        cohesionRadius = boidInfo.CohesionRadius;
        seperationForceWeight = boidInfo.SeparationForceWeight;
        alignmentForceWeight = boidInfo.AlignmentForceWeight;
        cohesionForceWeight = boidInfo.CohesionForceWeight;
        centerForceWeight = boidInfo.CenterForceWeight;

        simBoundRadius = simParams.SimBoundRadius;
        gizmoStruct = simParams.GizmoStruct;
    }

    public void init(int index, GameObject boid, IBoidSearch search, BoidInfo boidInfo, SimulationParameters simParams) {
        
        boid.SetActive(true);
        search.AddBoid(index,boid.transform.position,this);

        myIndex = index;
        this.boid = boid;
        this.search = search;
        this.boidInfo = boidInfo;
        this.simParams = simParams;

        boidRenderer = boid.GetComponentInChildren<MeshRenderer>();
        if (boidRenderer != null) {
            originalColor = boidRenderer.material.color;
        }

        Vector2 rand = UnityEngine.Random.onUnitSphere;
        currHeading = new Vector3(rand.x,0,rand.y).normalized;
        outsideTimer = 0;
        grabValuesFromSO();
    }

    public void disable() {
        if (boidRenderer != null) {
            boidRenderer.material.color = originalColor;
        }
        search.RemoveBoid(myIndex);
        boid.SetActive(false);
    }
    
    void Update(){
        if (boidRenderer != null) {
            if (simParams.IsVisualizingSearch && simParams.TargetBoidID == myIndex) {
                boidRenderer.material.color = simParams.HighlightBoidColor;
            } else {
                boidRenderer.material.color = originalColor;
            }
        }
        move();
    }

    void move() {
        grabValuesFromSO();
        Boid[] neighbors;
        int numNeighbors;
        int numChecks;
        float maxRadius = Math.Max(Math.Max(seperationRadius,alignmentRadius),cohesionRadius);
        Stopwatch stopwatch = Stopwatch.StartNew();
        (numNeighbors, numChecks, neighbors) = search.FindNeighbors(myIndex,maxRadius);
        stopwatch.Stop();
        float totalTime = (float)stopwatch.Elapsed.TotalMilliseconds;
        Vector3 desiredHeading = currHeading;
        if(numNeighbors > 0) {
            Vector3 force = calculateForces(numNeighbors,neighbors);
            if (force.sqrMagnitude > 0.001f) {
                desiredHeading = force;
            }
        }
        desiredHeading += getCenterForceScaled();
        currHeading = Vector3.Lerp(currHeading, desiredHeading.normalized, Time.deltaTime * turnSpeed).normalized;
        transform.Translate(currHeading * speed * Time.deltaTime);
        if (gizmoStruct.showBoidHeading) {
            Debug.DrawLine(boid.transform.position,boid.transform.position + (currHeading * 5),gizmoStruct.headingColor);
        }

        if (gizmoStruct.showNeighbors) {
            drawNeighbors(transform.position,neighbors, numNeighbors);
        }
        SimManager.updateRunningTotals(numNeighbors,numChecks,totalTime);
        
        search.UpdatePosition(myIndex,transform.position);
    }

    Vector3 getSeperationForce(Vector3 myPos, Vector3 neighborPos, float dist) {
        if(dist * dist <= seperationRadius * seperationRadius) {
            Vector3 seperation = (myPos - neighborPos).normalized;
            return seperation;
        }
        return Vector3.zero;
    }

    Vector3 getAlignmentForce(Vector3 myPos, Boid neighbor, float dist) {
        if(dist * dist <= alignmentRadius * alignmentRadius) {
            Vector3 neighborHeading = neighbor.CurrHeading;
            return neighborHeading;
        }
        return Vector3.zero;
    }

    Vector3 getCohesionForce(Vector3 neighborPos, float dist) {
        if(dist * dist <= cohesionRadius * cohesionRadius) {
            return neighborPos;
        }
        return Vector3.zero;
    }

    Vector3 getCenterForce(Vector3 myPos) {
        Vector3 centerForce = Vector3.zero;
        if(!isInSim(myPos)) {
            centerForce = -myPos;
            centerForce.y = 0;
            centerForce.Normalize();
            outsideTimer += Time.deltaTime;
        }
        else {
            outsideTimer = 0;
        }
        return centerForce;
    }

    Vector3 calculateForces(int numNeighbors, Boid[] neighbors) {
        Vector3 myPos = boid.transform.position;
        Vector3 totalSeperation = Vector3.zero;
        Vector3 totalAlignment = Vector3.zero;
        Vector3 totalCohesion = Vector3.zero;

        for(int i = 0; i < numNeighbors; i++) {
            Boid currNeighbor = neighbors[i];
            Vector3 neighborPos = currNeighbor.transform.position;
            float dist = (myPos - neighborPos).magnitude;
            totalSeperation += getSeperationForce(myPos,neighborPos,dist);
            totalAlignment += getAlignmentForce(myPos,currNeighbor,dist);
            totalCohesion += getCohesionForce(neighborPos,dist);
        }

        // normalize cohesion
        totalCohesion /= numNeighbors;
        totalCohesion.y = 1;
        totalCohesion = totalCohesion - myPos;


        Vector3 finalForce = Vector3.zero;
        finalForce += totalSeperation.normalized * seperationForceWeight;
        finalForce += totalAlignment.normalized * alignmentForceWeight;
        finalForce += totalCohesion.normalized * cohesionForceWeight;
        
        return finalForce.normalized;
    }


    Vector3 getCenterForceScaled() {
        Vector3 myPos = boid.transform.position;
        Vector3 centerForce = getCenterForce(myPos);
        Vector3 finalForce = centerForce * (outsideTimer * centerForceWeight);
        return finalForce;
    }

    public Vector3 CurrHeading { get => currHeading; set => currHeading = value;}

    private bool isInSim(Vector3 pos) {
        float posX = pos.x;
        float posZ = pos.z;

        float halfsimRadius = simBoundRadius / 2;

        bool xGood = posX < halfsimRadius && posX > -halfsimRadius;
        bool zGood = posZ < halfsimRadius && posZ > -halfsimRadius;

        return xGood && zGood;
    }


    private void drawNeighbors(Vector3 myPos, Boid[] neighbors, int numNeighbors) {
        for(int i = 0; i < numNeighbors; i++) {
            Boid neighbor = neighbors[i];
            Vector3 neighborPos = neighbor.gameObject.transform.position;
            Debug.DrawLine(myPos, neighborPos, gizmoStruct.neighborColor);
        }
    }

    public int getID() {
        return myIndex;
    }

}
