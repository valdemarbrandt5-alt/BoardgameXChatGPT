using UnityEngine;

public class MagnetEffectManager : MonoBehaviour
{
    public static MagnetEffectManager Instance;

    [Header("Coin Visuals")]
    public GameObject flyingCoinPrefab;
    public int minCoinVisuals = 3;
    public int maxCoinVisuals = 15;
    public int coinsPerVisual = 2;

    [Header("Spawn Offsets")]
    public Vector3 sourceOffset = new Vector3(0f, 1f, 0f);
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

    private void Awake()
    {
        Instance = this;
    }

    public void PlayMagnetEffect(
        Transform from,
        Transform to,
        int coinAmount,
        ItemDefinition stolenItem,
        float duration
    )
    {
        if (from == null || to == null)
            return;

        Vector3 start = from.position + sourceOffset;
        Vector3 end = to.position + targetOffset;

        Color sourceColor = Color.white;
        PlayerStats sourceStats = from.GetComponent<PlayerStats>();

        if (sourceStats != null)
        {
            sourceColor = sourceStats.playerColor;
        }

        SpawnCoinVisuals(start, end, coinAmount, duration, sourceColor, to);
        SpawnItemVisual(start, end, stolenItem, duration);
    }

    private void SpawnCoinVisuals(
        Vector3 start,
        Vector3 end,
        int coinAmount,
        float duration,
        Color auraColor,
        Transform target
    )
    {
        if (flyingCoinPrefab == null || coinAmount <= 0)
            return;

        int coinVisualCount = Mathf.Clamp(
            Mathf.CeilToInt((float)coinAmount / Mathf.Max(1, coinsPerVisual)),
            minCoinVisuals,
            maxCoinVisuals
        );

        for (int i = 0; i < coinVisualCount; i++)
        {
            Vector3 randomStartOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.35f),
                Random.Range(-0.5f, 0.5f)
            );

            GameObject coinObj = Instantiate(
                flyingCoinPrefab,
                start + randomStartOffset,
                Quaternion.identity
            );

            FlyingCoin flyingCoin = coinObj.GetComponent<FlyingCoin>();

            if (flyingCoin != null)
            {
                flyingCoin.Init(
                    start + randomStartOffset,
                    end,
                    duration,
                    auraColor,
                    target
                );
            }
        }
    }

    private void SpawnItemVisual(
        Vector3 start,
        Vector3 end,
        ItemDefinition stolenItem,
        float duration
    )
    {
        if (stolenItem == null || stolenItem.worldPrefab == null)
            return;

        Vector3 offset = new Vector3(
            Random.Range(-0.25f, 0.25f),
            Random.Range(0.1f, 0.4f),
            Random.Range(-0.25f, 0.25f)
        );

        GameObject itemObj = Instantiate(
            stolenItem.worldPrefab,
            start + offset,
            Quaternion.identity
        );

        FlyingItemObject fi = itemObj.GetComponent<FlyingItemObject>();

        if (fi == null)
        {
            fi = itemObj.AddComponent<FlyingItemObject>();
        }

        fi.Init(start + offset, end, duration);
    }
}