using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class WinScreenRandomImage : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] randomSprites;

    private void Awake()
    {
        ApplyRandomImage();
    }

    public void ApplyRandomImage()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage == null || randomSprites == null || randomSprites.Length == 0)
        {
            return;
        }

        Sprite selectedSprite = randomSprites[Random.Range(0, randomSprites.Length)];
        if (selectedSprite != null)
        {
            targetImage.sprite = selectedSprite;
        }
    }
}
