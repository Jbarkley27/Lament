using UnityEngine;
using UnityEngine.UI;

public class MinimapElement: MonoBehaviour
{
    public Image IconImage;
    public Sprite Icon;
    public Color IconColor;
    public int IconDimension;

    void Start()
    {
        SetupIcon();
    }

    public void SetupIcon()
    {
        IconImage.sprite = Icon;
        IconImage.color = IconColor;
        IconImage.rectTransform.sizeDelta = new Vector2(IconDimension, IconDimension);
    }
}