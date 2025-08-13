using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField]
    GameObject gameMenuPanel;
    [SerializeField]
    Button close;
    [SerializeField]
    Button open;

    void CloseGameMenu()
    {
        gameMenuPanel.SetActive(false);
    }

    void OpenGameMenu()
    {
        gameMenuPanel.SetActive(true);
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
    }
}
