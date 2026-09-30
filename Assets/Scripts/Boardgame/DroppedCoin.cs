using UnityEngine;
using System.Collections;

public class DroppedCoin : MonoBehaviour
{
    public int value;
    public PlayerStats owner;

    [Header("References")]
    public Renderer auraRenderer;

    [Header("Launch")]
    public float launchDuration = 0.45f;
    public float arcHeight = 0.45f;

    [Header("Hover")]
    public float hoverHeight = 0.12f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.04f;
    public float rotationSpeed = 100f;

    [Header("Pickup")]
    public float pickupDelay = 0.35f;

    [Header("Magnet")]
    public float magnetRange = 2.5f;
    public float magnetStartSpeed = 2f;
    public float magnetAcceleration = 10f;
    public float collectDistance = 0.25f;
    public float targetHeightOffset = 0.8f;

    private Vector3 basePosition;
    private float bobOffset;
    private bool canBePickedUp = false;
    private bool hasSettled = false;
    private bool isMagnetMoving = false;

    private Transform magnetTarget;
    private float currentMagnetSpeed;

    public void Setup(int coinValue, PlayerStats coinOwner, Vector3 targetPos)
    {
        value = coinValue;
        owner = coinOwner;
        bobOffset = Random.Range(0f, 100f);

        SetupAura();
        StartCoroutine(AnimateLaunch(targetPos));
    }

    private void SetupAura()
    {
        if (auraRenderer == null || owner == null)
            return;

        Material mat = auraRenderer.material;
        mat.EnableKeyword("_EMISSION");

        Color baseEmission = mat.GetColor("_EmissionColor");

        Color tintedEmission = new Color(
            baseEmission.r * owner.playerColor.r,
            baseEmission.g * owner.playerColor.g,
            baseEmission.b * owner.playerColor.b
        );

        mat.SetColor("_EmissionColor", tintedEmission);

        Color baseColor = mat.color;
        Color tintedBase = new Color(
            owner.playerColor.r,
            owner.playerColor.g,
            owner.playerColor.b,
            baseColor.a
        );

        mat.color = tintedBase;
    }

    private IEnumerator AnimateLaunch(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float timer = 0f;

        while (timer < launchDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / launchDuration);

            Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;

            transform.position = flatPos + Vector3.up * arc;
            transform.Rotate(Vector3.up, rotationSpeed * 2f * Time.deltaTime, Space.World);

            yield return null;
        }

        Vector3 settlePos = transform.position;
        basePosition = new Vector3(settlePos.x, settlePos.y + hoverHeight, settlePos.z);

        float settleTimer = 0f;
        float settleDuration = 0.08f;
        Vector3 from = transform.position;

        while (settleTimer < settleDuration)
        {
            settleTimer += Time.deltaTime;
            float t = Mathf.Clamp01(settleTimer / settleDuration);
            transform.position = Vector3.Lerp(from, basePosition, t);
            yield return null;
        }

        transform.position = basePosition;
        hasSettled = true;

        yield return new WaitForSeconds(pickupDelay);
        canBePickedUp = true;
    }

    private void Update()
    {
        if (!canBePickedUp)
            return;

        if (isMagnetMoving)
        {
            MoveToTarget();
            return;
        }

        if (hasSettled)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobAmount;
            transform.position = basePosition + Vector3.up * bob;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        FindMagnetTarget();
    }

    private void FindMagnetTarget()
    {
        PlayerStats[] players = FindObjectsOfType<PlayerStats>();

        foreach (PlayerStats player in players)
        {
            if (player == null) continue;
            if (player == owner) continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);

            if (dist <= magnetRange)
            {
                magnetTarget = player.transform;
                currentMagnetSpeed = magnetStartSpeed;
                isMagnetMoving = true;
                return;
            }
        }
    }

    private void MoveToTarget()
    {
        if (magnetTarget == null)
        {
            isMagnetMoving = false;
            return;
        }

        currentMagnetSpeed += magnetAcceleration * Time.deltaTime;

        Vector3 targetPos = magnetTarget.position + Vector3.up * targetHeightOffset;
        Vector3 dir = (targetPos - transform.position).normalized;

        transform.position += dir * currentMagnetSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationSpeed * 3f * Time.deltaTime, Space.World);

        float dist = Vector3.Distance(transform.position, targetPos);

        if (dist <= collectDistance)
        {
            PlayerStats player = magnetTarget.GetComponent<PlayerStats>();

            if (player != null)
            {
                player.coins += value;
                Debug.Log(player.name + " picked up " + value + " coins");
            }

            Destroy(gameObject);
        }
    }
}