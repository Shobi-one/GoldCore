using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Fragsurf.Movement;

namespace GoldCore
{
    /// <summary>
    /// The developer console (toggle with the Console button, ~ by default).
    /// Commands are registered with <see cref="Register"/>; the ConVar system in the console phase
    /// will build on this.
    /// </summary>
    public class Control : MonoBehaviour
    {
        [SerializeField] public GameObject console;
        [SerializeField] public TMP_InputField commandInput;
        public GameObject command;
        public GameObject contentController;
        public bool consoleActive;

        static readonly Color InfoColor = new Color32(253, 207, 114, 255);

        private sealed class Command
        {
            public string help;
            public Action<string[]> run;
        }

        private readonly Dictionary<string, Command> commands =
            new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> history = new List<string>();
        private int historyIndex;

        void Start()
        {
            console.SetActive(false);
            consoleActive = false;

            Register("help", "Lists every command.", ShowCommands);
            Register("commands", "Lists every command.", ShowCommands);
            Register("map", "map <scene>: loads a scene from the build settings.", LoadMap);
            Register("maps", "Lists the scenes in the build settings.", ListMaps);
            Register("impulse", "impulse 101: full health, armor and ammo.", Impulse);
            Register("god", "Toggles taking no damage.", God);
            Register("kill", "Kills the player.", _ => Player()?.Kill());
            Register("sv_autobunnyhop", "sv_autobunnyhop <0|1>: jump again automatically while Jump is held.", AutoBunnyhop);
            Register("clear", "Clears the console.", _ => Clear());
            Register("quit", "Quits the game.", _ => Application.Quit());
        }

        /// <summary>Adds a command. Arguments arrive with the command name at index 0.</summary>
        public void Register(string name, string help, Action<string[]> run) => commands[name] = new Command { help = help, run = run };

        void Update()
        {
            if (GameInput.ConsolePressed) Toggle();
            if (!consoleActive) return;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) ExecuteCommand();
            else if (Input.GetKeyDown(KeyCode.UpArrow)) BrowseHistory(-1);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) BrowseHistory(1);
        }

        public void Open()
        {
            if (consoleActive) return;
            console.SetActive(true);
            consoleActive = true;
            GameInput.PushBlock();
            commandInput.ActivateInputField();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
        }

        public void Close()
        {
            if (!consoleActive) return;
            console.SetActive(false);
            consoleActive = false;
            GameInput.PopBlock();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
        }

        public void Toggle()
        {
            if (consoleActive) Close();
            else Open();
        }

        void OnDestroy()
        {
            if (consoleActive) GameInput.PopBlock();
        }

        public void ExecuteCommand()
        {
            string line = commandInput.text.Trim();
            commandInput.text = "";
            commandInput.ActivateInputField();
            if (line.Length == 0) return;

            history.Add(line);
            historyIndex = history.Count;
            Print("] " + line, Color.white);
            Execute(line);
        }

        /// <summary>Runs one or more commands separated by ';', like a Source config line.</summary>
        public void Execute(string line)
        {
            foreach (string part in line.Split(';'))
            {
                string[] words = part.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 0) continue;
                if (commands.TryGetValue(words[0], out var entry)) entry.run(words);
                else Print($"Unknown command \"{words[0]}\"", Color.red);
            }
        }

        public void Print(string text) => Print(text, InfoColor);

        public void Print(string text, Color color)
        {
            if (command == null || contentController == null)
            {
                Debug.Log(text);
                return;
            }
            GameObject entry = Instantiate(command, contentController.transform, false);
            var label = entry.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.color = color;
        }

        void Clear()
        {
            foreach (Transform child in contentController.transform) Destroy(child.gameObject);
        }

        void BrowseHistory(int step)
        {
            if (history.Count == 0) return;
            historyIndex = Mathf.Clamp(historyIndex + step, 0, history.Count);
            commandInput.text = historyIndex < history.Count ? history[historyIndex] : "";
            commandInput.caretPosition = commandInput.text.Length;
        }

        static HealthManager Player() => FindFirstObjectByType<HealthManager>();

        void ShowCommands(string[] args)
        {
            foreach (var pair in commands.OrderBy(p => p.Key))
                Print($"{pair.Key}: {pair.Value.help}");
        }

        void LoadMap(string[] args)
        {
            if (args.Length < 2)
            {
                Print("Usage: map <scene>. Type maps for the list.");
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(args[1]))
            {
                Print($"Map \"{args[1]}\" isn't in the build settings.", Color.red);
                return;
            }
            Print("Loading map " + args[1]);
            Close();
            SceneManager.LoadScene(args[1]);
        }

        void ListMaps(string[] args)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                Print(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
        }

        void Impulse(string[] args)
        {
            if (args.Length < 2 || args[1] != "101")
            {
                Print("Usage: impulse 101");
                return;
            }
            HealthManager player = Player();
            if (player == null) return;
            player.Heal(player.maxHealth);
            player.AddArmor(player.maxArmor);
            foreach (GunSystem gun in player.GetComponentsInChildren<GunSystem>(true)) gun.IncreaseAmmo(gun.magazineSize * 4);
            Print("Gave health, armor and ammo.");
        }

        void God(string[] args)
        {
            HealthManager player = Player();
            if (player == null) return;
            player.godMode = !player.godMode;
            Print(player.godMode ? "godmode ON" : "godmode OFF");
        }

        void AutoBunnyhop(string[] args)
        {
            SurfCharacter surfer = FindFirstObjectByType<SurfCharacter>();
            if (surfer == null) return;
            if (args.Length > 1) surfer.movementConfig.autoBhop = args[1] != "0";
            Print($"sv_autobunnyhop = {(surfer.movementConfig.autoBhop ? 1 : 0)}");
        }
    }
}
