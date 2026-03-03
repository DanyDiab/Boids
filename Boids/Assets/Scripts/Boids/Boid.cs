using System;
using Unity.VisualScripting;
using UnityEngine;

public class Boid : MonoBehaviour{
    float speed;
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
    Vector3 currHeading;
    int myIndex;
    GameObject boid; 

    [Header("SOs")]
    BoidInfo boidInfo;
    SimulationParameters simParams;
    
    GizmoStruct gizmoStruct;


    private void grabValuesFromSO() {
        speed = boidInfo.Speed;
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
        Vector2 rand = UnityEngine.Random.insideUnitCircle;
        CurrHeading = new Vector3(rand.x,0,rand.y);
        currHeading = Vector3.back;
        outsideTimer = 0;
        grabValuesFromSO();
    }

    public void disable() {
        boid.SetActive(false);
    }
    
    void Update(){
        move();
    }

    void move() {
        Boid[] neighbors;
        int numNeighbors;
        float maxRadius = Math.Max(Math.Max(seperationRadius,alignmentRadius),cohesionRadius);
        (numNeighbors, neighbors) = search.FindNeighbors(myIndex,maxRadius);

        if(numNeighbors > 0) {
            currHeading = calculateForces(numNeighbors,neighbors);
        }
        currHeading += getCenterForceScaled();

        transform.Translate(currHeading * speed * Time.deltaTime);
        if (gizmoStruct.showBoidHeading) {
            Debug.DrawLine(boid.transform.position,boid.transform.position + (currHeading * 5),gizmoStruct.headingColor);
        }

        if (gizmoStruct.showNeighbors) {
            drawNeighbors(transform.position,neighbors);
        }
        
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


    private void drawNeighbors(Vector3 myPos, Boid[] neighbors) {
        foreach(Boid neighbor in neighbors) {
            Vector3 neighborPos = neighbor.gameObject.transform.position;
            Debug.DrawLine(myPos, neighborPos, gizmoStruct.neighborColor);
        }
    }

    public int getID() {
        return myIndex;
    }

}
