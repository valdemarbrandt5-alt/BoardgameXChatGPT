using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BoardGame/Items/Shotgun Behaviour")]
public class ShotgunItemBehaviour : BaseItemBehaviour
{
    [Header("Visuals")]
    public GameObject shotgunWeaponPrefab;
    public GameObject linePrefab;
    public GameObject targetIndicatorPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Aim Settings")]
    public float hitRange = 5f;
    public float indicatorRange = 2f;
    public float aimRotateSpeed = 140f;
    public float lineHeightOffset = 1f;
    public float targetIndicatorHeightOffset = 0.05f;

    [Header("Hit Detection")]
    public float hitWidth = 0.75f;

    [Header("Damage")]
    public int minDamage = 4;
    public int maxDamage = 20;

    [Header("Shot Feel")]
    public float shotDelay = 0.08f;
    public float postShotDelay = 0.35f;
    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.18f;

    [Header("Camera")]
    [Range(0f, 1f)] public float cameraFollowTowardAim = 0.25f;

    private ItemUseContext context;
    private PlayerWeaponVisuals weaponVisuals;
    private PlayerAimRotation aimRotation;
    private CameraFollow cameraFollow;

    private GameObject spawnedLine;
    private LineRenderer lineRenderer;

    private readonly List<GameObject> spawnedIndicators = new List<GameObject>();
    private readonly List<PlayerController> currentTargets = new List<PlayerController>();

    private float currentAimAngle;
    private bool isShooting = false;

    public override void BeginUse(ItemUseContext context)
    {
        this.context = context;
        weaponVisuals = context.userPlayer.GetComponent<PlayerWeaponVisuals>();
        aimRotation = context.userPlayer.GetComponent<PlayerAimRotation>();
        cameraFollow = context.turnManager.cameraFollow;
        isShooting = false;

        currentAimAngle = context.userPlayer.transform.eulerAngles.y;

        if (weaponVisuals != null && shotgunWeaponPrefab != null)
        {
            weaponVisuals.ShowWeapon(shotgunWeaponPrefab);
        }

        SpawnLineIfNeeded();
        UpdateAimAndTargets();
        UpdateLaserLine();
        UpdateCameraAimFollow();
    }

