using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BoardGame/Items/Magnet Behaviour")]
public class MagnetItemBehaviour : BaseItemBehaviour
{
    [Header("Visuals")]
    public GameObject magnetVisualPrefab;
    public GameObject coneVisualPrefab;
    public GameObject targetIndicatorPrefab;

    [Header("Aim Settings")]
    public float aimRange = 3f;
    public float aimRotateSpeed = 120f;
    public float coneAngle = 45f;
    public float visualHeightOffset = 0.05f;

    [Header("Camera")]
    [Range(0f, 1f)] public float cameraFollowTowardAim = 0.3f;

    [Header("Steal Settings")]
    public int minStealPercent = 15;
    public int maxStealPercent = 40;

    [Header("Timing")]
    public float suctionDuration = 1.5f;
    public float postUseDelay = 0.2f;
    public float staggerBetweenTargets = 0.08f;

    private ItemUseContext context;
    private PlayerWeaponVisuals weaponVisuals;
    private PlayerAimRotation aimRotation;
    private CameraFollow cameraFollow;

    private GameObject spawnedConeVisual;
    private ConeRangeVisual coneRangeVisual;
    private readonly List<GameObject> spawnedIndicators = new List<GameObject>();

    private float currentAimAngle;
    private readonly List<PlayerController> currentTargets = new List<PlayerController>();
    private bool isUsing = false;

    private struct MagnetStealResult
    {
        public PlayerController targetPlayer;
        public int stolenCoins;
        public ItemDefinition stolenItem;
    }

    public override void BeginUse(ItemUseContext context)
    {
        this.context = context;
        weaponVisuals = context.userPlayer.GetComponent<PlayerWeaponVisuals>();
        aimRotation = context.userPlayer.GetComponent<PlayerAimRotation>();
        cameraFollow = context.turnManager.cameraFollow;
        isUsing = false;

        currentAimAngle = context.userPlayer.transform.eulerAngles.y;

        if (weaponVisuals != null && magnetVisualPrefab != null)
        {
            weaponVisuals.ShowWeapon(magnetVisualPrefab);
        }

        if (coneVisualPrefab != null)
        {
            spawnedConeVisual = Instantiate(coneVisualPrefab);
            coneRangeVisual = spawnedConeVisual.GetComponent<ConeRangeVisual>();

            if (coneRangeVisual != null)
            {
                coneRangeVisual.SetValues(aimRange, coneAngle);
            }
        }

        UpdateConeVisual();
        UpdateAimAndTargets();
        UpdateCameraAimFollow();
    }

    public override void HandleInput()
    {
        if (isUsing)
            return;

        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            currentAimAngle += horizontal * aimRotateSpeed * Time.deltaTime;

            UpdateConeVisual();
            UpdateAimAndTargets();
            UpdateCameraAimFollow();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentTargets.Count > 0)
            {
                context.turnManager.StartCoroutine(UseMagnetRoutine());
            }
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

    private void UpdateAimAndTargets()
    {
        Vector3 aimDirection = GetAimDirection();
        Vector3 aimPoint = context.userPlayer.transform.position + aimDirection * aimRange;

        if (aimRotation != null)
        {
            aimRotation.SetAiming(aimPoint);
        }

        FindTargetsInCone(aimDirection);
        UpdateTargetIndicators();
    }

    private Vector3 GetAimDirection()
    {
        Vector3 dir = Quaternion.Euler(0f, currentAimAngle, 0f) * Vector3.forward;
        dir.y = 0f;
        return dir.normalized;
    }

    private void FindTargetsInCone(Vector3 aimDirection)
    {
        currentTargets.Clear();

        PlayerController[] players = context.turnManager.players;
        if (players == null)
            return;

        Vector3 origin = context.userPlayer.transform.position;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player = players[i];
            if (player == null || player == context.userPlayer)
                continue;

            Vector3 toTarget = player.transform.position - origin;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (distance > aimRange || distance < 0.01f)
                continue;

            Vector3 dirToTarget = toTarget.normalized;
            float angle = Vector3.Angle(aimDirection, dirToTarget);

            if (angle <= coneAngle * 0.5f)
            {
                currentTargets.Add(player);
            }
        }
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
            GameObject indicator = Instantiate(targetIndicatorPrefab);
            indicator.transform.position =
                currentTargets[i].transform.position + Vector3.up * visualHeightOffset;

