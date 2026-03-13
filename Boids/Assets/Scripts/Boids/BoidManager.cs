using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoidManager : MonoBehaviour{
    int numBoids;
    Boid[] boids;
    [SerializeField] GameObject boidPrefab;
    IBoidSearch search;
    List<Boid> boidPool;
    float width;
    [SerializeField] BoidInfo boidInfo;
    [SerializeField] SimulationParameters simParams;

    public delegate void BoidSpawnEvent();
    public static event BoidSpawnEvent OnBoidSpawn;


    void Awake(){
        init();
        boidPool = new List<Boid>();
        spawnBoids();
        OnBoidSpawn += init;
    }
    void init() {
        width = simParams.SimBoundRadius;
        numBoids = simParams.NumBoids;
        boids = new Boid[numBoids];
        initSearch();
    }

    void initSearch() {
        switch (simParams.CurrSearchAlgo) {
            case (SearchAlgos.BF): {
                search = new BFNeighborSearch(numBoids);
                break;
            }
            case (SearchAlgos.UNIFORMGRID): {
                search = new UniformGridSearch(numBoids,simParams.CellSize,width);
                break;
            }
            case (SearchAlgos.QUADTREE): {
                search = new QuadTreeSearch(numBoids,simParams.LeafCapacity, width);
                break;       
            }

        }
    }

    void Update(){
        if(Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            clearBoids();
            init();
            spawnBoids();
        }
    }

    (Vector3, Quaternion) getRandPosRot() {
        float halfWidth = width / 2;
        float posX = halfWidth - Random.Range(0,width);
        float posZ = halfWidth - Random.Range(0,width);
        float randRot = Random.Range(0,359);
        Vector3 pos = new Vector3(posX,1,posZ);
        Quaternion rotation = Quaternion.Euler(0,randRot,0);
        return(pos,rotation);
    }


    void spawnBoids() {
        OnBoidSpawn?.Invoke();


        int poolCount = boidPool.Count;
        int numGrabFromPool = Mathf.Min(numBoids,poolCount);
        Debug.Log("need to grab " + numGrabFromPool + " Boids from pool");

        if(numGrabFromPool > 0) {
            int lastElement = poolCount - 1;
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
        Debug.Log("need to make " + (numBoids -numGrabFromPool) + " Boids");
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
