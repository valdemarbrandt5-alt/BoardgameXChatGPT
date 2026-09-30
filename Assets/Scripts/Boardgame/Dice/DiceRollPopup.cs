using TMPro;
using UnityEngine;

public class DiceRollPopup : MonoBehaviour
{
    public TMP_Text popupText;

    public Vector3 offset = new Vector3(0f, 2.5f, 0f);
    public float duration = 1f;
    public float riseAmount = 0.8f;

    [Header("Scale Pop")]
    public float startScale = 1.8f;
    public float endScale = 1f;

    [Header("Fade")]
    public float startAlpha = 1f;
    public float endAlpha = 0f;

    private Transform target;
    private bool isShowing = false;
    private float timer = 0f;
    private Vector3 startOffset;

    private void Awake()
    {
        startOffset = offset;
        gameObject.SetActive(false);
    }

    public void Show(Transform followTarget, int rolledValue)
    {
        target = followTarget;
        timer = 0f;
        offset = startOffset;
        isShowing = true;

        if (popupText != null)
        {
            popupText.text = rolledValue.ToString();
            popupText.transform.localScale = Vector3.one * startScale;

            Color c = popupText.color;
            c.a = startAlpha;
            popupText.color = c;
        }

        gameObject.SetActive(true);
        UpdatePosition();
    }

    private void Update()
    {
        if (!isShowing || target == null || popupText == null)
            return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        offset = startOffset + Vector3.up * (riseAmount * t);
        UpdatePosition();

        float eased = 1f - Mathf.Pow(1f - t, 3f);

        float scale = Mathf.Lerp(startScale, endScale, eased);
        popupText.transform.localScale = Vector3.one * scale;

        Color c = popupText.color;
        c.a = Mathf.Lerp(startAlpha, endAlpha, t);
        popupText.color = c;

        if (timer >= duration)
        {
            isShowing = false;
            gameObject.SetActive(false);
            target = null;
        }
    }

    private void UpdatePosition()
    {
        transform.position = target.position + offset;
    }
}