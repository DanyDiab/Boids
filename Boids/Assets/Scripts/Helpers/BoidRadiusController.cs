using UnityEngine;
using TMPro;

public class BoidRadiusController : MonoBehaviour {
    [Header("Data")]
    [SerializeField] BoidInfo boidInfo;
    [Header("Shape Transforms")]
    [SerializeField] Transform separationShape;
    [SerializeField] Transform alignmentShape;
    [SerializeField] Transform cohesionShape;

    [SerializeField] float visualScaleMultiplier = 1f;

    private float lastSeparationRadius;
    private float lastAlignmentRadius;
    private float lastCohesionRadius;

    void Start() {
        UpdateAllVisuals();
    }

    void Update() {
        if (boidInfo == null) return;

        bool hasChanged = false;

        if (boidInfo.SeparationRadius != lastSeparationRadius) {
            UpdateSeparationVisual(boidInfo.SeparationRadius);
            hasChanged = true;
        }
        
        if (boidInfo.AlignmentRadius != lastAlignmentRadius) {
            UpdateAlignmentVisual(boidInfo.AlignmentRadius);
            hasChanged = true;
        }
        
        if (boidInfo.CohesionRadius != lastCohesionRadius) {
            UpdateCohesionVisual(boidInfo.CohesionRadius);
            hasChanged = true;
        }

        if (hasChanged) {
            UpdateLastKnownValues();
        }
    }

    void UpdateAllVisuals() {
        if (boidInfo == null) return;
        
        UpdateSeparationVisual(boidInfo.SeparationRadius);
        UpdateAlignmentVisual(boidInfo.AlignmentRadius);
        UpdateCohesionVisual(boidInfo.CohesionRadius);
        
        UpdateLastKnownValues();
    }

    void UpdateLastKnownValues() {
        lastSeparationRadius = boidInfo.SeparationRadius;
        lastAlignmentRadius = boidInfo.AlignmentRadius;
        lastCohesionRadius = boidInfo.CohesionRadius;
    }

    void UpdateSeparationVisual(float radius) {
        float scaledRadius = radius * visualScaleMultiplier;
        separationShape.localScale = new Vector3(scaledRadius, scaledRadius, scaledRadius);
    }

    void UpdateAlignmentVisual(float radius) {
        float scaledRadius = radius * visualScaleMultiplier;
        alignmentShape.localScale = new Vector3(scaledRadius, scaledRadius, scaledRadius);
    }

    void UpdateCohesionVisual(float radius) {
        float scaledRadius = radius * visualScaleMultiplier;
        cohesionShape.localScale = new Vector3(scaledRadius, scaledRadius, scaledRadius);
    }
}