using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float heightOffset = 1f;

    private PlayerStats stats;
    private PlayerAimRotation aimRotation;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        aimRotation = GetComponent<PlayerAimRotation>();
    }

    public IEnumerator MoveSteps(int steps)
    {
        IsMoving = true;

        int stepsRemaining = steps;
        BoardTile lastTileReached = stats.currentTile;

        for (int i = 0; i < steps; i++)
        {
            stepsRemaining = steps - i;

            BoardTile current = stats.currentTile;

            if (current == null)
            {
                IsMoving = false;

                if (aimRotation != null)
                    aimRotation.SetIdle();

                yield break;
            }

            BoardTile next = current.nextTile;

            if (current.HasBranch())
            {
                yield return StartCoroutine(ChooseDirection(current, stepsRemaining, result =>
                {
                    next = result;
                }));
            }

            if (next == null)
            {
                IsMoving = false;

                if (aimRotation != null)
                    aimRotation.SetIdle();

                yield break;
            }

            yield return StartCoroutine(MoveToTile(next));

            stats.currentTile = next;
            lastTileReached = next;

            yield return StartCoroutine(CheckTrophy(next));
        }

        if (lastTileReached != null && TileResolver.Instance != null)
        {
            TileResolver.Instance.ResolveTile(stats, lastTileReached);
        }

        IsMoving = false;

        if (aimRotation != null)
            aimRotation.SetIdle();
    }

    private IEnumerator CheckTrophy(BoardTile tile)
    {
        if (TrophyManager.Instance == null)
            yield break;

        if (tile == TrophyManager.Instance.currentTrophyTile)
        {
            TrophyPromptUI.Instance.Show();

            while (!TrophyPromptUI.Instance.HasDecision())
            {
                yield return null;
            }

            bool buy = TrophyPromptUI.Instance.GetResult();

            TrophyPromptUI.Instance.Hide();

            if (buy)
            {
                TrophyManager.Instance.TryBuyTrophy(stats);
            }
            else
            {
                Debug.Log("Skipped trophy");
            }
        }
    }

    private IEnumerator MoveToTile(BoardTile tile)
    {
        if (tile == null || tile.standPoint == null)
            yield break;

        Vector3 target = tile.standPoint.position + Vector3.up * heightOffset;

        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (aimRotation != null && direction.sqrMagnitude > 0.001f)
        {
            aimRotation.SetMoving(direction.normalized);
        }

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
    }
    public IEnumerator MoveAlongPath(List<BoardTile> path, int maxSteps, bool resolveTileAtEnd = true, bool allowTrophyCheck = true)
    {
        if (path == null || path.Count <= 1)
            yield break;

        IsMoving = true;

        int stepsTaken = 0;
        BoardTile lastTileReached = stats.currentTile;

        for (int i = 1; i < path.Count && stepsTaken < maxSteps; i++)
        {
            BoardTile next = path[i];
            if (next == null)
                break;

            yield return StartCoroutine(MoveToTile(next));

            stats.currentTile = next;
            lastTileReached = next;
            stepsTaken++;

            if (allowTrophyCheck)
            {
                yield return StartCoroutine(CheckTrophy(next));
            }
        }

        if (resolveTileAtEnd && lastTileReached != null && TileResolver.Instance != null)
        {
            TileResolver.Instance.ResolveTile(stats, lastTileReached);
        }

        IsMoving = false;

        if (aimRotation != null)
            aimRotation.SetIdle();
    }
    private IEnumerator ChooseDirection(BoardTile tile, int stepsRemaining, System.Action<BoardTile> onChosen)
    {
        Debug.Log("Steps remaining: " + stepsRemaining);

        DirectionSelector selector = FindObjectOfType<DirectionSelector>();

        if (selector == null)
        {
            Debug.LogWarning("No DirectionSelector found in scene. Falling back to normal path.");
            onChosen?.Invoke(tile.nextTile);
            yield break;
        }

        yield return StartCoroutine(selector.Choose(tile, stepsRemaining, onChosen));
    }
}