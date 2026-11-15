using UnityEngine;

// Giro lento de ida y vuelta de la cámara aérea del centro espacial.
public class HubCameraSway : MonoBehaviour
{
    public float amplitude = 10f; // grados
    public float period = 40f;    // segundos por vuelta completa

    float baseYaw;

    void Start() => baseYaw = transform.eulerAngles.y;

    void Update()
    {
        float yaw = baseYaw + Mathf.Sin(Time.time * 2f * Mathf.PI / period) * amplitude;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
