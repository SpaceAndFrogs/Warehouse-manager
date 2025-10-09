using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField]
    GameObject menuObject;
    [SerializeField]
    GameObject menuPanel;
    [SerializeField]
    Button close;
    [SerializeField]
    Button open;
    [SerializeField]
    Button settingsMenuButton;
    [SerializeField]
    Button backFromSettingsMenu;
    [SerializeField]
    GameObject settingsMenu;
    [SerializeField]
    TextMeshProUGUI menuTitle;


    void CloseGameMenu()
    {
        menuObject.SetActive(false);
    }

    void OpenGameMenu()
    {
        menuObject.SetActive(true);
        menuPanel.SetActive(true);
        menuTitle.text = "Menu";
    }
    void OpenSettingsMenu(bool open)
    {
        if(open)
        {
            settingsMenu.SetActive(true);
            menuPanel.SetActive(false);
            menuTitle.text = "Settings";
        }
        else
        {
            settingsMenu.SetActive(false);
            menuPanel.SetActive(true);
            menuTitle.text = "Menu";
        }
    }

    void Awake()
    {
        AddListeners();
        CloseGameMenu();   
    }

    void AddListeners()
    {
        close.onClick.AddListener(CloseGameMenu);
        open.onClick.AddListener(OpenGameMenu);
        settingsMenuButton.onClick.AddListener(() => OpenSettingsMenu(true));
        backFromSettingsMenu.onClick.AddListener(() => OpenSettingsMenu(false));
    }
}
