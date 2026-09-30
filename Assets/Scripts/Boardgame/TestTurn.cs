using UnityEngine;

public class TestTurn : MonoBehaviour
{
    public PlayerController player;
    public DiceRoller dice;

    private bool isMoving = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isMoving)
        {
            StartTurn();
        }
    }

    void StartTurn()
    {
        int roll = dice.Roll();
        StartCoroutine(MovePlayer(roll));
    }

    System.Collections.IEnumerator MovePlayer(int steps)
    {
        isMoving = true;

        yield return player.StartCoroutine(player.MoveSteps(steps));

        isMoving = false;
    }
}