            spawnedIndicators.Add(indicator);
        }
    }

    private void UpdateConeVisual()
    {
        if (spawnedConeVisual == null)
            return;

        Vector3 origin = context.userPlayer.transform.position + Vector3.up * visualHeightOffset;
        spawnedConeVisual.transform.position = origin;
        spawnedConeVisual.transform.rotation = Quaternion.Euler(0f, currentAimAngle, 0f);

        if (coneRangeVisual != null)
        {
            coneRangeVisual.SetValues(aimRange, coneAngle);
        }
    }

    private void UpdateCameraAimFollow()
    {
        if (cameraFollow == null || context == null || context.userPlayer == null)
            return;

        Vector3 playerPos = context.userPlayer.transform.position;
        Vector3 aimDirection = GetAimDirection();
        Vector3 aimPos = playerPos + aimDirection * aimRange;

        Vector3 cameraFocusPoint = Vector3.Lerp(playerPos, aimPos, cameraFollowTowardAim);
        cameraFocusPoint.y = playerPos.y;

        cameraFollow.SetOverrideWorldPosition(cameraFocusPoint);
    }

    private IEnumerator UseMagnetRoutine()
    {
        if (isUsing)
            yield break;

        isUsing = true;

        if (currentTargets.Count == 0)
        {
            isUsing = false;
            yield break;
        }

        List<MagnetStealResult> results = new List<MagnetStealResult>();
        int totalStolenCoins = 0;

        PlayerInventory userInventory = context.userPlayer.GetComponent<PlayerInventory>();
        if (userInventory == null)
        {
            isUsing = false;
            yield break;
        }

        for (int i = 0; i < currentTargets.Count; i++)
        {
            PlayerController target = currentTargets[i];
            if (target == null)
                continue;

            PlayerStats targetStats = target.GetComponent<PlayerStats>();
            PlayerInventory targetInventory = target.GetComponent<PlayerInventory>();

            if (targetStats == null)
                continue;

            int percent = Random.Range(minStealPercent, maxStealPercent + 1);

            int stolenCoins = Mathf.FloorToInt(targetStats.coins * (percent / 100f));
            if (targetStats.coins > 0)
            {
                stolenCoins = Mathf.Max(1, stolenCoins);
            }

            stolenCoins = Mathf.Min(stolenCoins, targetStats.coins);

            ItemDefinition stolenItem = null;
            if (targetInventory != null)
            {
                stolenItem = targetInventory.RemoveRandomOwnedItem();
            }

            targetStats.coins -= stolenCoins;
            totalStolenCoins += stolenCoins;

            results.Add(new MagnetStealResult
            {
                targetPlayer = target,
                stolenCoins = stolenCoins,
                stolenItem = stolenItem
            });

            if (MagnetEffectManager.Instance != null)
            {
                MagnetEffectManager.Instance.PlayMagnetEffect(
                    target.transform,
                    context.userPlayer.transform,
                    stolenCoins,
                    stolenItem,
                    suctionDuration
                );
            }

            if (staggerBetweenTargets > 0f)
            {
                yield return new WaitForSeconds(staggerBetweenTargets);
            }
        }

        yield return new WaitForSeconds(suctionDuration);

        for (int i = 0; i < results.Count; i++)
        {
            MagnetStealResult result = results[i];

            

            if (result.stolenItem != null)
            {
                userInventory.AddItem(result.stolenItem);
            }
        }

        if (DamagePopupManager.Instance != null && totalStolenCoins > 0)
        {
            DamagePopupManager.Instance.ShowHealPopup(
                context.userPlayer.transform.position,
                totalStolenCoins
            );
        }

        yield return new WaitForSeconds(postUseDelay);

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

    private void CleanupVisuals()
    {
        if (spawnedConeVisual != null)
        {
            Destroy(spawnedConeVisual);
            spawnedConeVisual = null;
            coneRangeVisual = null;
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