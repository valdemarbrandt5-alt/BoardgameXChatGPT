using UnityEngine;

public class DiceVisualController : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 1.5f, 0f);
    public Vector3 rotationSpeed = new Vector3(240f, 360f, 180f);

    private Transform target;
    private bool isVisibleAndSpinning = false;

    public void Show(Transform followTarget)
    {
        target = followTarget;
        isVisibleAndSpinning = true;
        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void HideAndDestroy()
    {
        isVisibleAndSpinning = false;
        Destroy(gameObject);
    }

    private void Update()
    {
        if (!isVisibleAndSpinning || target == null)
            return;

        UpdatePosition();
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void UpdatePosition()
    {
        transform.position = target.position + offset;
    }
}