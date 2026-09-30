using UnityEngine;

public class FlyingCoin : MonoBehaviour
{
    public float arcHeight = 1.5f;
    public bool scaleDownAtEnd = true;

    [Header("Aura")]
    public Renderer auraRenderer;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float duration;
    private float time;

    private Vector3 initialScale;

    private bool hasGivenReward = false;
    private Transform targetTransform;
    private int coinValue = 1;

    public void Init(
        Vector3 start,
        Vector3 target,
        float duration,
        Color playerColor,
        Transform targetTransform
    )
    {
        startPos = start;
        targetPos = target;
        this.duration = duration;
        time = 0f;

        initialScale = transform.localScale;
        this.targetTransform = targetTransform;

        ApplyAuraColor(playerColor);
    }

    private void ApplyAuraColor(Color playerColor)
    {
        if (auraRenderer == null)
            return;

        Material mat = auraRenderer.material;
        mat.EnableKeyword("_EMISSION");

        // samme metode som dit DroppedCoin script
        Color baseEmission = mat.GetColor("_EmissionColor");

        Color tintedEmission = new Color(
            baseEmission.r * playerColor.r * 2f,
            baseEmission.g * playerColor.g * 2f,
            baseEmission.b * playerColor.b * 2f
        );

        mat.SetColor("_EmissionColor", tintedEmission);

        Color baseColor = mat.color;
        Color tintedBase = new Color(
            playerColor.r,
            playerColor.g,
            playerColor.b,
            baseColor.a
        );

        mat.color = tintedBase;
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

        // 💥 Giver +1 når den rammer
        if (t >= 1f && !hasGivenReward)
        {
            hasGivenReward = true;

            if (targetTransform != null)
            {
                PlayerStats stats = targetTransform.GetComponent<PlayerStats>();

                if (stats != null)
                {
                    stats.coins += coinValue;

                    if (DamagePopupManager.Instance != null)
                    {
                        DamagePopupManager.Instance.ShowHealPopup(
                            targetTransform.position,
                            coinValue
                        );
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}