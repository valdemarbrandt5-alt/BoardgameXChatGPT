using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreboardSlotUI : MonoBehaviour
{
    [Header("Player Color")]
    public RawImage playerColorImage;

    [Header("Name")]
    public TMP_Text nameText;

    [Header("HP")]
    public Slider hpSlider;
    public TMP_Text hpText;

    [Header("Coins")]
    public TMP_Text coinsText;

    [Header("Trophies")]
    public GameObject[] trophyIcons;

    [Header("Highlight")]
    public Image backgroundImage;
    public Color normalBackgroundColor = Color.white;
    public Color activeBackgroundColor = new Color(1f, 1f, 0.5f, 1f);

    private PlayerStats player;

    public void SetPlayer(PlayerStats targetPlayer)
    {
        player = targetPlayer;
        Refresh(false);
    }

    public PlayerStats GetPlayer()
    {
        return player;
    }

    public void Refresh(bool isActive)
    {
        if (player == null)
            return;

        // 🎨 Player color
        if (playerColorImage != null)
            playerColorImage.color = player.playerColor;

        // 🏷 Name
        if (nameText != null)
            nameText.text = player.displayName;

        // ❤️ HP (Slider + Text)
        if (hpSlider != null)
        {
            hpSlider.maxValue = player.maxHp;
            hpSlider.value = player.hp;

            // 🔥 HP color (green → yellow → red)
            Image fill = hpSlider.fillRect.GetComponent<Image>();

            if (fill != null)
            {
                float percent = player.maxHp > 0 ? (float)player.hp / player.maxHp : 0f;

                if (percent > 0.64f)
                    fill.color = Color.green;
                else if (percent > 0.34f)
                    fill.color = Color.yellow;
                else
                    fill.color = Color.red;
            }
        }

        if (hpText != null)
            hpText.text = player.hp.ToString();

        // 💰 Coins
        if (coinsText != null)
            coinsText.text = player.coins.ToString();

        // 🏆 Trophies
        if (trophyIcons != null)
        {
            for (int i = 0; i < trophyIcons.Length; i++)
            {
                if (trophyIcons[i] != null)
                    trophyIcons[i].SetActive(i < player.trophies);
            }
        }

        // ⭐ Active player highlight
        if (backgroundImage != null)
            backgroundImage.color = isActive ? activeBackgroundColor : normalBackgroundColor;
    }
}