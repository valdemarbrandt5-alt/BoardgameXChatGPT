using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPathMarker : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float heightOffset = 0.3f;

    public int startPathIndex;
    public int currentPathIndex;
    private PathVisualizer owner;

    public void StartMoving(List<BoardTile> path, PathVisualizer visualizer, int pathIndex)
    {
        owner = visualizer;
        startPathIndex = pathIndex;
        StartCoroutine(MoveAlongPath(path));
    }

    private IEnumerator MoveAlongPath(List<BoardTile> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            currentPathIndex = i;

            if (path[i] == null || path[i].standPoint == null)
                continue;

            Vector3 target = path[i].standPoint.position + Vector3.up * heightOffset;

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }
        }

        if (owner != null)
            owner.UnregisterMarker(gameObject);

        Destroy(gameObject);
    }
}