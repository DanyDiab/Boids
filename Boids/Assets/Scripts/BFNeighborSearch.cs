using System;
using UnityEngine;

public class BFNeighborSearch : IBoidSearch {
    Vector3[] boidPositions;
    Boid[] currNeighbors;
    Boid[] boids;
    int numBoids;

    
    public BFNeighborSearch(int numBoids) {
        this.numBoids = numBoids;
        currNeighbors = new Boid[numBoids - 1];
        boidPositions = new Vector3[numBoids];
        boids = new Boid[numBoids];
    }

    public void UpdatePosition(int idx, Vector3 newPos) {
        boidPositions[idx] = newPos;
    }
    public void AddBoid(int index, Vector3 position, Boid boid) {
        boidPositions[index] = position;
        boids[index] = boid;
    }

    public void RemoveBoid(int index) {
        boids[index] = null;
    }

    public (int, Boid[]) FindNeighbors(int index, float radius){
       Span<Vector3> positions = boidPositions;

        Vector3 currPos = positions[index];
        int numNeighbors = 0;
        float radiusSq = radius * radius;
        for(int i = 0; i < positions.Length; i++) {
            if(i == index) continue;
            Vector3 distance = currPos - positions[i];
            if(distance.sqrMagnitude <= radiusSq) {
                currNeighbors[numNeighbors] = boids[i];
                numNeighbors++;
            }
        }
        return (numNeighbors, currNeighbors);
    }
}
