using UnityEngine;

// Cámara lenta que gira alrededor de la nave, con el planeta de fondo.
public class ResultsCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 11f;
    public float height = 2.6f;
    public float degreesPerSecond = 6f;

    float yaw = 30f;

    void LateUpdate()
    {
        if (target == null) return;
        yaw += degreesPerSecond * Time.deltaTime;
        var offset = Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, height, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.down * 0.6f);
    }
}
