using UnityEngine;

public interface IBoidSearch {
    void AddBoid(int index, Vector3 position, Boid boid);
    void RemoveBoid(int index);
    void UpdatePosition(int index, Vector3 position);
    (int numNeighbors, Boid[] neighbors) FindNeighbors(int index, float radius);
}