using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Shake(float duration, float strength)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        Vector3 originalPos = transform.localPosition;
        float timer = 0f;

        while (timer < duration)
        {
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}