    public override void HandleInput()
    {
        if (isShooting)
            return;

        float horizontal = 0f;

        if (Input.GetKey(KeyCode.A))
            horizontal -= 1f;

        if (Input.GetKey(KeyCode.D))
            horizontal += 1f;

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            currentAimAngle += horizontal * aimRotateSpeed * Time.deltaTime;

            UpdateAimAndTargets();
            UpdateLaserLine();
            UpdateCameraAimFollow();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            context.turnManager.StartCoroutine(ShootRoutine());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUse();
            ItemUseManager.Instance.CancelActiveItemUse();
        }
    }

    public override void CancelUse()
    {
        CleanupVisuals();

        if (weaponVisuals != null)
        {
            weaponVisuals.HideWeapon();
        }

        if (aimRotation != null)
        {
            aimRotation.SetIdle();
        }

        if (cameraFollow != null)
        {
            cameraFollow.ClearOverrideWorldPosition();
        }
    }

    private void SpawnLineIfNeeded()
    {
        if (spawnedLine != null || linePrefab == null)
            return;

        spawnedLine = Instantiate(linePrefab);
        lineRenderer = spawnedLine.GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            Debug.LogWarning("Shotgun line prefab has no LineRenderer.");
        }
    }

    private void UpdateAimAndTargets()
    {
        Vector3 aimDirection = GetAimDirection();
        Vector3 aimPoint = context.userPlayer.transform.position + aimDirection * indicatorRange;

        if (aimRotation != null)
        {
            aimRotation.SetAiming(aimPoint);
        }

        FindTargetsNearLaser(aimDirection);
        UpdateTargetIndicators();
    }

    private Vector3 GetAimDirection()
    {
        Vector3 dir = Quaternion.Euler(0f, currentAimAngle, 0f) * Vector3.forward;
        dir.y = 0f;
        return dir.normalized;
    }

    private void FindTargetsNearLaser(Vector3 aimDirection)
    {
        currentTargets.Clear();

        PlayerController[] players = context.turnManager.players;
        if (players == null)
            return;

        Vector3 origin = context.userPlayer.transform.position;
        Vector3 end = origin + aimDirection * hitRange;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player = players[i];
            if (player == null || player == context.userPlayer)
                continue;

            Vector3 targetPos = player.transform.position;
            targetPos.y = origin.y;

            float alongLine;
            float distanceToLine = DistancePointToSegmentXZ(targetPos, origin, end, out alongLine);

            if (alongLine < 0f || alongLine > hitRange)
                continue;

            if (distanceToLine <= hitWidth)
            {
                currentTargets.Add(player);
            }
        }
    }

    private float DistancePointToSegmentXZ(Vector3 point, Vector3 start, Vector3 end, out float alongSegment)
    {
        Vector2 p = new Vector2(point.x, point.z);
        Vector2 a = new Vector2(start.x, start.z);
        Vector2 b = new Vector2(end.x, end.z);

        Vector2 ab = b - a;
        float abSqr = ab.sqrMagnitude;

        if (abSqr < 0.0001f)
        {
            alongSegment = 0f;
            return Vector2.Distance(p, a);
        }

        float t = Vector2.Dot(p - a, ab) / abSqr;
        t = Mathf.Clamp01(t);

        Vector2 closest = a + ab * t;
        alongSegment = Vector2.Distance(a, closest);

        return Vector2.Distance(p, closest);
    }

    private void UpdateTargetIndicators()
    {
        for (int i = 0; i < spawnedIndicators.Count; i++)
        {
            if (spawnedIndicators[i] != null)
            {
                Destroy(spawnedIndicators[i]);
            }
        }

        spawnedIndicators.Clear();

        if (targetIndicatorPrefab == null)
            return;

        for (int i = 0; i < currentTargets.Count; i++)
        {
            if (currentTargets[i] == null)
                continue;

            GameObject indicator = Instantiate(targetIndicatorPrefab);
            indicator.transform.position = currentTargets[i].transform.position + Vector3.up * targetIndicatorHeightOffset;
            spawnedIndicators.Add(indicator);
        }
    }

    private void UpdateLaserLine()
    {
        if (lineRenderer == null)
            return;

        Vector3 start = GetLineStartPosition();
        Vector3 end = start + GetAimDirection() * indicatorRange;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private Vector3 GetLineStartPosition()
    {
        if (weaponVisuals != null)
        {
            Transform muzzle = weaponVisuals.GetMuzzlePoint();
            if (muzzle != null)
                return muzzle.position;

            Transform anchor = weaponVisuals.GetWeaponAnchor();
            if (anchor != null)
                return anchor.position;
        }

        return context.userPlayer.transform.position + Vector3.up * lineHeightOffset;
    }

    private void UpdateCameraAimFollow()
    {
        if (cameraFollow == null || context == null || context.userPlayer == null)
            return;

        Vector3 playerPos = context.userPlayer.transform.position;
        Vector3 aimPos = playerPos + GetAimDirection() * indicatorRange;

        Vector3 cameraFocusPoint = Vector3.Lerp(playerPos, aimPos, cameraFollowTowardAim);
        cameraFocusPoint.y = playerPos.y;

        cameraFollow.SetOverrideWorldPosition(cameraFocusPoint);
    }

    private IEnumerator ShootRoutine()
    {
        if (isShooting)
            yield break;

        isShooting = true;

        UpdateAimAndTargets();
        UpdateLaserLine();
        UpdateCameraAimFollow();
        SpawnMuzzleFlash();

        yield return new WaitForSeconds(shotDelay);

        for (int i = 0; i < currentTargets.Count; i++)
        {
            PlayerController target = currentTargets[i];
            if (target == null)
                continue;

            PlayerStats targetStats = target.GetComponent<PlayerStats>();
            if (targetStats == null)
                continue;

            float distance = Vector3.Distance(
                context.userPlayer.transform.position,
                target.transform.position
            );

            int damage = CalculateDamage(distance);
            targetStats.TakeDamageFromSource(damage, context.userPlayer.transform);

            Debug.Log(
                context.userPlayer.name +
                " hit " +
                target.name +
                " with Shotgun for " +
                damage +
                " damage."
            );
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeDuration, shakeStrength);
        }

        yield return new WaitForSeconds(postShotDelay);

        CleanupVisuals();

        if (weaponVisuals != null)
        {
            weaponVisuals.HideWeapon();
        }

        if (aimRotation != null)
        {
            aimRotation.SetIdle();
        }

        if (cameraFollow != null)
        {
            cameraFollow.ClearOverrideWorldPosition();
        }

        ItemUseManager.Instance.FinishActiveItemUse(false);
    }

    private int CalculateDamage(float distance)
    {
        float normalized = 1f - Mathf.Clamp01(distance / hitRange);
        return Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, normalized));
    }

    private void SpawnMuzzleFlash()
    {
        if (muzzleFlashPrefab == null)
            return;

        Vector3 spawnPos = context.userPlayer.transform.position + Vector3.up;

        if (weaponVisuals != null)
        {
            Transform muzzle = weaponVisuals.GetMuzzlePoint();
            if (muzzle != null)
            {
                spawnPos = muzzle.position;
            }
            else
            {
                Transform anchor = weaponVisuals.GetWeaponAnchor();
                if (anchor != null)
                {
                    spawnPos = anchor.position;
                }
            }
        }

        Instantiate(muzzleFlashPrefab, spawnPos, Quaternion.Euler(0f, currentAimAngle, 0f));
    }

    private void CleanupVisuals()
    {
        if (spawnedLine != null)
        {
            Destroy(spawnedLine);
            spawnedLine = null;
            lineRenderer = null;
        }

        for (int i = 0; i < spawnedIndicators.Count; i++)
        {
            if (spawnedIndicators[i] != null)
            {
                Destroy(spawnedIndicators[i]);
            }
        }

        spawnedIndicators.Clear();
        currentTargets.Clear();
    }
}