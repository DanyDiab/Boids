using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BoidEducationManager : MonoBehaviour
{
    [Header("Boid Configuration")]
    [SerializeField] private BoidInfo boidInfo;
    [SerializeField] private SimulationParameters simParams;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text explanationText;
    
    [Header("Force Toggles")]
    [SerializeField] private Toggle separationToggle;
    [SerializeField] private Toggle alignmentToggle;
    [SerializeField] private Toggle cohesionToggle;

    [Header("Radius Sliders")]
    [SerializeField] private Slider separationRadiusSlider;
    [SerializeField] private Slider alignmentRadiusSlider;
    [SerializeField] private Slider cohesionRadiusSlider;

    private enum EducationStep
    {
        Separation,
        Alignment,
        Cohesion,
        Combined
    }

    private EducationStep currentStep = EducationStep.Separation;

    private void Start()
    {
        // Initialize UI with SO values
        if (separationRadiusSlider != null) separationRadiusSlider.value = boidInfo.SeparationRadius;
        if (alignmentRadiusSlider != null) alignmentRadiusSlider.value = boidInfo.AlignmentRadius;
        if (cohesionRadiusSlider != null) cohesionRadiusSlider.value = boidInfo.CohesionRadius;

        // Add listeners for sliders with null checks
        if (separationRadiusSlider != null)
        {
            separationRadiusSlider.onValueChanged.AddListener(val => {
                boidInfo.SeparationRadius = val;
            });
        }

        if (alignmentRadiusSlider != null)
        {
            alignmentRadiusSlider.onValueChanged.AddListener(val => {
                boidInfo.AlignmentRadius = val;
            });
        }

        if (cohesionRadiusSlider != null)
        {
            cohesionRadiusSlider.onValueChanged.AddListener(val => {
                boidInfo.CohesionRadius = val;
            });
        }

        // Add listeners for toggles
        if (separationToggle != null) separationToggle.onValueChanged.AddListener(OnSeparationToggleChanged);
        if (alignmentToggle != null) alignmentToggle.onValueChanged.AddListener(OnAlignmentToggleChanged);
        if (cohesionToggle != null) cohesionToggle.onValueChanged.AddListener(OnCohesionToggleChanged);

        UpdateStepUI();
    }

    public void NextStep()
    {
        if (currentStep < EducationStep.Combined)
        {
            currentStep++;
            UpdateStepUI();
        }
    }

    public void PreviousStep()
    {
        if (currentStep > EducationStep.Separation)
        {
            currentStep--;
            UpdateStepUI();
        }
    }

    private void UpdateStepUI()
    {
        // Reset all weights first
        boidInfo.SeparationForceWeight = 0;
        boidInfo.AlignmentForceWeight = 0;
        boidInfo.CohesionForceWeight = 0;

        switch (currentStep)
        {
            case EducationStep.Separation:
                boidInfo.SeparationForceWeight = 1.5f; 
                SetToggles(true, false, false);
                break;

            case EducationStep.Alignment:
                boidInfo.AlignmentForceWeight = 1.0f;
                SetToggles(false, true, false);
                break;

            case EducationStep.Cohesion:
                boidInfo.CohesionForceWeight = 1.0f;
                SetToggles(false, false, true);
                break;

            case EducationStep.Combined:
                boidInfo.SeparationForceWeight = 1.5f;
                boidInfo.AlignmentForceWeight = 1.0f;
                boidInfo.CohesionForceWeight = 1.0f;
                SetToggles(true, true, true);
                break;
        }

        RefreshExplanationText();
    }

    private void RefreshExplanationText()
    {
        string title = "Boid Rules";
        string desc = "Toggle the rules to see how they affect the flock.";

        if (separationToggle.isOn && alignmentToggle.isOn && cohesionToggle.isOn)
        {
            title = "Flocking (All Combined)";
            desc = "When all three rules are combined, complex 'emergent behavior' occurs. The boids now behave like a natural flock or school of fish.";
        }
        else if (separationToggle.isOn && !alignmentToggle.isOn && !cohesionToggle.isOn)
        {
            title = "1. Separation";
            desc = "<b>Separation:</b> Steer to avoid crowding local flockmates.\n\nThis rule prevents boids from colliding with each other. It creates a 'personal space' around each boid.";
        }
        else if (!separationToggle.isOn && alignmentToggle.isOn && !cohesionToggle.isOn)
        {
            title = "2. Alignment";
            desc = "<b>Alignment:</b> Steer towards the average heading of local flockmates.\n\nThis rule causes boids to fly in the same direction as their neighbors, creating synchronized motion.";
        }
        else if (!separationToggle.isOn && !alignmentToggle.isOn && cohesionToggle.isOn)
        {
            title = "3. Cohesion";
            desc = "<b>Cohesion:</b> Steer towards the average position (center of mass) of local flockmates.\n\nThis rule keeps the flock together by pulling boids toward the middle of the group.";
        }
        else if (separationToggle.isOn || alignmentToggle.isOn || cohesionToggle.isOn)
        {
            title = "Custom Combination";
            desc = "You are currently viewing a combination of rules. Observe how the boids interact with only these forces active.";
        }

        if (explanationText != null)
        {
            explanationText.text = $"<size=120%>{title}</size>\n\n{desc}";
        }
    }

    private void SetToggles(bool sep, bool align, bool coh)
    {
        separationToggle.SetIsOnWithoutNotify(sep);
        alignmentToggle.SetIsOnWithoutNotify(align);
        cohesionToggle.SetIsOnWithoutNotify(coh);
    }

    private void OnSeparationToggleChanged(bool isOn)
    {
        boidInfo.SeparationForceWeight = isOn ? 1.5f : 0f;
        RefreshExplanationText();
    }

    private void OnAlignmentToggleChanged(bool isOn)
    {
        boidInfo.AlignmentForceWeight = isOn ? 1.0f : 0f;
        RefreshExplanationText();
    }

    private void OnCohesionToggleChanged(bool isOn)
    {
        boidInfo.CohesionForceWeight = isOn ? 1.0f : 0f;
        RefreshExplanationText();
    }

    private void CheckIfCombined()
    {
        // This is now handled by RefreshExplanationText()
    }
}
