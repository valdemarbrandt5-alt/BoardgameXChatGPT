using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{
    public GameObject markerPrefab;
    public float spawnDelay = 0.2f;
    public PlayerStats activePlayer;

    public int minTileGap = 5;

    private readonly List<GameObject> spawnedMarkers = new List<GameObject>();

    private BoardTile lastPlayerTile;
    private BoardTile lastTrophyTile;

    private List<BoardTile> currentPath = new List<BoardTile>();

    private void Start()
    {
        StartCoroutine(SpawnFlowLoop());
    }

    private void Update()
    {
        if (activePlayer == null || TrophyManager.Instance == null || BoardManager.Instance == null)
            return;

        BoardTile currentPlayerTile = activePlayer.currentTile;
        BoardTile currentTrophyTile = TrophyManager.Instance.currentTrophyTile;

        if (currentPlayerTile == null || currentTrophyTile == null)
            return;

        if (currentPlayerTile != lastPlayerTile || currentTrophyTile != lastTrophyTile)
        {
            lastPlayerTile = currentPlayerTile;
            lastTrophyTile = currentTrophyTile;

            currentPath = BoardManager.Instance.FindPath(currentPlayerTile, currentTrophyTile);
        }
    }

    public void SetActivePlayer(PlayerStats player)
    {
        activePlayer = player;
        lastPlayerTile = null;
        lastTrophyTile = null;

        ClearMarkers();

        if (activePlayer != null && TrophyManager.Instance != null && BoardManager.Instance != null)
        {
            if (activePlayer.currentTile != null && TrophyManager.Instance.currentTrophyTile != null)
            {
                currentPath = BoardManager.Instance.FindPath(
                    activePlayer.currentTile,
                    TrophyManager.Instance.currentTrophyTile
                );
            }
        }
    }

    private IEnumerator SpawnFlowLoop()
    {
        while (true)
        {
            if (currentPath != null && currentPath.Count > 0)
            {
                if (!HasMarkerTooCloseAhead(minTileGap))
                {
                    GameObject marker = Instantiate(
                        markerPrefab,
                        currentPath[0].standPoint.position + Vector3.up * 0.3f,
                        Quaternion.identity
                    );

                    spawnedMarkers.Add(marker);

                    MovingPathMarker mover = marker.GetComponent<MovingPathMarker>();
                    if (mover != null)
                    {
                        mover.StartMoving(new List<BoardTile>(currentPath), this, 0);
                    }
                }
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private bool HasMarkerTooCloseAhead(int tileGap)
    {
        for (int i = spawnedMarkers.Count - 1; i >= 0; i--)
        {
            if (spawnedMarkers[i] == null)
            {
                spawnedMarkers.RemoveAt(i);
                continue;
            }

            MovingPathMarker marker = spawnedMarkers[i].GetComponent<MovingPathMarker>();
            if (marker == null)
                continue;

            // 🔥 nu checker vi hvor markeren ER på pathen
            if (marker.currentPathIndex < tileGap)
                return true;
        }

        return false;
    }

    public void UnregisterMarker(GameObject marker)
    {
        if (spawnedMarkers.Contains(marker))
            spawnedMarkers.Remove(marker);
    }

    private void ClearMarkers()
    {
        for (int i = 0; i < spawnedMarkers.Count; i++)
        {
            if (spawnedMarkers[i] != null)
                Destroy(spawnedMarkers[i]);
        }

        spawnedMarkers.Clear();
    }
}