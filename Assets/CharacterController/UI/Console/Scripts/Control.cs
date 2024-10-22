using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Control : MonoBehaviour
{
    [SerializeField] public GameObject console;
    [SerializeField] public TMP_InputField commandInput;
    public GameObject command;
    public GameObject contentController;
    public bool consoleActive;

    // Dictionary to store commands and actions
    private Dictionary<string, System.Action<string[]>> commandActions;

    // Start is called before the first frame update
    void Start()
    {
        console.SetActive(false);
        consoleActive = false;

        // Initialize the command dictionary
        commandActions = new Dictionary<string, System.Action<string[]>>()
        {
            { "map", LoadMap },
            { "impulse", ImpulseCommand },
            { "sv_autobunnyhop", AutoBunnyhop },
            { "commands", ShowCommands },
            { "quit", QuitGame }
        };
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
            ExecuteCommand();
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

    public void ExecuteCommand()
    {
        var NewCommand = Instantiate(command);
        NewCommand.transform.SetParent(contentController.transform, false);
        NewCommand.GetComponent<TextMeshProUGUI>().text = "]" + commandInput.text;
        
        string[] words = commandInput.text.Split(' ');
        commandInput.text = "";
        commandInput.ActivateInputField();

        // Check if command exists in dictionary
        if (commandActions.ContainsKey(words[0]))
        {
            commandActions[words[0]].Invoke(words); // Execute the command's associated action
        }
        else
        {
            var CommandCheck = Instantiate(command);
            CommandCheck.transform.SetParent(contentController.transform, false);
            CommandCheck.GetComponent<TextMeshProUGUI>().text = "Unknown command";
            CommandCheck.GetComponent<TextMeshProUGUI>().color = Color.red;
        }
    }

    private void LoadMap(string[] args)
    {
        if (args.Length > 1)
        {
            SceneManager.LoadScene(args[1]);
            var CommandCheck = Instantiate(command);
            CommandCheck.transform.SetParent(contentController.transform, false);
            CommandCheck.GetComponent<TextMeshProUGUI>().text = "Loading map " + args[1];
            Close();
        }
    }

    private void ImpulseCommand(string[] args)
    {
        if (args.Length > 1 && args[1] == "101")
        {
            var CommandCheck = Instantiate(command);
            CommandCheck.transform.SetParent(contentController.transform, false);
            CommandCheck.GetComponent<TextMeshProUGUI>().text = "To be added...";
            CommandCheck.GetComponent<TextMeshProUGUI>().color = new Color32(253, 207, 114, 255);
        }
    }

    private void AutoBunnyhop(string[] args)
    {
        if (args.Length > 1 && args[1] == "1")
        {
            var CommandCheck = Instantiate(command);
            CommandCheck.transform.SetParent(contentController.transform, false);
            CommandCheck.GetComponent<TextMeshProUGUI>().text = "To be added...";
            CommandCheck.GetComponent<TextMeshProUGUI>().color = new Color32(253, 207, 114, 255);
        }
    }

    private void ShowCommands(string[] args)
    {
        var CommandCheck = Instantiate(command);
        CommandCheck.transform.SetParent(contentController.transform, false);
        CommandCheck.GetComponent<TextMeshProUGUI>().text = "map, maplist, quit";
        CommandCheck.GetComponent<TextMeshProUGUI>().color = new Color32(253, 207, 114, 255);
    }

    private void QuitGame(string[] args)
    {
        Application.Quit();
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
