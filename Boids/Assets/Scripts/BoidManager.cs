using UnityEngine;
using UnityEngine.InputSystem;

public class BoidManager : MonoBehaviour{
    int numBoids;
    GameObject[] boids;
    [SerializeField] GameObject boidPrefab;
    [SerializeField] GameObject simulationBounds;
    float width;
    float height;
    void Start(){
        numBoids = 10;
        boids = new GameObject[numBoids];
        Vector3 simScale = simulationBounds.transform.localScale;
        width = simScale.x;
        height = simScale.y;
    }


    void Update(){
        if(Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            clearBoids();
            spawnBoids();
        }
    }

    void spawnBoids() {
        float halfWidth = width / 2;
        float halfHeight = height / 2;
        for(int i = 0; i < numBoids; i++) {
            float posX = halfWidth - Random.Range(0,width);
            float posZ = halfHeight - Random.Range(0,height);
            float randRot = Random.Range(0,359);
            Vector3 pos = new Vector3(posX,1,posZ);
            Quaternion rotation = Quaternion.Euler(0,randRot,0);
            GameObject boid = Instantiate(boidPrefab,pos,rotation);
            boids[i] = boid;
        }
    }

    void clearBoids() {
        for(int i = 0; i < numBoids; i++) {
            GameObject boid = boids[i];
            Destroy(boid);
            boids[i] = null;
        }
    }


}
