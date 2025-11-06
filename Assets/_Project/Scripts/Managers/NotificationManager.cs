using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    // A struct or class to hold notification data
    public class Notification
    {
        public string Message;
        public string Title;
        public float Duration;
        public Sprite Icon;
        // public Action OnShow;  // Callback for when it starts showing
        // public Action OnHide;  // Callback for when it ends

        public Notification(string message, string title, float duration, Sprite icon)
        {
            Message = message;
            Title = title;
            Duration = duration;
            Icon = icon;
            // OnShow = onShow;
            // OnHide = onHide;
        }
    }

    private Queue<Notification> _queue = new Queue<Notification>();
    private bool _isDisplaying = false;


    [Header("UI")]
    public Image iconImage;
    public GameObject root;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public CanvasGroup notifRootCG;
    public Sprite defaultSprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        HideUI();
    }

    /// <summary>
    /// Public method to send a new notification to be displayed.
    /// </summary>
    public static void Show(string message, string title, Sprite icon, float duration = 2f)
    {
        if (Instance == null)
        {
            Debug.LogWarning("NotificationManager not found in scene.");
            return;
        }

        Instance.EnqueueNotification(new Notification(message, title, duration, icon));
    }

    private void EnqueueNotification(Notification notification)
    {
        _queue.Enqueue(notification);

        if (!_isDisplaying)
            StartCoroutine(DisplayNext());
    }

    private IEnumerator DisplayNext()
    {
        _isDisplaying = true;

        while (_queue.Count > 0)
        {
            var notif = _queue.Dequeue();

            // Trigger OnShow logic (UI hook, sound, etc.)
            Logger.Log("New Event " + notif.Title, Logger.LogTypeCategory.GameAction);
            ShowUI(notif);

            yield return new WaitForSeconds(notif.Duration);
            // Trigger OnHide logic (UI hide, animation end, etc.)
            HideUI();
        }

        _isDisplaying = false;
    }

    public void ShowUI(Notification notif)
    {
        notifRootCG.alpha = 0;
        root.SetActive(true);
        titleText.text = notif.Title;
        descriptionText.text = notif.Message;
        iconImage.sprite = notif.Icon;
        notifRootCG.DOFade(.9f, .2f);
    }

    public void HideUI()
    {
        notifRootCG.DOFade(0, .2f)
        .OnComplete(() =>
        {
            root.SetActive(true);
            titleText.text = "";
            descriptionText.text = "";
            iconImage.sprite = defaultSprite;
        });
    }
}
