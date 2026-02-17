using System;
using UnityEngine;

public class Boid : MonoBehaviour{
    [SerializeField] float speed;
    [SerializeField] float seperationRadius;
    [SerializeField] float alignmentRadius;
    [SerializeField] float cohesionRadius;

    [SerializeField] float seperationForceWeight;
    [SerializeField] float alignmentForceWeight;
    [SerializeField] float cohesionForceWeight;




    IBoidSearch search;
    Vector3 currHeading;

    int myIndex;
    GameObject boid; 


    public void init(int index, GameObject boid, IBoidSearch search) {
        speed = 10;
        boid.SetActive(true);
        myIndex = index;
        this.boid = boid;
        this.search = search;
        search.AddBoid(index,boid.transform.position,this);
        currHeading = Vector3.back;
        seperationRadius = 5;
        alignmentRadius = 6;
        cohesionRadius = 20;
        Vector2 rand = UnityEngine.Random.insideUnitCircle;
        CurrHeading = new Vector3(rand.x,0,rand.y);

        seperationForceWeight = 2f;
        alignmentForceWeight = 2f;
        cohesionForceWeight = 1f;
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
        finalForce += totalSeperation.normalized * seperationForceWeight;
        finalForce += totalAlignment.normalized * alignmentForceWeight;
        finalForce += totalCohesion.normalized * cohesionForceWeight;
        
        return finalForce.normalized;
    }

    public Vector3 CurrHeading { get => currHeading; set => currHeading = value;}



    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, seperationRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, alignmentRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cohesionRadius);

    }
}
