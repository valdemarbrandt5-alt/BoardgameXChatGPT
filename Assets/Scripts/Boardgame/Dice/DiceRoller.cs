using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public int minRoll = 1;
    public int maxRoll = 10;

    public int Roll()
    {
        int result = Random.Range(minRoll, maxRoll + 1);
        Debug.Log("Rolled: " + result);
        return result;
    }
}