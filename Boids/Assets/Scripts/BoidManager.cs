using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoidManager : MonoBehaviour{
    int numBoids;
    GameObject[] boids;
    [SerializeField] GameObject boidPrefab;
    [SerializeField] GameObject simulationBounds;

    List<GameObject> boidPool;
    float width;
    float height;
    void Start(){
        numBoids = 10;
        boids = new GameObject[numBoids];
        boidPool = new List<GameObject>();
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
        float posX = halfWidth - Random.Range(0,width);
        float posZ = halfHeight - Random.Range(0,height);
        float randRot = Random.Range(0,359);
        Vector3 pos = new Vector3(posX,1,posZ);
        Quaternion rotation = Quaternion.Euler(0,randRot,0);
        return(pos,rotation);
    }

    void spawnBoids() {

        int poolCount = boidPool.Count;
        int maxGrab = Mathf.Min(numBoids,poolCount);

        if(maxGrab > 0) {
            int lastElement = boidPool.Count - 1;
            for(int i = 0; i < maxGrab; i++) {
                GameObject boid = boidPool[lastElement];
                boid.SetActive(true);
                boidPool.RemoveAt(lastElement);
                boids[i] = boid;
                (Vector3 pos, Quaternion rot) = getRandPosRot();
                Transform boidTrans = boid.transform;
                boidTrans.position = pos;
                boidTrans.rotation = rot;
                lastElement--;
            }
        }

        for(int i = maxGrab; i < numBoids; i++) {
            (Vector3 pos, Quaternion rot) = getRandPosRot();
            GameObject boid = Instantiate(boidPrefab,pos,rot);
            boids[i] = boid;
        }

    }

    void clearBoids() {
        boidPool.AddRange(boids);
        for(int i = 0; i < numBoids; i++) {
            boids[i].SetActive(false);      
            boids[i] = null;
        }
    }    
}
