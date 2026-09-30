using UnityEngine;

public class TestMovement : MonoBehaviour
{
    public PlayerController player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(player.MoveSteps(3));
        }
    }
}