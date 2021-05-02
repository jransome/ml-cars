using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public SpeciesEvolver EvolutionManager;
    public float OrbitSpeed = 2f;
    private Transform chaseTransform;
    private float lastCameraUpdate = 0f;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1)) Time.timeScale = 1;
        if (Input.GetKeyUp(KeyCode.Alpha6)) Time.timeScale = 6;

        if (Time.time > lastCameraUpdate + 1f && EvolutionManager.MostSuccessfulAlive != null)
        {
            lastCameraUpdate = Time.time;
            chaseTransform = EvolutionManager.MostSuccessfulAlive.transform;
        }

        if (chaseTransform != null)
        {
            transform.position = chaseTransform.position;
            transform.Rotate(Vector3.up * OrbitSpeed * Time.deltaTime);
        }
    }
}
