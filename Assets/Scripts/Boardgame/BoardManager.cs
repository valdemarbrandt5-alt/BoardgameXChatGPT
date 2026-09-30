using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    public BoardTile SimulateMove(BoardTile start, int steps, BoardTile firstChoice)
    {
        BoardTile current = firstChoice;

        for (int i = 1; i < steps; i++)
        {
            if (current == null)
                return null;

            if (current.nextTile != null)
            {
                current = current.nextTile;
            }
            else
            {
                break;
            }
        }

        return current;
    }
    public List<BoardTile> FindPath(BoardTile start, BoardTile target)
    {
        Queue<BoardTile> queue = new Queue<BoardTile>();
        Dictionary<BoardTile, BoardTile> cameFrom = new Dictionary<BoardTile, BoardTile>();

        queue.Enqueue(start);
        cameFrom[start] = null;

        while (queue.Count > 0)
        {
            BoardTile current = queue.Dequeue();

            if (current == target)
                break;

            // normal path
            if (current.nextTile != null && !cameFrom.ContainsKey(current.nextTile))
            {
                queue.Enqueue(current.nextTile);
                cameFrom[current.nextTile] = current;
            }

            // branch path
            if (current.branchTile != null && !cameFrom.ContainsKey(current.branchTile))
            {
                queue.Enqueue(current.branchTile);
                cameFrom[current.branchTile] = current;
            }
        }

        // reconstruct path
        List<BoardTile> path = new List<BoardTile>();
        BoardTile temp = target;

        while (temp != null)
        {
            path.Add(temp);
            temp = cameFrom.ContainsKey(temp) ? cameFrom[temp] : null;
        }

        path.Reverse();
        return path;
    }
}