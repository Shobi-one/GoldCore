using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Control : MonoBehaviour
{
    //[SerializeField] public GameObject Player;
    [SerializeField] public GameObject console;
    [SerializeField] public TMP_InputField commandInput;
    public GameObject command;
    public GameObject contentController;
    public bool consoleActive;
    private string CommandSplit;

    // Start is called before the first frame update
    void Start()
    {
        console.SetActive(false);
        consoleActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Console"))
        {
            Toggle();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Command();
        }
    }
    public void Open()
    {
        console.SetActive(true);
        consoleActive = true;
        commandInput.ActivateInputField();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
    }

    public void Command()
    {
        var NewCommand = Instantiate(command);
        NewCommand.transform.SetParent(contentController.transform, false);
        NewCommand.GetComponent<TextMeshProUGUI>().text ="]" + commandInput.text;
        char[] delimiters = { ' ', ' '};
        string[] words = NewCommand.GetComponent<TextMeshProUGUI>().text.Split(delimiters);
        commandInput.text = "";
        commandInput.ActivateInputField();

        //command loader
        if(words[0] == "]map")
        {
            SceneManager.LoadScene(words[1]);
            var CommandCheck = Instantiate(command);
            CommandCheck.transform.SetParent(contentController.transform, false);
            CommandCheck.GetComponent<TextMeshProUGUI>().text = "Loading map " + words[1];
            Close();
        }

        if (words[0] == "]impulse 101" && words[1] == "101")
        {
            var CommandCheck = Instantiate(command);
            CommandCheck.transform.SetParent(contentController.transform, false);
            CommandCheck.GetComponent<TextMeshProUGUI>().text = "To be added...";
            CommandCheck.GetComponent<TextMeshProUGUI>().color = new Color32(253, 207, 114, 255);
        }

        if (words[0] == "]sv_autobunnyhop" && words[1] == "1")
        {
            
            var CommandCheck = Instantiate(command);
            CommandCheck.transform.SetParent(contentController.transform, false);
            CommandCheck.GetComponent<TextMeshProUGUI>().text = "To be added...";
            CommandCheck.GetComponent<TextMeshProUGUI>().color = new Color32(253, 207, 114, 255);
        }

        if (words[0] == "]commands")
        {

            var CommandCheck = Instantiate(command);
            CommandCheck.transform.SetParent(contentController.transform, false);
            CommandCheck.GetComponent<TextMeshProUGUI>().text = "map";
            CommandCheck.GetComponent<TextMeshProUGUI>().color = new Color32(253, 207, 114, 255);
            var CommandCheck1 = Instantiate(command);
            CommandCheck.transform.SetParent(contentController.transform, false);
            CommandCheck.GetComponent<TextMeshProUGUI>().text = "maplist";
            CommandCheck.GetComponent<TextMeshProUGUI>().color = new Color32(253, 207, 114, 255);
        }

        if (words[0] == "]quit")
        {
            Application.Quit();
        }
    }
    public void Close()
    {
        console.SetActive(false);
        consoleActive = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
    }

    public void Toggle()
    {
        if (consoleActive)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

}
