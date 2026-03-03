using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoidManager : MonoBehaviour{
    int numBoids;
    Boid[] boids;
    [SerializeField] GameObject boidPrefab;
    UniformGridSearch search;
    List<Boid> boidPool;
    float width;
    [SerializeField] BoidInfo boidInfo;
    [SerializeField] SimulationParameters simParams;

    void Start(){
        numBoids = simParams.NumBoids;
        boids = new Boid[numBoids];
        boidPool = new List<Boid>();
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
        float posX = halfWidth - UnityEngine.Random.Range(0,width);
        float posZ = halfWidth - UnityEngine.Random.Range(0,width);
        float randRot = UnityEngine.Random.Range(0,359);
        Vector3 pos = new Vector3(posX,1,posZ);
        Quaternion rotation = Quaternion.Euler(0,randRot,0);
        return(pos,rotation);
    }


    void spawnBoids() {
        width = simParams.SimBoundRadius;

        int poolCount = boidPool.Count;
        int numGrabFromPool = Mathf.Min(numBoids,poolCount);
        search = new UniformGridSearch(numBoids,10,width);
        if(numGrabFromPool > 0) {
            int lastElement = boidPool.Count - 1;
            for(int i = 0; i < numGrabFromPool; i++) {
                Boid boid = boidPool[lastElement];
                boid.init(i,boid.gameObject,search, boidInfo, simParams);
                boidPool.RemoveAt(lastElement);
                boids[i] = boid;
                (Vector3 pos, Quaternion rot) = getRandPosRot();
                Transform boidTrans = boid.transform;
                boidTrans.position = pos;
                lastElement--;
            }
        }

        for(int i = numGrabFromPool; i < numBoids; i++) {
            (Vector3 pos, Quaternion rot) = getRandPosRot();
            GameObject boidGO = Instantiate(boidPrefab,pos,Quaternion.identity);
            Boid boid = boidGO.GetComponent<Boid>();
            boid.init(i,boidGO, search, boidInfo, simParams);
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
