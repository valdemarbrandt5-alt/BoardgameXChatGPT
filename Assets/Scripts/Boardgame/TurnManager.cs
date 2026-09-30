using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public PlayerController[] players;
    public CameraFollow cameraFollow;
    public PathVisualizer pathVisualizer;

    public DiceVisualController diceVisualPrefab;
    public DiceRollPopup diceRollPopup;

    private DiceVisualController currentDiceVisual;

    private int currentPlayerIndex = 0;
    private bool isBusy = false;
    private bool waitingForRollInput = false;
    private bool turnActionConsumed = false;

    public float diceToMoveDelay = 1f;

    public PlayersScoreboardUI playersScoreboardUI;
    private bool usedItemThisTurn = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (playersScoreboardUI != null)
        {
            playersScoreboardUI.SetPlayers(players);
        }

        if (ControlsUI.Instance != null)
        {
            ControlsUI.Instance.ShowNormalControls();
        }

        SetActivePlayer(0);
    }

    private void Update()
    {
        if (isBusy)
            return;

        if (cameraFollow != null && cameraFollow.IsMapViewOpen())
            return;

        if (waitingForRollInput && !turnActionConsumed && Input.GetKeyDown(KeyCode.Space) &&(ItemUseManager.Instance == null || !ItemUseManager.Instance.IsBusyWithItems))
        {
            StartCoroutine(TakeTurn());
            return;
        }

        if (
    waitingForRollInput &&
    !turnActionConsumed &&
    Input.GetKeyDown(KeyCode.Q) &&
    (ItemUseManager.Instance == null || !ItemUseManager.Instance.IsBusyWithItems)
)
        {
            if (ItemUseManager.Instance != null)
            {
                ItemUseManager.Instance.OpenItemMenu(this);
            }
        }
    }

    public PlayerController GetCurrentPlayer()
    {
        if (players == null || players.Length == 0)
            return null;

        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Length)
            return null;

        return players[currentPlayerIndex];
    }

    private IEnumerator TakeTurn()
    {
        isBusy = true;
        waitingForRollInput = false;

        if (ControlsUI.Instance != null)
        {
            ControlsUI.Instance.HideAll();
        }

        PlayerController player = players[currentPlayerIndex];

        int roll = Random.Range(1, 11);
        Debug.Log(player.name + " rolled: " + roll);

        if (currentDiceVisual != null)
        {
            currentDiceVisual.HideAndDestroy();
            currentDiceVisual = null;
        }

        if (diceRollPopup != null)
            diceRollPopup.Show(player.transform, roll);

        yield return new WaitForSeconds(diceToMoveDelay);

        yield return player.StartCoroutine(player.MoveSteps(roll));

        yield return new WaitForSeconds(1f);

        NextPlayer();

        isBusy = false;
    }

    private void NextPlayer()
    {
        currentPlayerIndex++;

        if (currentPlayerIndex >= players.Length)
            currentPlayerIndex = 0;

        SetActivePlayer(currentPlayerIndex);
    }

    public bool CanOpenMap()
    {
        if (ItemUseManager.Instance != null && ItemUseManager.Instance.IsBusyWithItems)
            return false;

        return waitingForRollInput && !isBusy && !turnActionConsumed;
    }

    public bool CanUseItemsNow()
    {
        return waitingForRollInput && !isBusy && !turnActionConsumed && !usedItemThisTurn;
    }
    public void MarkItemUsedThisTurn()
    {
        usedItemThisTurn = true;
    }

    public bool HasUsedItemThisTurn()
    {
        return usedItemThisTurn;
    }

    public bool CanShowDiceAgain()
    {
        return waitingForRollInput && !isBusy && !turnActionConsumed;
    }

    public void ConsumeTurnAction()
    {
        turnActionConsumed = true;

        waitingForRollInput = false;

        HideDiceVisual();

        if (ControlsUI.Instance != null)
        {
            ControlsUI.Instance.HideAll();
        }

        Debug.Log("Turn action consumed. Player cannot roll.");
        StartCoroutine(EndTurnAfterConsumedAction());
    }

    private IEnumerator EndTurnAfterConsumedAction()
    {
        yield return new WaitForSeconds(1f);
        NextPlayer();
    }

    public void HideDiceVisual()
    {
        if (currentDiceVisual != null)
        {
            currentDiceVisual.HideAndDestroy();
            currentDiceVisual = null;
        }
    }

    public void ShowDiceVisual()
    {
        if (!waitingForRollInput)
            return;

        PlayerController player = GetCurrentPlayer();

        if (player == null)
            return;

        SpawnDiceVisual(player.transform);
    }

    private void SetActivePlayer(int index)
    {
        PlayerController player = players[index];
        PlayerStats stats = player.GetComponent<PlayerStats>();

        turnActionConsumed = false;
        usedItemThisTurn = false;

        Debug.Log("Now playing: " + player.name);

        if (cameraFollow != null)
            cameraFollow.SetTarget(player.transform);

        if (pathVisualizer != null)
            pathVisualizer.SetActivePlayer(stats);

        if (playersScoreboardUI != null)
            playersScoreboardUI.SetActivePlayer(stats);

        waitingForRollInput = true;

        SpawnDiceVisual(player.transform);
        PlayerAimRotation aim = player.GetComponent<PlayerAimRotation>();
        if (aim != null)
        {
            aim.SetIdle();
        }
        if (ControlsUI.Instance != null)
        {
            ControlsUI.Instance.ShowNormalControls();
        }
    }

    private void SpawnDiceVisual(Transform target)
    {
        if (diceVisualPrefab == null)
            return;

        if (currentDiceVisual != null)
        {
            Destroy(currentDiceVisual.gameObject);
            currentDiceVisual = null;
        }

        currentDiceVisual = Instantiate(diceVisualPrefab);
        currentDiceVisual.Show(target);
    }
}