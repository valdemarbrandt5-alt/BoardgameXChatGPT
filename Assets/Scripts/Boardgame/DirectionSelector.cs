using System.Collections;
using UnityEngine;

public class DirectionSelector : MonoBehaviour
{
    public GameObject arrowLeft;
    public GameObject arrowRight;

    public float heightOffset = 2f;

    public Color baseColor = Color.red;
    public Color emissionColor = Color.red;
    public float selectedEmissionIntensity = 5f;
    public float unselectedEmissionIntensity = 0f;

    private BoardTile selectedTile;
    private BoardTile normalTile;
    private BoardTile branchTile;

    private BoardTile currentTile;
    private int currentStepsRemaining;

    private Renderer leftRenderer;
    private Renderer rightRenderer;

    private Material leftMaterial;
    private Material rightMaterial;

    private BoardManager board;

    private void Awake()
    {
        board = BoardManager.Instance;

        leftRenderer = arrowLeft.GetComponentInChildren<Renderer>();
        rightRenderer = arrowRight.GetComponentInChildren<Renderer>();

        leftMaterial = leftRenderer.material;
        rightMaterial = rightRenderer.material;

        leftMaterial.EnableKeyword("_EMISSION");
        rightMaterial.EnableKeyword("_EMISSION");

        arrowLeft.SetActive(false);
        arrowRight.SetActive(false);
    }

    public IEnumerator Choose(BoardTile tile, int stepsRemaining, System.Action<BoardTile> onChosen)
    {
        if (tile == null || tile.nextTile == null || tile.branchTile == null)
            yield break;

        currentTile = tile;
        currentStepsRemaining = stepsRemaining;

        normalTile = tile.nextTile;
        branchTile = tile.branchTile;

        selectedTile = normalTile;

        PositionArrows(tile);

        arrowLeft.SetActive(true);
        arrowRight.SetActive(true);

        UpdateVisuals();

        Debug.Log("Choose direction | Steps remaining: " + stepsRemaining);

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                selectedTile = normalTile;
                UpdateVisuals();
                Debug.Log("NORMAL path | Steps left: " + stepsRemaining);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                selectedTile = branchTile;
                UpdateVisuals();
                Debug.Log("BRANCH path | Steps left: " + stepsRemaining);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                arrowLeft.SetActive(false);
                arrowRight.SetActive(false);

                LandingPreview.Instance.Clear();

                onChosen?.Invoke(selectedTile);
                yield break;
            }

            yield return null;
        }
    }

    private void PositionArrows(BoardTile tile)
    {
        if (tile.nextTile == null) return;

        // NORMAL (venstre)
        Vector3 nextPos = tile.nextTile.standPoint.position + Vector3.up * heightOffset;
        Vector3 nextDir = (tile.nextTile.standPoint.position - tile.standPoint.position).normalized;

        arrowLeft.transform.position = nextPos;
        arrowLeft.transform.rotation = Quaternion.LookRotation(nextDir);

        // BRANCH (højre)
        if (tile.branchTile != null)
        {
            Vector3 branchPos = tile.branchTile.standPoint.position + Vector3.up * heightOffset;
            Vector3 branchDir = (tile.branchTile.standPoint.position - tile.standPoint.position).normalized;

            arrowRight.transform.position = branchPos;
            arrowRight.transform.rotation = Quaternion.LookRotation(branchDir);
        }
    }

    private void UpdateVisuals()
    {
        SetArrowVisual(leftMaterial, selectedTile == normalTile);
        SetArrowVisual(rightMaterial, selectedTile == branchTile);

        // 🔥 Landing preview
        if (LandingPreview.Instance != null && board != null)
        {
            BoardTile landing = board.SimulateMove(
                currentTile,
                currentStepsRemaining,
                selectedTile
            );

            LandingPreview.Instance.Show(landing);
        }
    }

    private void SetArrowVisual(Material mat, bool isSelected)
    {
        mat.color = baseColor;

        float intensity = isSelected ? selectedEmissionIntensity : unselectedEmissionIntensity;
        mat.SetColor("_EmissionColor", emissionColor * intensity);
    }
}