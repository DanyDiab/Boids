using UnityEngine;

public class BoidRotator : MonoBehaviour {
    [SerializeField] float rotationSpeedX = 45f;
    [SerializeField] float rotationSpeedY = 45f;
    [SerializeField] float rotationSpeedZ = 45f;

    void Update() {
        float xRotation = rotationSpeedX * Time.deltaTime;
        float yRotation = rotationSpeedY * Time.deltaTime;
        float zRotation = rotationSpeedZ * Time.deltaTime;

        transform.Rotate(xRotation, yRotation, zRotation, Space.Self);
    }
}