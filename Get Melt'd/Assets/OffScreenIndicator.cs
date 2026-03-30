using UnityEngine;
using UnityEngine.UI;

public class OffScreenIndicator : MonoBehaviour
{
    public Transform target;          // The Fridge Hinge/Body
    public Renderer targetRenderer;   // Drag the "Goal-Arrow" Mesh/Sprite Renderer here
    public float margin = 50f;
    public float rotationOffset = -90f;

    private Camera mainCam;
    private RectTransform rectTransform;
    private Image arrowImage;

    void Start()
    {
        mainCam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        arrowImage = GetComponent<Image>();

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
    }

    void Update()
    {
        if (target == null || targetRenderer == null) return;

        // 1. Check if the 3D Billboard is visible to the camera
        if (targetRenderer.isVisible)
        {
            // If the player can see the 3D arrow, hide the UI arrow
            arrowImage.enabled = false;
            return;
        }
        else
        {
            // If the 3D arrow is hidden (behind a wall or behind the player), show UI arrow
            arrowImage.enabled = true;
        }

        // 2. Standard Off-Screen Logic
        Vector3 screenPos = mainCam.WorldToScreenPoint(target.position);

        if (screenPos.z < 0)
        {
            screenPos *= -1;
        }

        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2f;
        Vector3 direction = screenPos - screenCenter;

        Vector3 cappedScreenPos = screenPos;
        cappedScreenPos.x = Mathf.Clamp(cappedScreenPos.x, margin, Screen.width - margin);
        cappedScreenPos.y = Mathf.Clamp(cappedScreenPos.y, margin, Screen.height - margin);

        rectTransform.position = cappedScreenPos;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    }
}