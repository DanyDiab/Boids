using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class BoidManager : MonoBehaviour{
    int numBoids;
    Boid[] boids;
    [SerializeField] GameObject boidPrefab;
    [SerializeField] GameObject simulationBounds;
    BFNeighborSearch search;

    List<Boid> boidPool;
    float width;
    float height;
    void Start(){
        numBoids = 1000;
        boids = new Boid[numBoids];
        boidPool = new List<Boid>();
        
        Vector3 simScale = simulationBounds.transform.localScale;
        width = simScale.x;
        height = simScale.y;
        spawnBoids();
    }

    void Update(){
        if(Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            clearBoids();
            spawnBoids();
        }
    }

    (Vector3, Quaternion) getRandPosRot() {
        float halfWidth = width / 2;
        float halfHeight = height / 2;
        float posX = halfWidth - UnityEngine.Random.Range(0,width);
        float posZ = halfHeight - UnityEngine.Random.Range(0,height);
        float randRot = UnityEngine.Random.Range(0,359);
        Vector3 pos = new Vector3(posX,1,posZ);
        Quaternion rotation = Quaternion.Euler(0,randRot,0);
        return(pos,rotation);
    }

    void spawnBoids() {

        int poolCount = boidPool.Count;
        int numGrabFromPool = Mathf.Min(numBoids,poolCount);
        search = new BFNeighborSearch(numBoids);
        if(numGrabFromPool > 0) {
            int lastElement = boidPool.Count - 1;
            for(int i = 0; i < numGrabFromPool; i++) {
                Boid boid = boidPool[lastElement];
                boid.init(i,boid.gameObject,search);
                boidPool.RemoveAt(lastElement);
                boids[i] = boid;
                (Vector3 pos, Quaternion rot) = getRandPosRot();
                Transform boidTrans = boid.transform;
                boidTrans.position = pos;
                boidTrans.rotation = rot;
                lastElement--;

            }
        }

        for(int i = numGrabFromPool; i < numBoids; i++) {
            (Vector3 pos, Quaternion rot) = getRandPosRot();
            GameObject boidGO = Instantiate(boidPrefab,pos,rot);
            Boid boid = boidGO.GetComponent<Boid>();
            boid.init(i,boidGO, search);
            boids[i] = boid;
        }

       
        
    }

    void clearBoids() {
        boidPool.AddRange(boids);
        for(int i = 0; i < numBoids; i++) {
            
            if(boids[i] == null) continue;
            boids[i].disable();   
            boids[i] = null;
        }
    }


}
