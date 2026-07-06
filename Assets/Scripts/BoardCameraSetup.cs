using UnityEngine;

public class BoardCameraSetup : MonoBehaviour
{
    [Header("Camera Safe Padding")]
    [SerializeField, Range(0f, 0.35f)] private float topScreenPadding = 0.12f;
    [SerializeField, Range(0f, 0.35f)] private float bottomScreenPadding = 0.18f;
    [SerializeField, Range(0f, 0.2f)] private float horizontalScreenPadding = 0.04f;

    public void SetupCamera(int width, int height, float borderSize)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || width <= 0 || height <= 0)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;
        float safeWidth = safeArea.width > 0f ? safeArea.width : Screen.width;
        float safeHeight = safeArea.height > 0f ? safeArea.height : Screen.height;

        float leftPadding = safeWidth * horizontalScreenPadding;
        float rightPadding = safeWidth * horizontalScreenPadding;
        float topPadding = safeHeight * topScreenPadding;
        float bottomPadding = safeHeight * bottomScreenPadding;
        float availableWidth = Mathf.Max(1f, safeWidth - leftPadding - rightPadding);
        float availableHeight = Mathf.Max(1f, safeHeight - topPadding - bottomPadding);

        float boardWorldWidth = width + borderSize * 2f;
        float boardWorldHeight = height + borderSize * 2f;
        float pixelsPerWorldUnit = Mathf.Min(availableWidth / boardWorldWidth, availableHeight / boardWorldHeight);
        mainCamera.orthographicSize = safeHeight / (pixelsPerWorldUnit * 2f);

        // Compensa safe area y padding para mantener el tablero visible en pantallas con notch o barras.
        float safeCenterX = safeArea.xMin + safeWidth / 2f;
        float safeCenterY = safeArea.yMin + safeHeight / 2f;
        float availableCenterX = safeArea.xMin + leftPadding + availableWidth / 2f;
        float availableCenterY = safeArea.yMin + bottomPadding + availableHeight / 2f;
        float worldUnitsPerPixel = (mainCamera.orthographicSize * 2f) / safeHeight;

        float boardCenterX = (width - 1) / 2f;
        float boardCenterY = (height - 1) / 2f;
        float cameraX = boardCenterX - (availableCenterX - safeCenterX) * worldUnitsPerPixel;
        float cameraY = boardCenterY - (availableCenterY - safeCenterY) * worldUnitsPerPixel;
        mainCamera.transform.position = new Vector3(cameraX, cameraY, -10f);
    }
}
