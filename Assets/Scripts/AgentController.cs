using UnityEngine;

public abstract class AgentController : MonoBehaviour
{
    public abstract void Throttle(float input);
    public abstract void Steer(float input);
    public abstract void Brake(float input);
    public abstract void ResetToPosition(Vector3 startPosition, Quaternion startRotation);
}
