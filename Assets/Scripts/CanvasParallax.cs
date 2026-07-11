using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CanvasParallax : MonoBehaviour
{
    [SerializeField] private RectTransform mountainLayer;
    [SerializeField] private Sprite[] randomBackgrounds;
    [SerializeField] private float strength = 20f;
    [SerializeField] private float smooth = 5f;
    [SerializeField] private float autoSpeed = 0.2f;
    [SerializeField] private float extraSize = 80f;

    private Vector2 startPosition;
    private Vector2 lastViewportSize;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            RandomizeBackground();
        }

        if (mountainLayer != null)
        {
            FitMountainLayerToCanvas();
        }
    }

    private void RandomizeBackground()
    {
        if (mountainLayer == null || randomBackgrounds == null || randomBackgrounds.Length == 0)
        {
            return;
        }

        Image image = mountainLayer.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        image.sprite = randomBackgrounds[Random.Range(0, randomBackgrounds.Length)];
        lastViewportSize = Vector2.zero;
    }

    private void Update()
    {
        if (mountainLayer == null)
        {
            return;
        }

        FitMountainLayerToCanvas();

        if (!Application.isPlaying)
        {
            return;
        }

        float time = Time.time * autoSpeed;
        Vector2 offset = new Vector2(Mathf.Sin(time), Mathf.Cos(time * 0.7f));
        Vector2 targetPosition = startPosition + offset * strength;

        mountainLayer.anchoredPosition = Vector2.Lerp(
            mountainLayer.anchoredPosition,
            targetPosition,
            Time.deltaTime * smooth
        );
    }

    private void FitMountainLayerToCanvas()
    {
        RectTransform viewport = GetViewportRect();
        if (viewport == null)
        {
            return;
        }

        mountainLayer.SetAsFirstSibling();

        Vector2 viewportSize = viewport.rect.size;
        if (viewportSize == lastViewportSize)
        {
            return;
        }

        lastViewportSize = viewportSize;
        mountainLayer.anchorMin = new Vector2(0.5f, 0.5f);
        mountainLayer.anchorMax = new Vector2(0.5f, 0.5f);
        mountainLayer.pivot = new Vector2(0.5f, 0.5f);

        Image image = mountainLayer.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = false;
        }

        Sprite sprite = image != null ? image.sprite : null;
        if (sprite == null)
        {
            mountainLayer.sizeDelta = viewportSize + Vector2.one * extraSize;
        }
        else
        {
            Vector2 spriteSize = sprite.rect.size;
            float scale = Mathf.Max(viewportSize.x / spriteSize.x, viewportSize.y / spriteSize.y);
            mountainLayer.sizeDelta = spriteSize * scale + Vector2.one * extraSize;
        }

        startPosition = Vector2.zero;
        mountainLayer.anchoredPosition = startPosition;
    }

    private RectTransform GetViewportRect()
    {
        Canvas canvas = mountainLayer.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            return canvas.transform as RectTransform;
        }

        return mountainLayer.parent as RectTransform;
    }
}
