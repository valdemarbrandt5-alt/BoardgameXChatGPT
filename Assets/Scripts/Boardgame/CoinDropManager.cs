using UnityEngine;

public class CoinDropManager : MonoBehaviour
{
    public static CoinDropManager Instance;

    public GameObject coinPrefab;

    public int maxVisualCoins = 20;
    public float spreadRadius = 1.8f;
    public float spawnHeightOffset = 0.6f;

    private void Awake()
    {
        Instance = this;
    }

    public void DropCoins(int amount, PlayerStats owner, Vector3 position)
    {
        if (amount <= 0) return;

        int visualCount = Mathf.Min(maxVisualCoins, amount);
        int coinsPerPickup = Mathf.CeilToInt((float)amount / visualCount);

        int remaining = amount;

        for (int i = 0; i < visualCount; i++)
        {
            int value = Mathf.Min(coinsPerPickup, remaining);
            remaining -= value;

            SpawnCoin(value, owner, position, i, visualCount);
        }
    }

    private void SpawnCoin(int value, PlayerStats owner, Vector3 center, int index, int total)
    {
        Vector3 spawnPos = center + Vector3.up * spawnHeightOffset;

        float angle = (360f / total) * index + Random.Range(-10f, 10f);
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

        float distance = Random.Range(spreadRadius * 0.6f, spreadRadius);

        // vigtigt: target skal være på gulvplan, ikke i spawnhøjde
        Vector3 targetPos = new Vector3(
            center.x + dir.x * distance,
            center.y,
            center.z + dir.z * distance
        );

        GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

        DroppedCoin droppedCoin = coin.GetComponent<DroppedCoin>();
        droppedCoin.Setup(value, owner, targetPos);
    }
}