using UnityEngine;
using System.Collections.Generic;

public class TrophyManager : MonoBehaviour
{
    public static TrophyManager Instance;

    public List<BoardTile> possibleTiles;
    public BoardTile currentTrophyTile;

    public GameObject trophyVisual;

    public int trophyCost = 40;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MoveTrophy();
    }

    public void MoveTrophy()
    {
        if (possibleTiles.Count == 0) return;

        BoardTile newTile;

        do
        {
            newTile = possibleTiles[Random.Range(0, possibleTiles.Count)];
        }
        while (newTile == currentTrophyTile);

        currentTrophyTile = newTile;

        // flyt visual
        if (trophyVisual != null && newTile.standPoint != null)
        {
            trophyVisual.transform.position =
                newTile.standPoint.position + Vector3.up * 1f;
        }

        Debug.Log("New trophy at: " + newTile.name);
    }

    public void TryBuyTrophy(PlayerStats player)
    {
        if (player.coins < trophyCost)
        {
            Debug.Log("Not enough coins");
            return;
        }

        player.coins -= trophyCost;
        player.trophies++;

        Debug.Log(player.name + " bought a trophy!");

        MoveTrophy();

        CheckWin(player);
    }

    private void CheckWin(PlayerStats player)
    {
        if (player.trophies >= 5)
        {
            Debug.Log(player.name + " WINS THE GAME!");
        }
    }
}