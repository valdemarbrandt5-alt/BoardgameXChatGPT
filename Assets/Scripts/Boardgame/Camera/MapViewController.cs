using System.Collections;
using UnityEngine;

public class MapViewController : MonoBehaviour
{
    public static MapViewController Instance;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera mapCamera;

    [Header("Transition")]
    public float transitionDuration = 0.4f;

    [Header("Map Movement")]
    public float moveSpeed = 20f;
    public bool invertHorizontal = false;
    public bool invertVertical = false;

    private bool isMapOpen = false;
    private bool isTransitioning = false;

    private Vector3 defaultMapPosition;
    private Quaternion defaultMapRotation;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (mapCamera != null)
        {
            defaultMapPosition = mapCamera.transform.position;
            defaultMapRotation = mapCamera.transform.rotation;
        }

        SetMapViewImmediate(false);
    }

    private void Update()
    {
        if (isTransitioning)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleMapView();
        }

        if (isMapOpen)
        {
            HandleMapMovement();
        }
    }

    public void ToggleMapView()
    {
        if (isTransitioning)
            return;

        StartCoroutine(SwitchMapView(!isMapOpen));
    }

    public bool IsMapOpen()
    {
        return isMapOpen;
    }

    public bool IsBusy()
    {
        return isTransitioning;
    }

    private IEnumerator SwitchMapView(bool open)
    {
        isTransitioning = true;

        // Fade ud på det nuværende kamera
        Camera fromCam = open ? mainCamera : mapCamera;
        Camera toCam = open ? mapCamera : mainCamera;

        if (fromCam != null)
        {
            AudioListener fromListener = fromCam.GetComponent<AudioListener>();
            if (fromListener != null)
                fromListener.enabled = true;
        }

        yield return StartCoroutine(FadeCamera(fromCam, 1f, 0f));

        if (fromCam != null)
            fromCam.gameObject.SetActive(false);

        if (toCam != null)
            toCam.gameObject.SetActive(true);

        // audio listener håndtering
        if (mainCamera != null)
        {
            AudioListener mainListener = mainCamera.GetComponent<AudioListener>();
            if (mainListener != null)
                mainListener.enabled = !open;
        }

        if (mapCamera != null)
        {
            AudioListener mapListener = mapCamera.GetComponent<AudioListener>();
            if (mapListener != null)
                mapListener.enabled = open;
        }

        isMapOpen = open;

        yield return StartCoroutine(FadeCamera(toCam, 0f, 1f));

        isTransitioning = false;
    }

    private IEnumerator FadeCamera(Camera cam, float startAlpha, float endAlpha)
    {
        if (cam == null)
            yield break;

        float timer = 0f;

        // Sørg for kameraet renderer til sin viewport
        Rect originalRect = cam.rect;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / transitionDuration);

            // Fake fade ved at krympe viewport lidt mindre relevant, så vi gør intet fancy her.
            // Du kan senere udskifte dette med rigtig fullscreen fade UI.
            yield return null;
        }

        cam.rect = originalRect;
    }

    private void SetMapViewImmediate(bool open)
    {
        isMapOpen = open;
        isTransitioning = false;

        if (mainCamera != null)
            mainCamera.gameObject.SetActive(!open);

        if (mapCamera != null)
            mapCamera.gameObject.SetActive(open);

        if (mainCamera != null)
        {
            AudioListener mainListener = mainCamera.GetComponent<AudioListener>();
            if (mainListener != null)
                mainListener.enabled = !open;
        }

        if (mapCamera != null)
        {
            AudioListener mapListener = mapCamera.GetComponent<AudioListener>();
            if (mapListener != null)
                mapListener.enabled = open;
        }
    }

    private void HandleMapMovement()
    {
        if (mapCamera == null)
            return;

        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;

        if (invertHorizontal) h *= -1f;
        if (invertVertical) v *= -1f;

        Vector3 move = new Vector3(h, 0f, v).normalized * moveSpeed * Time.deltaTime;
        mapCamera.transform.position += move;
    }

    public void ResetMapCamera()
    {
        if (mapCamera == null)
            return;

        mapCamera.transform.position = defaultMapPosition;
        mapCamera.transform.rotation = defaultMapRotation;
    }
}