using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public SpeciesEvolver EvolutionManager;
    public float OrbitSpeed = 2f;
    private Transform chaseTransform = null;

    private void Start()
    {
        MostSuccessfulPoller.OnMostSuccessfulAliveChanged += (newCarBrain) =>
        {
            chaseTransform = newCarBrain.transform;
        };
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1)) Time.timeScale = 1;
        if (Input.GetKeyUp(KeyCode.Alpha6)) Time.timeScale = 6;

        if (chaseTransform != null)
        {
            transform.position = chaseTransform.position;
            transform.Rotate(Vector3.up * OrbitSpeed * Time.deltaTime);
        }
    }
}
