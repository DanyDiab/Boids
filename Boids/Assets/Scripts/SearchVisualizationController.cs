using UnityEngine;
using UnityEngine.InputSystem;

public class SearchVisualizationController : MonoBehaviour {
    [SerializeField] SimulationParameters simParams;
    [SerializeField] BoidManager boidManager;
    
    Camera mainCam;
    float defaultOrthographicSize;
    Vector3 defaultCameraPosition;
    Quaternion defaultCameraRotation;

    float currentLerpFactor = 0f;
    const float minTimeScale = 0.05f;

    void Start() {
        mainCam = Camera.main;
        defaultOrthographicSize = mainCam.orthographicSize;
        defaultCameraPosition = mainCam.transform.position;
        defaultCameraRotation = mainCam.transform.rotation;
    }

    void Update() {
        if (Keyboard.current == null) return;

        // Toggle Visualization
        if (Keyboard.current.vKey.wasPressedThisFrame) {
            simParams.IsVisualizingSearch = !simParams.IsVisualizingSearch;
            if (simParams.IsVisualizingSearch) {
                // Ensure we have a valid target ID
                if (simParams.TargetBoidID < 0 || simParams.TargetBoidID >= simParams.NumBoids) {
                    simParams.TargetBoidID = 0;
                }
            }
        }

        // Cycle through boids when in visualization mode
        if (simParams.IsVisualizingSearch) {
            if (Keyboard.current.tabKey.wasPressedThisFrame) {
                simParams.TargetBoidID = (simParams.TargetBoidID + 1) % simParams.NumBoids;
            }
        }

        HandleLerps();
    }

    void HandleLerps() {
        float targetFactor = simParams.IsVisualizingSearch ? 1f : 0f;
        currentLerpFactor = Mathf.MoveTowards(currentLerpFactor, targetFactor, Time.unscaledDeltaTime * simParams.SearchVisLerpSpeed);

        // Lerp TimeScale
        Time.timeScale = Mathf.Lerp(1f, minTimeScale, currentLerpFactor);

        // Lerp Camera
        if (currentLerpFactor > 0) {
            Boid targetBoid = GetTargetBoid();
            if (targetBoid != null) {
                Vector3 targetPos = targetBoid.transform.position;
                // Maintain camera's height and rotation for top-down view
                Vector3 cameraTargetPos = new Vector3(targetPos.x, defaultCameraPosition.y, targetPos.z);
                
                mainCam.transform.position = Vector3.Lerp(defaultCameraPosition, cameraTargetPos, currentLerpFactor);
                mainCam.orthographicSize = Mathf.Lerp(defaultOrthographicSize, simParams.ZoomOrthographicSize, currentLerpFactor);
            }
        } else {
            mainCam.transform.position = defaultCameraPosition;
            mainCam.orthographicSize = defaultOrthographicSize;
        }
    }

    Boid GetTargetBoid() {
        if (boidManager != null && boidManager.boids != null && 
            simParams.TargetBoidID >= 0 && simParams.TargetBoidID < boidManager.boids.Length) {
            return boidManager.boids[simParams.TargetBoidID];
        }
        return null;
    }
}
