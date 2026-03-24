using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoidRadiusController : MonoBehaviour {
    [Header("Data")]
    [SerializeField] BoidInfo boidInfo;

    [Header("Sliders")]
    [SerializeField] Slider separationSlider;
    [SerializeField] Slider alignmentSlider;
    [SerializeField] Slider cohesionSlider;

    [Header("Titles")]
    [SerializeField] TMP_Text separationTitle;
    [SerializeField] TMP_Text alignmentTitle;
    [SerializeField] TMP_Text cohesionTitle;

    [Header("Shape Transforms")]
    [SerializeField] Transform separationShape;
    [SerializeField] Transform alignmentShape;
    [SerializeField] Transform cohesionShape;

    [SerializeField] float visualScaleMultiplier = 1f;

    void Start() {
        separationSlider.value = boidInfo.SeparationRadius;
        alignmentSlider.value = boidInfo.AlignmentRadius;
        cohesionSlider.value = boidInfo.CohesionRadius;

        UpdateSeparationVisual(boidInfo.SeparationRadius);
        UpdateAlignmentVisual(boidInfo.AlignmentRadius);
        UpdateCohesionVisual(boidInfo.CohesionRadius);

        separationSlider.onValueChanged.AddListener(OnSeparationChanged);
        alignmentSlider.onValueChanged.AddListener(OnAlignmentChanged);
        cohesionSlider.onValueChanged.AddListener(OnCohesionChanged);
    }

    void OnDestroy() {
        if (separationSlider != null) separationSlider.onValueChanged.RemoveListener(OnSeparationChanged);
        if (alignmentSlider != null) alignmentSlider.onValueChanged.RemoveListener(OnAlignmentChanged);
        if (cohesionSlider != null) cohesionSlider.onValueChanged.RemoveListener(OnCohesionChanged);
    }

    void OnSeparationChanged(float newValue) {
        boidInfo.SeparationRadius = newValue;
        UpdateSeparationVisual(newValue);
    }

    void OnAlignmentChanged(float newValue) {
        boidInfo.AlignmentRadius = newValue;
        UpdateAlignmentVisual(newValue);
    }

    void OnCohesionChanged(float newValue) {
        boidInfo.CohesionRadius = newValue;
        UpdateCohesionVisual(newValue);
    }

    void UpdateSeparationVisual(float radius) {
        float scaledRadius = radius * visualScaleMultiplier;
        separationShape.localScale = new Vector3(scaledRadius, scaledRadius, scaledRadius);
        separationTitle.text = "Separation: " + radius.ToString("F2");
    }

    void UpdateAlignmentVisual(float radius) {
        float scaledRadius = radius * visualScaleMultiplier;
        alignmentShape.localScale = new Vector3(scaledRadius, scaledRadius, scaledRadius);
        alignmentTitle.text = "Alignment: " + radius.ToString("F2");
    }

    void UpdateCohesionVisual(float radius) {
        float scaledRadius = radius * visualScaleMultiplier;
        cohesionShape.localScale = new Vector3(scaledRadius, scaledRadius, scaledRadius);
        cohesionTitle.text = "Cohesion: " + radius.ToString("F2");
    }
}