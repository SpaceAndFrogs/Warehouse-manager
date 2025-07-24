using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotificationsManager : MonoBehaviour
{
    public static NotificationsManager Instance;
    public List<NotificationsData> notificationsList;
    public NotificationScript notificationScript;

    void Awake()
    {
        Init();
    }

    void Init()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowNotification(NotificationsData.NotificationType type)
    {
        foreach (var notification in notificationsList)
        {
            if (notification.notificationType == type)
            {
                StopCoroutine(DisplayNotification(notification.displayDuration));
                notificationScript.gameObject.SetActive(false);

                notificationScript.textMeshProUGUI.text = notification.message;
                StartCoroutine(DisplayNotification(notification.displayDuration));
                break;
            }
        }
    }

    IEnumerator DisplayNotification(float displayDuration)
    {
        notificationScript.gameObject.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        notificationScript.gameObject.SetActive(false);
    }
}
