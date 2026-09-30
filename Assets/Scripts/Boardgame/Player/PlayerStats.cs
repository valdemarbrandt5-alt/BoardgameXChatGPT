using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public string displayName = "Player";
    public int maxHp = 30;
    public int hp = 30;
    public int coins = 0;
    public int trophies = 0;

    public Color playerColor = Color.white;
    public BoardTile currentTile;

    public int Heal(int amount)
    {
        if (amount <= 0)
            return 0;

        int oldHp = hp;
        hp = Mathf.Min(hp + amount, maxHp);
        int healed = hp - oldHp;

        if (healed > 0 && DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowHealPopup(transform.position, healed);
        }

        return healed;
    }

    public int AddCoins(int amount, bool showPopup = false)
    {
        if (amount <= 0)
            return 0;

        coins += amount;

        if (showPopup && DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowHealPopup(transform.position, amount);
        }

        return amount;
    }

    public int RemoveCoins(int amount)
    {
        if (amount <= 0)
            return 0;

        int removed = Mathf.Min(coins, amount);
        coins -= removed;
        return removed;
    }

    public void TakeDamage(int damage)
    {
        PlayGenericHitReaction(damage);
        ShowDamagePopup(damage);

        int coinLoss = RemoveCoins(damage);

        if (CoinDropManager.Instance != null)
        {
            CoinDropManager.Instance.DropCoins(
                coinLoss,
                this,
                transform.position
            );
        }

        hp -= damage;

        if (hp <= 0)
        {
            Die();
        }
    }

    public void TakeDamageFromSource(int damage, Transform source)
    {
        PlayDirectedHitReaction(damage, source);
        ShowDamagePopup(damage);

        int coinLoss = RemoveCoins(damage);

        if (CoinDropManager.Instance != null)
        {
            CoinDropManager.Instance.DropCoins(
                coinLoss,
                this,
                transform.position
            );
        }

        hp -= damage;

        if (hp <= 0)
        {
            Die();
        }
    }

    private void ShowDamagePopup(int damage)
    {
        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowDamagePopup(transform.position, damage);
        }
    }

    private void PlayGenericHitReaction(int damage)
    {
        PlayerHitReaction hitReaction = GetComponent<PlayerHitReaction>();
        if (hitReaction == null)
            return;

        Vector3 randomDir = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        hitReaction.PlayHitReaction(randomDir, damage);
    }

    private void PlayDirectedHitReaction(int damage, Transform source)
    {
        PlayerHitReaction hitReaction = GetComponent<PlayerHitReaction>();
        if (hitReaction == null)
            return;

        Vector3 dir;

        if (source != null)
        {
            dir = transform.position - source.position;
            dir.y = 0f;
            dir = dir.normalized;
        }
        else
        {
            dir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
        }

        hitReaction.PlayHitReaction(dir, damage);
    }

    private void Die()
    {
        int loss = Mathf.FloorToInt(coins * 0.33f);
        RemoveCoins(loss);

        if (CoinDropManager.Instance != null)
        {
            CoinDropManager.Instance.DropCoins(
                loss,
                this,
                transform.position
            );
        }

        hp = maxHp;

        Debug.Log(name + " died");
    }
}