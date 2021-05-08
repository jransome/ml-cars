using UnityEngine;

public class ConnectionView : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;

    public void Init(Vector2 start, Vector2 end)
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPositions(new Vector3[] { start, end });
        gameObject.SetActive(true);
    }

    public void UpdateView(float value)
    {
        lineRenderer.startWidth = lineRenderer.endWidth = Mathf.Abs(value / 100);
    }
}
