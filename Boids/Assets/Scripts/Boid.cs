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
    float simBoundRadius;

    IBoidSearch search;
    Vector3 currHeading;
    BoidInfo boidInfo;

    int myIndex;
    GameObject boid; 


    public void init(int index, GameObject boid, IBoidSearch search, BoidInfo boidInfo) {
        boid.SetActive(true);
        search.AddBoid(index,boid.transform.position,this);
        myIndex = index;
        this.boid = boid;
        this.search = search;
        Vector2 rand = UnityEngine.Random.insideUnitCircle;
        CurrHeading = new Vector3(rand.x,0,rand.y);
        currHeading = Vector3.back;
        this.boidInfo = boidInfo;
        speed = boidInfo.Speed;
        seperationRadius = boidInfo.SeparationRadius;
        alignmentRadius = boidInfo.AlignmentRadius;
        cohesionRadius = boidInfo.CohesionRadius;
        seperationForceWeight = boidInfo.SeparationForceWeight;
        alignmentForceWeight = boidInfo.AlignmentForceWeight;
        cohesionForceWeight = boidInfo.CohesionForceWeight;
        simBoundRadius = boidInfo.SimBoundRadius;
    }

    public void disable() {
        boid.SetActive(false);
    }
    void Start(){
         
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
        // Quaternion targetRotation = Quaternion.Euler(0,Mathf.Atan2(finalForce.x, finalForce.z), 0);
        // transform.rotation = targetRotation;
        transform.Translate(currHeading * speed * Time.deltaTime);
        Debug.DrawLine(boid.transform.position,boid.transform.position + (currHeading * 5),Color.red);
        search.UpdatePosition(myIndex,transform.position);
    }

    Vector3 calculateForces(int numNeighbors, Boid[] neighbors) {
        Vector3 myPos = boid.transform.position;
        Vector3 totalSeperation = Vector3.zero;
        Vector3 totalAlignment = Vector3.zero;
        Vector3 totalCohesion = Vector3.zero;
        Vector3 centerForce = Vector3.zero;
        for(int i = 0; i < numNeighbors; i++) {
            Boid currNeighbor = neighbors[i];
            Vector3 neighborPos = currNeighbor.transform.position;
            float dist = (myPos - neighborPos).magnitude;
            
            if(dist * dist <= seperationRadius * seperationRadius) {
               Vector3 seperation = (myPos - neighborPos).normalized;
               totalSeperation += seperation;
               totalSeperation.Normalize();
            }
            
            if(dist * dist <= alignmentRadius * alignmentRadius) {
                Vector3 neighborHeading = currNeighbor.CurrHeading;
                totalAlignment += neighborHeading;  
                totalAlignment.Normalize();
            }

            if(dist * dist <= cohesionRadius * cohesionRadius) {
               totalCohesion += neighborPos;
            }
        }

        totalCohesion /= numNeighbors;
        totalCohesion.y = 1;
        totalCohesion = (totalCohesion - myPos).normalized;

        Vector3 finalForce = Vector3.zero;
        float distToCenter = myPos.magnitude;

        if(!isInSim(myPos)) {
            float edgeWeight = distToCenter / simBoundRadius;
            centerForce = -myPos;
            centerForce.y = 0;
            finalForce += centerForce * (edgeWeight * .1f);
        }

        finalForce += totalSeperation.normalized * seperationForceWeight;
        finalForce += totalAlignment.normalized * alignmentForceWeight;
        finalForce += totalCohesion.normalized * cohesionForceWeight;
        
        return finalForce.normalized;
    }

    public Vector3 CurrHeading { get => currHeading; set => currHeading = value;}

    private bool isInSim(Vector3 pos) {
        float posX = pos.x;
        float posZ = pos.z;

        float halfsimRadius = boidInfo.SimBoundRadius;

        bool xGood = posX < halfsimRadius && posX > -halfsimRadius;
        bool zGood = posZ < halfsimRadius && posZ > -halfsimRadius;

        return xGood && zGood;
    }

}
