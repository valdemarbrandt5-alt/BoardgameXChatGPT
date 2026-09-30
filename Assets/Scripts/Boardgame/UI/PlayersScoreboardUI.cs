using UnityEngine;

public class PlayersScoreboardUI : MonoBehaviour
{
    public static PlayersScoreboardUI Instance;

    public PlayerScoreboardSlotUI[] slots;

    private PlayerStats activePlayer;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        RefreshAll();
    }

    public void SetPlayers(PlayerController[] players)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < players.Length)
            {
                PlayerStats stats = players[i].GetComponent<PlayerStats>();
                slots[i].gameObject.SetActive(true);
                slots[i].SetPlayer(stats);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetActivePlayer(PlayerStats player)
    {
        activePlayer = player;
        RefreshAll();
    }

    public void RefreshAll()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].gameObject.activeSelf)
            {
                bool isActive = slots[i].GetPlayer() == activePlayer;
                slots[i].Refresh(isActive);
            }
        }
    }
}