using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SimulationParameters", menuName = "Scriptable Objects/SimulationParameters")]
public class SimulationParameters : ScriptableObject{
    [SerializeField] int numBoids;
    [SerializeField] float simBoundRadius; 
    
    [SerializeField] UniformGridSearch search;

    [Header("Gizmo Options")]
    [SerializeField] GizmoStruct gizmoStruct;

    public int NumBoids { get => numBoids; set => numBoids = value; }
    public float SimBoundRadius { get => simBoundRadius; set => simBoundRadius = value; }
    public UniformGridSearch Search { get => search; set => search = value; }
    public GizmoStruct GizmoStruct {get => gizmoStruct; set => gizmoStruct = value; }
}
