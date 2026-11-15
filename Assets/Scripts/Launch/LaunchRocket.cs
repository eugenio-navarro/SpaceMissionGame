using UnityEngine;

// Una variante de cohete armada en la escena. LaunchSequence elige una por id.
public class LaunchRocket : MonoBehaviour
{
    public string id = "mediano";
    public float height = 8f;
    public Transform stage1;
    public ParticleSystem stage1Flame;
    public ParticleSystem stage1Smoke;
    public ParticleSystem stage2Flame;
}
