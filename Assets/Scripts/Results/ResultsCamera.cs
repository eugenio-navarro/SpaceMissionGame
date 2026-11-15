using UnityEngine;

// Cámara lenta alrededor del satélite. Lo encuadra corrido a la izquierda para dejar lugar al panel.
public class ResultsCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 12.5f;
    public float height = 2.6f;
    public float centerAngle = -25f;  // grados
    public float swing = 22f;         // cuánto se mueve a cada lado
    public float speed = 0.12f;
    public float frameShift = 3.9f;   // corre el encuadre: el satélite queda a la izquierda

    void LateUpdate()
    {
        if (target == null) return;

        float angle = (centerAngle + Mathf.Sin(Time.time * speed) * swing) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Sin(angle) * distance, height, -Mathf.Cos(angle) * distance);
        transform.position = target.position + offset;

        Vector3 right = Vector3.Cross(Vector3.up, -offset).normalized;
        transform.LookAt(target.position + right * frameShift);
    }
}
