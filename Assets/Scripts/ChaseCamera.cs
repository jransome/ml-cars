using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public God EvolutionManager;
    public float OrbitSpeed = 2f;
    private Transform chaseTransform;

    private void Update()
    {
        CarBrain carToTrack = EvolutionManager.MostSuccessfulAlive;

        if (carToTrack)
            chaseTransform = carToTrack.transform;

        if (chaseTransform != null)
        {
            transform.position = chaseTransform.position;
            transform.Rotate(Vector3.up * OrbitSpeed * Time.deltaTime);
        }
    }
}
