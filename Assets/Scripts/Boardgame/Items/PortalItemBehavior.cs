using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "BoardGame/Items/Portal Behaviour")]
public class PortalItemBehaviour : BaseItemBehaviour
{
    [Header("Target Indicator")]
    public GameObject targetIndicatorPrefab;
    public Vector3 indicatorOffset = new Vector3(0f, 0.05f, 0f);

    [Header("Timing")]
    public float suckUpDuration = 0.35f;
    public float swapDelay = 0.1f;
    public float dropDownDuration = 0.3f;
    public float cameraHoldAfterSwap = 1f;

    [Header("Suck Up Visual")]
    public float suckUpHeight = 2.5f;
    public AnimationCurve suckUpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve suckUpScaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.15f);

    [Header("Drop Down Visual")]
    public float dropStartHeight = 2.5f;
    public AnimationCurve dropCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve dropScaleCurve = AnimationCurve.EaseInOut(0f, 0.15f, 1f, 1f);

    [Header("Effects")]
    public GameObject portalEffectPrefab;
    public Vector3 effectOffset = new Vector3(0f, 0.1f, 0f);

    private ItemUseContext context;
    private PlayerController[] players;
    private int targetIndex = 0;

    private GameObject spawnedIndicator;
    private bool isSwapping = false;

    private CameraFollow cameraFollow;
    private PlayerAimRotation aimRotation;

    public override void BeginUse(ItemUseContext context)
    {
        this.context = context;
        players = context.turnManager.players;
        isSwapping = false;

        cameraFollow = context.turnManager.cameraFollow;
        aimRotation = context.userPlayer.GetComponent<PlayerAimRotation>();

        targetIndex = FindFirstValidTargetIndex();

        if (targetIndex == -1)
        {
            Debug.Log("No valid portal targets.");
            ItemUseManager.Instance.CancelActiveItemUse();
            return;
        }

        UpdateIndicatorPosition();
        UpdateCameraTarget();
        UpdatePlayerAim();
    }

    public override void HandleInput()
    {
        if (isSwapping)
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
                context.turnManager.StartCoroutine(SwapRoutine());
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUse();
            ItemUseManager.Instance.CancelActiveItemUse();
            return;
        }

        UpdateIndicatorPosition();
        UpdateCameraTarget();
        UpdatePlayerAim();
    }

    public override void CancelUse()
    {
        ClearVisuals();
        ClearCameraOverride();
        ClearPlayerAim();
        isSwapping = false;
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
        UpdateCameraTarget();
        UpdatePlayerAim();
    }

    private IEnumerator SwapRoutine()
    {
        if (isSwapping)
            yield break;

        isSwapping = true;

        PlayerController userPlayer = context.userPlayer;
        PlayerController targetPlayer = players[targetIndex];

        if (userPlayer == null || targetPlayer == null || targetPlayer == userPlayer)
        {
            isSwapping = false;
            yield break;
        }

        PlayerStats userStats = userPlayer.GetComponent<PlayerStats>();
        PlayerStats targetStats = targetPlayer.GetComponent<PlayerStats>();

        if (userStats == null || targetStats == null)
        {
            isSwapping = false;
            yield break;
        }

        BoardTile userTile = userStats.currentTile;
        BoardTile targetTile = targetStats.currentTile;

        if (userTile == null || targetTile == null)
        {
            Debug.LogWarning("Portal swap failed because one or both players have no currentTile.");
            isSwapping = false;
            yield break;
        }

        Vector3 userOriginalScale = userPlayer.transform.localScale;
        Vector3 targetOriginalScale = targetPlayer.transform.localScale;

        Vector3 userStartPos = userPlayer.transform.position;
        Vector3 targetStartPos = targetPlayer.transform.position;

        Vector3 userGroundPosAfterSwap = GetStandPosition(userPlayer, targetTile, targetStartPos);
        Vector3 targetGroundPosAfterSwap = GetStandPosition(targetPlayer, userTile, userStartPos);

        SpawnPortalEffect(userStartPos);
        SpawnPortalEffect(targetStartPos);

        if (cameraFollow != null)
        {
            cameraFollow.SetOverrideTarget(targetPlayer.transform);
        }

        yield return AnimateSuckUp(userPlayer.transform, userOriginalScale);
        yield return AnimateSuckUp(targetPlayer.transform, targetOriginalScale);

        yield return new WaitForSeconds(swapDelay);

        userStats.currentTile = targetTile;
        targetStats.currentTile = userTile;

        Vector3 userDropStartPos = userGroundPosAfterSwap + Vector3.up * dropStartHeight;
        Vector3 targetDropStartPos = targetGroundPosAfterSwap + Vector3.up * dropStartHeight;

        userPlayer.transform.position = userDropStartPos;
        targetPlayer.transform.position = targetDropStartPos;

        userPlayer.transform.localScale = userOriginalScale * 0.15f;
        targetPlayer.transform.localScale = targetOriginalScale * 0.15f;

        SpawnPortalEffect(userPlayer.transform.position);
        SpawnPortalEffect(targetPlayer.transform.position);

        if (cameraFollow != null)
        {
            cameraFollow.SetOverrideTarget(targetPlayer.transform);
        }

        if (spawnedIndicator != null)
        {
            spawnedIndicator.transform.position = targetPlayer.transform.position + indicatorOffset;
        }

        yield return AnimateDropDown(userPlayer.transform, userGroundPosAfterSwap, userOriginalScale);
        yield return AnimateDropDown(targetPlayer.transform, targetGroundPosAfterSwap, targetOriginalScale);

        Debug.Log(userPlayer.name + " swapped position with " + targetPlayer.name + " using Portal.");

        yield return new WaitForSeconds(cameraHoldAfterSwap);

        ClearVisuals();
        ClearCameraOverride();
        ClearPlayerAim();

        ItemUseManager.Instance.FinishActiveItemUse(true);
    }

    private IEnumerator AnimateSuckUp(Transform targetTransform, Vector3 originalScale)
    {
        if (targetTransform == null)
            yield break;

        Vector3 startPos = targetTransform.position;
        Vector3 endPos = startPos + Vector3.up * suckUpHeight;

        float elapsed = 0f;

        while (elapsed < suckUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / suckUpDuration);

            float posT = suckUpCurve.Evaluate(t);
            float scaleT = suckUpScaleCurve.Evaluate(t);

            targetTransform.position = Vector3.Lerp(startPos, endPos, posT);
            targetTransform.localScale = originalScale * scaleT;

            yield return null;
        }
    }

    private IEnumerator AnimateDropDown(Transform targetTransform, Vector3 groundPosition, Vector3 originalScale)
    {
        if (targetTransform == null)
            yield break;

        Vector3 startPos = targetTransform.position;
        Vector3 endPos = groundPosition;

        float elapsed = 0f;

        while (elapsed < dropDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDownDuration);

            float posT = dropCurve.Evaluate(t);
            float scaleT = dropScaleCurve.Evaluate(t);

            targetTransform.position = Vector3.Lerp(startPos, endPos, posT);
            targetTransform.localScale = originalScale * scaleT;

            yield return null;
        }

        targetTransform.position = endPos;
        targetTransform.localScale = originalScale;
    }

    private Vector3 GetStandPosition(PlayerController player, BoardTile tile, Vector3 fallbackPosition)
    {
        if (player == null || tile == null || tile.standPoint == null)
            return fallbackPosition;

        return tile.standPoint.position + Vector3.up * player.heightOffset;
    }

    private void SpawnPortalEffect(Vector3 position)
    {
        if (portalEffectPrefab == null)
            return;

        Instantiate(portalEffectPrefab, position + effectOffset, Quaternion.identity);
    }

    private void UpdateIndicatorPosition()
    {
        if (players == null || targetIndex < 0 || targetIndex >= players.Length)
            return;

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

    private void UpdateCameraTarget()
    {
        if (players == null || targetIndex < 0 || targetIndex >= players.Length)
            return;

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

        if (players == null || targetIndex < 0 || targetIndex >= players.Length)
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
    }
}