using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrophyPromptUI : MonoBehaviour
{
    public static TrophyPromptUI Instance;

    public GameObject panel;

    public TMP_Text buyText;
    public TMP_Text skipText;
    public Canvas canvas;
    public Image buyOutline;
    public Image skipOutline;

    public Color normalOutlineColor = Color.black;
    public Color selectedOutlineColor = Color.yellow;

    private bool isShowing = false;
    private int selectedIndex = 0; // 0 = Buy, 1 = Skip

    private bool decisionMade = false;
    private bool buyResult = false;

    private void Awake()
    {
        Instance = this;

        if (panel != null)
            panel.SetActive(false);
            canvas.enabled = true;
    }

    public void Show()
    {
        panel.SetActive(true);
        isShowing = true;

        selectedIndex = 0;
        decisionMade = false;

        UpdateVisuals();
    }

    public void Hide()
    {
        panel.SetActive(false);
        isShowing = false;
    }

    private void Update()
    {
        if (!isShowing)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex = 0;
            UpdateVisuals();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = 1;
            UpdateVisuals();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            decisionMade = true;
            buyResult = (selectedIndex == 0);
        }
    }

    public bool HasDecision()
    {
        return decisionMade;
    }

    public bool GetResult()
    {
        return buyResult;
    }

    private void UpdateVisuals()
    {
        if (buyOutline != null)
            buyOutline.color = selectedIndex == 0 ? selectedOutlineColor : normalOutlineColor;

        if (skipOutline != null)
            skipOutline.color = selectedIndex == 1 ? selectedOutlineColor : normalOutlineColor;
    }
}