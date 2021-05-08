using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public SpeciesEvolver EvolutionManager;
    public float OrbitSpeed = 2f;
    public Transform ChaseTransform = null;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1)) Time.timeScale = 1;
        if (Input.GetKeyUp(KeyCode.Alpha6)) Time.timeScale = 6;

        if (ChaseTransform != null)
        {
            transform.position = ChaseTransform.position;
            transform.Rotate(Vector3.up * OrbitSpeed * Time.deltaTime);
        }
    }
}
