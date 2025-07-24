using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NotificationsData", menuName = "NotificationsData", order = 0)]
public class NotificationsData : ScriptableObject
{
    public enum NotificationType
    {
        Info,
        Warning,
        Error
    }

    public NotificationType notificationType;
    public string message;
    public float displayDuration = 3f;
}
