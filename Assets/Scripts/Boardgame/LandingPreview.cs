using UnityEngine;

public class LandingPreview : MonoBehaviour
{
    public static LandingPreview Instance;

    public GameObject markerPrefab;

    private GameObject currentMarker;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(BoardTile tile)
    {
        Clear();

        if (tile == null || tile.standPoint == null)
            return;

        currentMarker = Instantiate(markerPrefab);
        currentMarker.transform.position =
            tile.standPoint.position + Vector3.up * 0.5f;
    }

    public void Clear()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
        }
    }
}