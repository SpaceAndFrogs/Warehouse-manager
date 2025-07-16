using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class LoadRowScript : MonoBehaviour
{
    public Button loadButton;
    public Button deleteSaveButton;
    public TextMeshProUGUI saveNameText;

    public void DeleteSave()
    {
        string fileName = saveNameText.text + ".json";
        string filePath = System.IO.Path.Combine(Path.Combine(Application.persistentDataPath, SavingManager.instance.saveDirectoryName), fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Usunięto plik: " + filePath);
        }
        else
        {
            Debug.Log("Plik nie istnieje: " + filePath);
        }

        SavingManager.instance.loadRowScripts.Remove(this);
        Destroy(gameObject);
    }
}
