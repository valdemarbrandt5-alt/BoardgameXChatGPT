using UnityEngine;

public class BoardAutoLinker : MonoBehaviour
{
    [ContextMenu("Auto Link Tiles In Order")]
    public void AutoLinkTilesInOrder()
    {
        BoardTile[] tiles = GetComponentsInChildren<BoardTile>();

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].nextTile = null;

            if (i < tiles.Length - 1)
            {
                tiles[i].nextTile = tiles[i + 1];
            }
        }

        Debug.Log("Tiles auto-linked in hierarchy order.");
    }
}