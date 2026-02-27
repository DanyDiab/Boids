using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BoidInfo", menuName = "Scriptable Objects/BoidInfo")]
public class BoidInfo : ScriptableObject{

    [Header("Speed")]
    [SerializeField] private float speed;

    [Header("Radii")]
    [SerializeField] private float separationRadius;
    [SerializeField] private float alignmentRadius;
    [SerializeField] private float cohesionRadius;

    private float simBoundRadius; 

    [Header("Force Weights")]
    [SerializeField] private float separationForceWeight;
    [SerializeField] private float alignmentForceWeight;
    [SerializeField] private float cohesionForceWeight;
    [SerializeField] private float centerForceWeight;

    public float Speed { get => speed; set => speed = value; }
    public float SeparationRadius { get => separationRadius; set => separationRadius = value; }
    public float AlignmentRadius { get => alignmentRadius; set => alignmentRadius = value; }
    public float CohesionRadius { get => cohesionRadius; set => cohesionRadius = value; }
    public float SimBoundRadius { get => simBoundRadius; set => simBoundRadius = value; }
    public float SeparationForceWeight { get => separationForceWeight; set => separationForceWeight = value; }
    public float AlignmentForceWeight { get => alignmentForceWeight; set => alignmentForceWeight = value; }
    public float CohesionForceWeight { get => cohesionForceWeight; set => cohesionForceWeight = value; }
    public float CenterForceWeight {get => centerForceWeight; set => centerForceWeight = value; }

}
