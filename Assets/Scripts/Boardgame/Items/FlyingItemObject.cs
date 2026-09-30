using UnityEngine;

public class FlyingItemObject : MonoBehaviour
{
    public float arcHeight = 1f;
    public bool scaleDownAtEnd = false;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float duration;
    private float time;

    private Vector3 initialScale;

    public void Init(Vector3 start, Vector3 target, float duration)
    {
        startPos = start;
        targetPos = target;
        this.duration = duration;
        time = 0f;

        initialScale = transform.localScale;
    }

    private void Update()
    {
        time += Time.deltaTime;
        float t = Mathf.Clamp01(time / duration);

        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        pos.y += arc;

        transform.position = pos;

        if (scaleDownAtEnd)
        {
            float scale = Mathf.Lerp(1f, 0.2f, t);
            transform.localScale = initialScale * scale;
        }

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}