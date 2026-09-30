using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BoardGame/Items/Rocket Behaviour")]
public class RocketItemBehaviour : BaseItemBehaviour
{
    [Header("Visuals")]
    public GameObject rocketEffectPrefab;
    public GameObject rocketTrailPrefab;
    public Vector3 effectOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Settings")]
    public int maxMoveSteps = 10;
    public float startDelay = 0.2f;
    public float postMoveDelay = 0.35f;

    private ItemUseContext context;
    private CameraFollow cameraFollow;
    private PlayerAimRotation aimRotation;

    private bool isUsing = false;
    private GameObject spawnedTrail;

    public override void BeginUse(ItemUseContext context)
    {
        this.context = context;
        cameraFollow = context.turnManager.cameraFollow;
        aimRotation = context.userPlayer.GetComponent<PlayerAimRotation>();
        isUsing = false;

        if (aimRotation != null)
        {
            BoardTile trophyTile = TrophyManager.Instance != null ? TrophyManager.Instance.currentTrophyTile : null;

            if (trophyTile != null && trophyTile.standPoint != null)
            {
                aimRotation.SetAiming(trophyTile.standPoint.position);
            }
        }
    }

    public override void HandleInput()
    {
        if (isUsing)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            context.turnManager.StartCoroutine(UseRocketRoutine());
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

        if (aimRotation != null)
        {
            aimRotation.SetIdle();
        }

        if (cameraFollow != null)
        {
            cameraFollow.ClearOverrideTarget();
            cameraFollow.ClearOverrideWorldPosition();
        }
    }

    private IEnumerator UseRocketRoutine()
    {
        if (isUsing)
            yield break;

        isUsing = true;

        PlayerController userPlayer = context.userPlayer;
        PlayerStats userStats = context.userStats;

        if (userPlayer == null || userStats == null)
        {
            ItemUseManager.Instance.CancelActiveItemUse();
            yield break;
        }

        if (userStats.currentTile == null)
        {
            Debug.LogWarning("Rocket failed: player has no currentTile.");
            ItemUseManager.Instance.CancelActiveItemUse();
            yield break;
        }

        if (TrophyManager.Instance == null || TrophyManager.Instance.currentTrophyTile == null)
        {
            Debug.LogWarning("Rocket failed: no trophy tile found.");
            ItemUseManager.Instance.CancelActiveItemUse();
            yield break;
        }

        if (BoardManager.Instance == null)
        {
            Debug.LogWarning("Rocket failed: no BoardManager found.");
            ItemUseManager.Instance.CancelActiveItemUse();
            yield break;
        }

        BoardTile startTile = userStats.currentTile;
        BoardTile trophyTile = TrophyManager.Instance.currentTrophyTile;

        List<BoardTile> path = BoardManager.Instance.FindPath(startTile, trophyTile);

        if (path == null || path.Count <= 1)
        {
            Debug.Log("Rocket found no useful path.");
            ItemUseManager.Instance.CancelActiveItemUse();
            yield break;
        }

        SpawnStartEffects();

        if (cameraFollow != null)
        {
            cameraFollow.SetOverrideTarget(userPlayer.transform);
        }

        yield return new WaitForSeconds(startDelay);

        yield return userPlayer.StartCoroutine(
            userPlayer.MoveAlongPath(
                path,
                maxMoveSteps,
                true,
                true
            )
        );

        yield return new WaitForSeconds(postMoveDelay);

        CleanupVisuals();

        if (aimRotation != null)
        {
            aimRotation.SetIdle();
        }

        if (cameraFollow != null)
        {
            cameraFollow.ClearOverrideTarget();
            cameraFollow.ClearOverrideWorldPosition();
        }

        ItemUseManager.Instance.FinishActiveItemUse(false);
    }

    private void SpawnStartEffects()
    {
        if (context == null || context.userPlayer == null)
            return;

        Vector3 pos = context.userPlayer.transform.position + effectOffset;

        if (rocketEffectPrefab != null)
        {
            GameObject.Instantiate(rocketEffectPrefab, pos, Quaternion.identity);
        }

        if (rocketTrailPrefab != null)
        {
            spawnedTrail = GameObject.Instantiate(rocketTrailPrefab, context.userPlayer.transform);
            spawnedTrail.transform.localPosition = Vector3.zero;
            spawnedTrail.transform.localRotation = Quaternion.identity;
        }
    }

    private void CleanupVisuals()
    {
        if (spawnedTrail != null)
        {
            GameObject.Destroy(spawnedTrail);
            spawnedTrail = null;
        }
    }
}