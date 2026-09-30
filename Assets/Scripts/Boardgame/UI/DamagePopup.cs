using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public TMP_Text damageText;
    public float lifetime = 1f;
    public float moveSpeed = 1.5f;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Setup(int amount, bool isHeal)
    {
        if (damageText != null)
        {
            if (isHeal)
            {
                damageText.text = "+" + amount;
                damageText.color = Color.green;
            }
            else
            {
                damageText.text = "-" + amount;
                damageText.color = Color.softRed;
            }
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }
}