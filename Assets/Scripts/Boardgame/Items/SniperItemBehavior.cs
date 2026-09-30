using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "BoardGame/Items/Sniper Behaviour")]
public class SniperItemBehaviour : BaseItemBehaviour
{
    [Header("Target Indicator")]
    public GameObject targetIndicatorPrefab;
    public Vector3 indicatorOffset = new Vector3(0f, 0.05f, 0f);

    [Header("Laser Line")]
    public GameObject linePrefab;
    public Vector3 lineStartOffset = new Vector3(0f, 1f, 0f);
    public Vector3 lineEndOffset = new Vector3(0f, 1f, 0f);

    [Header("Weapon Visual")]
    public GameObject sniperWeaponPrefab;

    [Header("Shot Feel")]
    public float shotDelay = 0.1f;
    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.15f;
    public float postHitCameraDelay = 1f;

    private ItemUseContext context;
    private PlayerController[] players;
    private int targetIndex = 0;

    private GameObject spawnedIndicator;
    private GameObject spawnedLine;
    private LineRenderer lineRenderer;

    private bool isShooting = false;

    private CameraFollow cameraFollow;
    private PlayerWeaponVisuals weaponVisuals;
    private PlayerAimRotation aimRotation;

    public override void BeginUse(ItemUseContext context)
    {
        this.context = context;
        players = context.turnManager.players;
        isShooting = false;

        cameraFollow = context.turnManager.cameraFollow;
        weaponVisuals = context.userPlayer.GetComponent<PlayerWeaponVisuals>();
        aimRotation = context.userPlayer.GetComponent<PlayerAimRotation>();

        targetIndex = FindFirstValidTargetIndex();

        if (targetIndex == -1)
        {
            Debug.Log("No valid sniper targets.");
            ItemUseManager.Instance.CancelActiveItemUse();
            return;
        }

        if (weaponVisuals != null && sniperWeaponPrefab != null)
        {
            weaponVisuals.ShowWeapon(sniperWeaponPrefab);
        }

        SpawnLineIfNeeded();
        UpdateIndicatorPosition();
        UpdateLine();
        UpdateCameraTarget();
        UpdatePlayerAim();
    }

    public override void HandleInput()
    {
        if (isShooting)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            CycleTarget(-1);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            CycleTarget(1);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (context != null && context.turnManager != null)
            {
                context.turnManager.StartCoroutine(ShootRoutine());
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUse();
            ItemUseManager.Instance.CancelActiveItemUse();
            return;
        }

        UpdateLine();
        UpdateCameraTarget();
        UpdatePlayerAim();
    }

    public override void CancelUse()
    {
        ClearVisuals();
        ClearCameraOverride();
        ClearPlayerAim();

        if (weaponVisuals != null)
        {
            weaponVisuals.HideWeapon();
        }

        isShooting = false;
    }

    private int FindFirstValidTargetIndex()
    {
        if (players == null || players.Length <= 1)
            return -1;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i] != context.userPlayer)
                return i;
        }

        return -1;
    }

    private void CycleTarget(int direction)
    {
        if (players == null || players.Length <= 1)
            return;

        int attempts = 0;

        do
        {
            targetIndex += direction;

            if (targetIndex < 0)
                targetIndex = players.Length - 1;

            if (targetIndex >= players.Length)
                targetIndex = 0;

            attempts++;
        }
        while ((players[targetIndex] == null || players[targetIndex] == context.userPlayer) && attempts <= players.Length);

        UpdateIndicatorPosition();
        UpdateLine();
        UpdateCameraTarget();
        UpdatePlayerAim();
    }

    private IEnumerator ShootRoutine()
    {
        if (isShooting)
            yield break;

        isShooting = true;

        PlayerController targetPlayer = players[targetIndex];
        if (targetPlayer == null || targetPlayer == context.userPlayer)
        {
            isShooting = false;
            yield break;
        }

        PlayerStats targetStats = targetPlayer.GetComponent<PlayerStats>();
        if (targetStats == null)
        {
            isShooting = false;
            yield break;
        }

        UpdateLine();
        UpdateCameraTarget();
        UpdatePlayerAim();

        yield return new WaitForSeconds(shotDelay);

        int damage = Random.Range(10, 16);
        targetStats.TakeDamageFromSource(damage, context.userPlayer.transform);

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeDuration, shakeStrength);
        }

        Debug.Log(context.userPlayer.name + " used Sniper on " + targetPlayer.name + " for " + damage + " damage.");

        yield return new WaitForSeconds(postHitCameraDelay);

        ClearVisuals();
        ClearCameraOverride();
        ClearPlayerAim();

        if (weaponVisuals != null)
        {
            weaponVisuals.HideWeapon();
        }

        ItemUseManager.Instance.FinishActiveItemUse(false);
    }

    private void SpawnLineIfNeeded()
    {
        if (spawnedLine != null || linePrefab == null)
            return;

        spawnedLine = Instantiate(linePrefab);
        lineRenderer = spawnedLine.GetComponent<LineRenderer>();

        if (lineRenderer == null)
        {
            Debug.LogWarning("Sniper line prefab has no LineRenderer.");
        }
    }

    private void UpdateIndicatorPosition()
    {
        PlayerController targetPlayer = players[targetIndex];
        if (targetPlayer == null)
            return;

        if (spawnedIndicator == null && targetIndicatorPrefab != null)
        {
            spawnedIndicator = Instantiate(targetIndicatorPrefab);
        }

        if (spawnedIndicator != null)
        {
            spawnedIndicator.transform.position = targetPlayer.transform.position + indicatorOffset;
        }
    }

    private void UpdateLine()
    {
        if (lineRenderer == null || context == null)
            return;

        PlayerController targetPlayer = players[targetIndex];
        if (targetPlayer == null)
            return;

        Vector3 start = GetLineStartPosition();
        Vector3 end = targetPlayer.transform.position + lineEndOffset;

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

        return context.userPlayer.transform.position + lineStartOffset;
    }

    private void UpdateCameraTarget()
    {
        PlayerController targetPlayer = players[targetIndex];
        if (targetPlayer == null || cameraFollow == null)
            return;

        cameraFollow.SetOverrideTarget(targetPlayer.transform);
    }

    private void ClearCameraOverride()
    {
        if (cameraFollow != null)
        {
            cameraFollow.ClearOverrideTarget();
        }
    }

    private void UpdatePlayerAim()
    {
        if (aimRotation == null)
            return;

        PlayerController targetPlayer = players[targetIndex];
        if (targetPlayer == null)
            return;

        aimRotation.SetAiming(targetPlayer.transform.position);
    }

    private void ClearPlayerAim()
    {
        if (aimRotation != null)
        {
            aimRotation.SetIdle();
        }
    }

    private void ClearVisuals()
    {
        if (spawnedIndicator != null)
        {
            Destroy(spawnedIndicator);
            spawnedIndicator = null;
        }

        if (spawnedLine != null)
        {
            Destroy(spawnedLine);
            spawnedLine = null;
            lineRenderer = null;
        }
    }
}