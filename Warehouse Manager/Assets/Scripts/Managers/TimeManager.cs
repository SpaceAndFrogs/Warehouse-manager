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
        AddListeners();
    }

    void AddListeners()
    {
        HotkeysManager.OnKeyPressed += CheckForPauseInput;
    }

    void CheckForPauseInput(KeyCode kcode)
    {
        if (kcode == KeyCode.Space)
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
            // If paused, unpause and apply change
            if (Time.timeScale == 0)
            {
                Time.timeScale = currentTimeScale;
                isPaused = false;
            }

            float newTimeScale = Time.timeScale + timeScale;
            if (newTimeScale < 0)
            {
                newTimeScale = 0;
            }

            Time.timeScale = newTimeScale;
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
