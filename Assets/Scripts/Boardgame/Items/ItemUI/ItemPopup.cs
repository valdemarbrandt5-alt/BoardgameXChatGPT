using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopup : MonoBehaviour
{
    public Image itemImage;
    

    public float lifetime = 1f;
    public float moveSpeed = 1.5f;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Setup(Sprite icon)
    {
        if (itemImage != null)
        {
            itemImage.sprite = icon;
            itemImage.enabled = icon != null;
        }

        

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (mainCamera != null)
        {
            Vector3 dir = mainCamera.transform.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}