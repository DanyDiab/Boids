using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BoidInfo", menuName = "Scriptable Objects/BoidInfo")]
public class BoidInfo : ScriptableObject{

    [Header("Speed")]
    [SerializeField] float speed;
    [SerializeField] float turnSpeed;

    [Header("Radii")]
    [SerializeField] float separationRadius;
    [SerializeField] float alignmentRadius;
    [SerializeField] float cohesionRadius;

    [Header("Force Weights")]
    [SerializeField] float separationForceWeight;
    [SerializeField] float alignmentForceWeight;
    [SerializeField] float cohesionForceWeight;
    [SerializeField] float centerForceWeight;

    public float Speed { get => speed; set => speed = value; }
    public float TurnSpeed { get => turnSpeed; set => turnSpeed = value; }
    public float SeparationRadius { get => separationRadius; set => separationRadius = value; }
    public float AlignmentRadius { get => alignmentRadius; set => alignmentRadius = value; }
    public float CohesionRadius { get => cohesionRadius; set => cohesionRadius = value; }
    public float SeparationForceWeight { get => separationForceWeight; set => separationForceWeight = value; }
    public float AlignmentForceWeight { get => alignmentForceWeight; set => alignmentForceWeight = value; }
    public float CohesionForceWeight { get => cohesionForceWeight; set => cohesionForceWeight = value; }
    public float CenterForceWeight {get => centerForceWeight; set => centerForceWeight = value; }

}
