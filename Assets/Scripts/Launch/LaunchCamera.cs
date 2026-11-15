using UnityEngine;

// Cámara simple que sigue al cohete con un offset que se puede ir cambiando.
public class LaunchCamera : MonoBehaviour
{
    public Transform target;
    public float lookHeight = 4f;
    public Vector3 offset = new Vector3(7f, 1.5f, -12f);
    public float offsetLerpSpeed = 1.2f;
    public float shake;
    public float minY = 1f; // la cámara nunca baja de acá (no se mete bajo la plataforma)

    // Objeto que copia la posición de la cámara pero no su rotación (campo de estrellas).
    public Transform positionFollower;

    Vector3 currentOffset;

    void Start()
    {
        currentOffset = offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        currentOffset = Vector3.Lerp(currentOffset, offset, Time.deltaTime * offsetLerpSpeed);

        Vector3 pos = target.position + currentOffset;
        if (shake > 0f) pos += Random.insideUnitSphere * shake;

        if (pos.y < minY) pos.y = minY;
        transform.position = pos;
        transform.LookAt(target.position + Vector3.up * lookHeight);

        if (positionFollower != null) positionFollower.position = transform.position;
    }
}
