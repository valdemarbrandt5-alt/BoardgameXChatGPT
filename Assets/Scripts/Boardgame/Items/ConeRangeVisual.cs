using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ConeRangeVisual : MonoBehaviour
{
    public int arcSegments = 24;
    public float radius = 3f;
    public float coneAngle = 45f;
    public float yOffset = 0.05f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        DrawCone();
    }

    public void SetValues(float newRadius, float newConeAngle)
    {
        radius = newRadius;
        coneAngle = newConeAngle;
        DrawCone();
    }

    public void DrawCone()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        int pointCount = arcSegments + 3;
        lineRenderer.positionCount = pointCount;
        lineRenderer.loop = false;

        float halfAngle = coneAngle * 0.5f;
        float startAngle = -halfAngle;
        float step = coneAngle / arcSegments;

        lineRenderer.SetPosition(0, new Vector3(0f, yOffset, 0f));

        for (int i = 0; i <= arcSegments; i++)
        {
            float angle = startAngle + step * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 point = new Vector3(
                Mathf.Sin(rad) * radius,
                yOffset,
                Mathf.Cos(rad) * radius
            );

            lineRenderer.SetPosition(i + 1, point);
        }

        lineRenderer.SetPosition(pointCount - 1, new Vector3(0f, yOffset, 0f));
    }
}