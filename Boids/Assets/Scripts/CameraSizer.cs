using UnityEngine;

public class CameraSizer : MonoBehaviour {
    
    Camera cam;
    [SerializeField] SimulationParameters simParams;

    void Awake() {
       cam = GetComponent<Camera>();
       if (cam == null) cam = Camera.main;
       BoidManager.OnBoidSpawn += updateCamSize; 
    }

    void Start() {
       updateCamSize();
    }

    void OnDestroy() {
        BoidManager.OnBoidSpawn -= updateCamSize;
    }

    public void updateCamSize() {
        if (cam == null) {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
        }
        float radius = simParams.SimBoundRadius;
        float size = (radius / 2) + 20;
        cam.orthographicSize = size;
    }
}