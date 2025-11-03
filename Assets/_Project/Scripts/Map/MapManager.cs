using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MapManager: MonoBehaviour
{
    [Header("General")]
    public bool minimapOpen = false;
    public float openSpeed;
    public float closeSpeed;
    public RectTransform mapRectTransform;
    public bool IsLoading = false;
    public Image backgroundImage;




    [Header("Minimap")]
    public int miniMapWidth;
    public int miniMapHeight;
    public Color mmbackgroundColor;
    public CanvasGroup mmRoot;
    public Camera mmCamera;


    [Header("OverviewMap")]
    public int overviewMapWidth;
    public int overviewMapHeight;
    public Color ObackgroundColor;

    
    [Header("World Bounds")]
    public Vector2 WorldMin = new Vector2(-15000, -15000);
    public Vector2 WorldMax = new Vector2(15000, 15000);
    public GameObject mapRoot;
    public CanvasGroup mapCG;




    void Start()
    {
        CloseOverviewMap();
    }

    public void ToggleMap()
    {
        if (IsLoading) return;
        IsLoading = true;
        if (minimapOpen)
        {
            CloseOverviewMap();
        }
        else
        {
            OpenOverviewMap();
        }
    }


    public void CloseOverviewMap()
    {
        minimapOpen = false;
        backgroundImage.DOColor(mmbackgroundColor, .15f);
        mapRectTransform.DOSizeDelta(
            new Vector2(miniMapWidth, miniMapHeight),
            closeSpeed
        )
        .SetEase(Ease.OutCubic)
        .OnComplete(() =>
        {
            IsLoading = false;
            EnableMMCamera();
        });

        mapCG.DOFade(0, .1f)
        .OnComplete(() =>
        {
            mapRoot.SetActive(false);
        });
    }


    public void OpenOverviewMap()
    {
        if (minimapOpen) return;
        DisableMMCamera();
        backgroundImage.DOColor(ObackgroundColor, .15f);
        minimapOpen = true;
        mapRectTransform.DOSizeDelta(
            new Vector2(overviewMapWidth, overviewMapHeight),
            openSpeed
        )
        .SetEase(Ease.InCubic)
        .OnComplete(() =>
        {
            mapCG.DOFade(1, .1f)
            .OnComplete(() =>
            {
                mapRoot.SetActive(true);
                IsLoading = false;
            });
        });
    }

    public bool MinimapOpen()
    {
        return minimapOpen;
    }

    public void DisableMMCamera()
    {
        mmRoot.DOFade(0, .15f)
        .OnComplete(() =>
        {
            mmCamera.gameObject.SetActive(false);
        });
    }
    
    public void EnableMMCamera()
    {
        mmRoot.DOFade(1, .15f);
        mmCamera.gameObject.SetActive(true);
    }
}