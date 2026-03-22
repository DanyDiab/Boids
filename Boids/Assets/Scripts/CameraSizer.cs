using UnityEngine;

public class CameraSizer : MonoBehaviour {
    
    Camera cam;
    [SerializeField] SimulationParameters simParams;


    void Start() {
       cam = Camera.main;
       BoidManager.OnBoidSpawn += updateCamSize;
    }

    public void updateCamSize() {
        float radius = simParams.SimBoundRadius;
        float size = (radius / 2) + 20;
        cam.orthographicSize = size;
    }
}