using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    [SerializeField]
    float oneHourInSeconds;
    [SerializeField] TextMeshProUGUI currentSpeedTMP;
    public float currentTimeScale { get;  private set; }
    bool isPaused = false;

    void Awake()
    {
        MakeInstance();
    }

    void Start()
    {
        currentSpeedTMP.text = Time.timeScale.ToString() + " " + "X";
        currentTimeScale = Time.timeScale;
    }

    void Update()
    {
        CheckForPauseInput();
    }

    void CheckForPauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TogglePause();
    }

    void MakeInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetOneHour()
    {
        return oneHourInSeconds;
    }

    public float GetOneDay()
    {
        return oneHourInSeconds * 24;
    }

    public float GetOneWeek()
    {
        return oneHourInSeconds * 168;
    }

    public float GetOneMounth()
    {
        return oneHourInSeconds * 720;
    }

    public void ChangeTimeScale(float timeScale)
    {
        if (timeScale != 0)
        {
            if (Time.timeScale == 0)
            {
                if (isPaused)
                {
                    isPaused = false;
                    Time.timeScale = currentTimeScale;
                }
                else
                {
                    return;
                }
            }

            Time.timeScale += timeScale;
            if (Time.timeScale < 0)
            {
                Time.timeScale = 0;
                NotificationsManager.instance.ShowNotification(NotificationsData.NotificationType.TimeLowerThenZero);
            }

            currentTimeScale = Time.timeScale;
            currentSpeedTMP.text = Time.timeScale.ToString() + " " + "X";
        }
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0f)
        {
            currentTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;
        }
        else
        {
            Time.timeScale = currentTimeScale;
            isPaused = false;
        }

        currentSpeedTMP.text = Time.timeScale.ToString() + " " + "X";
    }
    

}
