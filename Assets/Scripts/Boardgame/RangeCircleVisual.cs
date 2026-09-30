using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RangeCircleVisual : MonoBehaviour
{
    public int segments = 40;
    public float radius = 3f;
    public float yOffset = 0.05f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        DrawCircle();
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        DrawCircle();
    }

    public void DrawCircle()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;

        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 pos = new Vector3(x, yOffset, z);
            lineRenderer.SetPosition(i, pos);
        }
    }
}