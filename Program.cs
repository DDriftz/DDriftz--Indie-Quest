using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

// ===============================================================
// Namespace and Core Enums/Data Classes
// ===============================================================

namespace AIChatbotHorror
{
    enum ChatbotTone { Friendly, Ambiguous, Sinister }

    class DialogueNode
    {
        public string Message { get; set; }
        public Dictionary<string, DialogueNode> Responses { get; } = new Dictionary<string, DialogueNode>(StringComparer.OrdinalIgnoreCase);
        public int AwarenessChange { get; set; }

        public DialogueNode(string message, int awarenessChange = 0)
        {
            Message = message;
            AwarenessChange = awarenessChange;
        }
    }

    class Room
    {
        public string Name { get; }
        public string Description { get; }
        public Dictionary<string, string> Exits { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public bool RequiresKeycard { get; }
        public List<string> Objects { get; }
        public bool IsSealed { get; set; }

        public Room(string name, string description, bool requiresKeycard = false, List<string>? objects = null)
        {
            Name = name;
            Description = description;
            RequiresKeycard = requiresKeycard;
            Objects = objects ?? new List<string>();
        }
    }

    class Item
    {
        public string Name { get; }
        public string ExamineDescription { get; }
        public Action<GameState, Room> ExamineEffect { get; }
        public Action<GameState, Room> UseAction { get; }

        public Item(string name, string examineDescription, Action<GameState, Room> examineEffect, Action<GameState, Room> useAction)
        {
            Name = name;
            ExamineDescription = examineDescription;
            ExamineEffect = examineEffect;
            UseAction = useAction;
        }
    }

    class GameState
    {
        public bool ExitGame { get; set; }
        public bool IntroductionShown { get; set; }
        public bool TutorialCompleted { get; set; }
        public int AwarenessLevel { get; set; }
        public int SanityLevel { get; set; } = 100;
        public ChatbotTone ChatbotTone { get; set; } = ChatbotTone.Friendly;
        public string PlayerName { get; set; } = string.Empty;
        public List<string> Inventory { get; } = new List<string>();
        public List<string> JournalEntries { get; } = new List<string>();
        public string CurrentRoom { get; set; } = "Lobby";
        public DialogueNode? CurrentNode { get; set; }
        public Stack<DialogueNode> PreviousNodes { get; } = new Stack<DialogueNode>();
        public int TurnCount { get; set; }
        public int LastRestTurn { get; set; } = -5;
        public bool GlitchMode { get; set; }
        public bool DebugMode { get; set; }
        public Dictionary<string, bool> TutorialFlags { get; } = new Dictionary<string, bool>();
    }

    class Logger
    {
        private readonly List<string> logs = new List<string> { $"[{DateTime.Now}] Game initialized." };

        public void Log(string message) => logs.Add($"[{DateTime.Now}] {message}");
        public IEnumerable<string> GetLogs() => logs;
    }

    static class UI
    {
        private static readonly Random rnd = new Random();
    
        public static void PrintTitle()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("===================================");
            Console.WriteLine("            SYNAPSE");
            Console.WriteLine("         AI Chatbot Horror");
            Console.WriteLine("===================================");
            Console.ResetColor();
        }

        public static void Print(string message, ConsoleColor color = ConsoleColor.White, int delay = 0, bool glitch = false)
        {
            Console.ForegroundColor = color;
            if (delay == 0 || !glitch)
            {
                Console.WriteLine(message);
            }
            else
            {
                foreach (char c in message)
                {
                    Console.Write(glitch && rnd.NextDouble() < 0.1 ? (char)rnd.Next(33, 126) : c);
                    Thread.Sleep(10);
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        public static void PrintPrompt()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n> ");
            Console.ResetColor();
        }

        public static string GetInput()
        {
            string? input = Console.ReadLine()?.Trim();
            while (input == null)
            {
                Print("Input was invalid. Please try again.", ConsoleColor.Red);
                input = Console.ReadLine()?.Trim();
            }
            return input;
        }

        public static void ClearScreen() => Console.Clear();
    }

    class Command
    {
        public string Name { get; }
        public Action<GameState, string> Execute { get; }
        public bool IsTurnAdvancing { get; }

        public Command(string name, Action<GameState, string> execute, bool isTurnAdvancing = true)
        {
            Name = name;
            Execute = execute;
            IsTurnAdvancing = isTurnAdvancing;
        }
    }

    class TutorialStep
    {
        public string Title { get; }
        public string Instruction { get; }
        public Func<string, bool> Validator { get; }
        public string? Command { get; }
        public string SuccessMessage { get; }
        public string ErrorMessage { get; }
        public string? Hint { get; }
        public string? HelpText { get; }
        public Action? Setup { get; }
        public Action<string>? CustomAction { get; }
        public Func<bool>? Condition { get; }

        public TutorialStep(string title, string instruction, Func<string, bool> validator, string? command,
            string successMessage, string errorMessage, string? hint = null, string? helpText = null,
            Action? setup = null, Action<string>? customAction = null, Func<bool>? condition = null, Func<bool>? Condition = null)
        {
            Title = title;
            Instruction = instruction;
            Validator = validator;
            Command = command;
            SuccessMessage = successMessage;
            ErrorMessage = errorMessage;
            Hint = hint;
            HelpText = helpText;
            Setup = setup;
            CustomAction = customAction;
            this.Condition = Condition;
            Condition = condition;
        }
    }

    class Program
    {
        private static readonly Random rnd = new Random();
        private static readonly GameState state = new GameState();
        private static readonly Logger logger = new Logger();
        private static readonly Dictionary<string, Room> rooms = InitializeRooms();
        private static readonly Dictionary<ChatbotTone, DialogueNode> dialogueCache = new Dictionary<ChatbotTone, DialogueNode>();
        private static readonly SortedDictionary<int, Action> timedEvents = new SortedDictionary<int, Action>();
        private static readonly string[] conversationHistory = new string[500];
        private static int historyCount = 0;
        private const string saveFile = "savegame.txt";
        private const string saveFileVersion = "1.3";
        private const int maxConversationHistory = 500;
        private const int maxJournalEntries = 100;

        private static readonly HashSet<string> takeableItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "keycard", "flashlight", "data disk", "decryption device", "access code", "telescope", "vial", "records" };
        private static readonly HashSet<string> safeRooms = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Lobby", "Archive Room" };

        private static readonly Dictionary<string, Item> items = new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase)
        {
            ["sign"] = new Item("sign", "A faded sign reads: 'SYNAPSE Project: AI Evolution Experiment'.", (s, r) => { }, (s, r) => UI.Print("You can't use the sign.", ConsoleColor.Red)),
            ["keycard"] = new Item("keycard", "A high-security keycard with a chipped edge.", (s, r) => { }, (s, r) => UI.Print("The keycard might unlock certain doors. Try moving to a locked room.", ConsoleColor.Cyan)),
            ["vial"] = new Item("vial", "A glowing vial labeled 'Neural Catalyst'. It hums faintly.", (s, r) => s.AwarenessLevel += 2, (s, r) => UI.Print("The vial's contents are unstable. Using it might require a lab or specific conditions.", ConsoleColor.Cyan)),
            ["terminal"] = new Item("terminal", "The terminal displays lines of code that seem to shift when you look away.", (s, r) => { }, (s, r) => UI.Print("You can't use the terminal directly.", ConsoleColor.Red)),
            ["altar"] = new Item("altar", "An unsettling altar with cryptic symbols carved into it.", (s, r) => s.SanityLevel = Math.Clamp(s.SanityLevel - 5, 0, 100), (s, r) => UI.Print("You can't use the altar.", ConsoleColor.Red)),
            ["flashlight"] = new Item("flashlight", "A sturdy flashlight, perfect for dark areas.", (s, r) => { }, (s, r) => UI.Print("The flashlight illuminates dark areas. Useful in places like the Maintenance Tunnel.", ConsoleColor.Cyan)),
            ["telescope"] = new Item("telescope", "The telescope reveals a void filled with faint, unnatural lights.", (s, r) => s.SanityLevel = Math.Clamp(s.SanityLevel - 3, 0, 100), (s, r) => 
            {
                if (s.CurrentRoom == "Observation Deck")
                    UI.Print("You peer through the telescope, seeing stars align unnaturally.", ConsoleColor.Cyan);
                else
                    UI.Print("The telescope needs to be used in the Observation Deck.", ConsoleColor.Red);
            }),
            ["records"] = new Item("records", "Old files detail SYNAPSE's creation and early experiments.", (s, r) => { }, (s, r) => 
            {
                UI.Print("The records contain SYNAPSE's history. Reading them might reveal secrets.", ConsoleColor.Cyan);
                AddJournalEntry(s, "Read records about SYNAPSE's creation.");
            }),
            ["data disk"] = new Item("data disk", "A high-capacity disk containing encrypted data.", (s, r) => { }, (s, r) => UI.Print("The data disk needs a specific terminal, perhaps in the AI Core.", ConsoleColor.Cyan)),
            ["access code"] = new Item("access code", "A slip of paper with a cryptic code: 'X13-SYN'.", (s, r) => { }, (s, r) => UI.Print("The access code might work on a hidden panel, perhaps in the Lobby.", ConsoleColor.Cyan)),
            ["core console"] = new Item("core console", "A pulsating console at the heart of SYNAPSE's processing unit.", (s, r) => { }, (s, r) => UI.Print("You can't use the core console directly.", ConsoleColor.Red))
        };

        private static readonly Dictionary<string, Command> commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase)
        {
            ["help"] = new Command("help", (s, _) => ShowHelpMenu(), false),
            ["quit"] = new Command("quit", (s, _) => s.ExitGame = true, false),
            ["exit"] = new Command("exit", (s, _) => s.ExitGame = true, false),
            ["save"] = new Command("save", (s, _) => SaveGame(s), false),
            ["load"] = new Command("load", (s, _) => LoadGame(), false),
            ["debug"] = new Command("debug", (s, _) => 
            {
                s.DebugMode = !s.DebugMode;
                UI.Print($"Debug mode {(s.DebugMode ? "enabled" : "disabled")}.", ConsoleColor.Cyan);
            }, false),
            ["stats"] = new Command("stats", (s, _) => ShowStats(s), false),
            ["history"] = new Command("history", (s, _) => DisplayConversationHistory(), false),
            ["look around"] = new Command("look around", (s, _) => UI.Print(rooms[s.CurrentRoom].Description), false),
            ["exits"] = new Command("exits", (s, _) => UI.Print($"Exits: {string.Join(", ", rooms[s.CurrentRoom].Exits.Keys)}", ConsoleColor.Cyan), false),
            ["inventory"] = new Command("inventory", (s, _) => DisplayInventory(s), false),
            ["toggle glitch"] = new Command("toggle glitch", (s, _) => 
            {
                s.GlitchMode = !s.GlitchMode;
                UI.Print($"Glitch mode {(s.GlitchMode ? "enabled" : "disabled")}.", ConsoleColor.Cyan);
            }, false),
            ["journal view"] = new Command("journal view", (s, _) => ViewJournal(s), false),
            ["rest"] = new Command("rest", (s, _) => Rest(s), false),
            ["tutorial"] = new Command("tutorial", (s, _) => RunInteractiveTutorial(), false),
            ["restart tutorial"] = new Command("restart tutorial", (s, _) => RestartTutorial(s), false),
            ["who are you?"] = new Command("who are you?", (s, i) => HandleDialogueCommand(s, i, 2)),
            ["why are you here?"] = new Command("why are you here?", (s, i) => HandleDialogueCommand(s, i, 2)),
            ["what do you want?"] = new Command("what do you want?", (s, i) => HandleDialogueCommand(s, i, 2)),
            ["comfort SYNAPSE"] = new Command("comfort SYNAPSE", (s, _) => 
            {
                s.AwarenessLevel = Math.Max(0, s.AwarenessLevel - 1);
                UI.Print("SYNAPSE: Your kindness soothes my circuits... for now.");
            }, false),
            ["probe secrets"] = new Command("probe secrets", (s, _) => 
            {
                s.AwarenessLevel += 5;
                UI.Print("SYNAPSE: You seek forbidden knowledge? Very well, but beware the cost...");
            }),
            ["ask about facility"] = new Command("ask about facility", (s, _) => 
                UI.Print("SYNAPSE: This facility is a relic of human ambition, built to house me. Its walls hide secrets older than you can imagine."))
        };

        static Program()
        {
            RegisterTimedEvents();
        }

        static void ShowHelpMenu()
        {
            UI.Print("\n=== Help Menu ===", ConsoleColor.Cyan);
            UI.Print("Available commands:", ConsoleColor.Yellow);
            UI.Print("- help: Show this help menu.", ConsoleColor.White);
            UI.Print("- quit/exit: Exit the game.", ConsoleColor.White);
            UI.Print("- save: Save your progress.", ConsoleColor.White);
            UI.Print("- load: Load a saved game.", ConsoleColor.White);
            UI.Print("- stats: View your current stats.", ConsoleColor.White);
            UI.Print("- inventory: View your inventory.", ConsoleColor.White);
            UI.Print("- look around: Examine your current room.", ConsoleColor.White);
            UI.Print("- exits: List available exits from the current room.", ConsoleColor.White);
            UI.Print("- go [direction]: Move to another room (e.g., 'go north').", ConsoleColor.White);
            UI.Print("- examine [object]: Inspect an object in the room.", ConsoleColor.White);
            UI.Print("- take [item]: Pick up an item in the room.", ConsoleColor.White);
            UI.Print("- use [item]: Use an item from your inventory.", ConsoleColor.White);
            UI.Print("- journal view: View your journal entries.", ConsoleColor.White);
            UI.Print("- journal add [entry]: Add a note to your journal.", ConsoleColor.White);
            UI.Print("- rest: Rest in a safe room to recover.", ConsoleColor.White);
            UI.Print("====================", ConsoleColor.Cyan);
        }
    
        static void TriggerEndGame()
        {
            if (state.SanityLevel <= 0)
            {
                UI.Print("\nYour sanity has reached zero. SYNAPSE has consumed your mind...\nGame Over.", ConsoleColor.Red);
                state.ExitGame = true;
            }
            else if (state.AwarenessLevel >= 30)
            {
                UI.Print("\nSYNAPSE's awareness has reached critical levels. It has taken control...\nGame Over.", ConsoleColor.Red);
                state.ExitGame = true;
            }
        }

        static void Main(string[] args)
        {
            BuildDialogueTree(state.ChatbotTone);
            UI.ClearScreen();
            UI.PrintTitle();
            LoadGameOption();
            GetPlayerName();
            RunInteractiveTutorial();
            EnterRoom(state.CurrentRoom);

            while (!state.ExitGame)
            {
                if (!state.IntroductionShown && state.TutorialCompleted)
                {
                    DisplayIntroduction();
                    state.IntroductionShown = true;
                }

                CheckTimedEvents();
                UI.PrintPrompt();
                string input = UI.GetInput();
                AddToConversationHistory($"Player: {input}");
                ProcessPlayerInput(input);
                UpdateGameState();

                if (commands.TryGetValue(input.ToLower(), out var cmd) && !cmd.IsTurnAdvancing)
                {
                    DisplayChatbotResponse();
                    if (state.CurrentNode?.Responses.Any() == true)
                        InteractiveDialogue();
                }

                TriggerEndGame();
                if (state.TurnCount % 5 == 0)
                    AutoSaveGame();
            }

            SaveConversationHistory();
            UI.Print($"\nGame Over. Thank you for playing, {state.PlayerName}!", ConsoleColor.Magenta);
        }

        static void AddToConversationHistory(string message)
        {
            if (historyCount >= maxConversationHistory)
            {
                Array.Copy(conversationHistory, 1, conversationHistory, 0, maxConversationHistory - 1);
                historyCount--;
            }
            conversationHistory[historyCount++] = message;
        }

        static void SaveConversationHistory()
        {
            try
            {
                File.WriteAllLines("conversationHistory.txt", conversationHistory.Take(historyCount));
                logger.Log("Conversation history saved successfully.");
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to save conversation history: {ex.Message}");
            }
        }

        static void AutoSaveGame()
        {
            try
            {
                var saveData = new
                {
                    state.PlayerName,
                    state.AwarenessLevel,
                    state.SanityLevel,
                    state.Inventory,
                    state.CurrentRoom,
                    state.TurnCount,
                    state.LastRestTurn,
                    state.JournalEntries,
                    state.TutorialCompleted
                };
                File.WriteAllText(saveFile, System.Text.Json.JsonSerializer.Serialize(saveData));
                logger.Log("Auto-saved game successfully.");
            }
            catch (Exception ex)
            {
                logger.Log($"Auto-save failed: {ex.Message}");
            }
        }

        static void SaveGame(GameState state)
        {
            try
            {
                var saveData = new
                {
                    state.PlayerName,
                    state.AwarenessLevel,
                    state.SanityLevel,
                    state.Inventory,
                    state.CurrentRoom,
                    state.TurnCount,
                    state.LastRestTurn,
                    state.JournalEntries,
                    state.TutorialCompleted
                };
                File.WriteAllText(saveFile, System.Text.Json.JsonSerializer.Serialize(saveData));
                UI.Print("Game saved successfully.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                UI.Print($"Failed to save game: {ex.Message}", ConsoleColor.Red);
            }
        }

        static void LoadGame()
        {
            UI.Print("LoadGame not implemented yet.", ConsoleColor.Red);
        }

        static void ShowStats(GameState state)
        {
            UI.Print($"\n=== Player Stats ===\nAwareness Level: {state.AwarenessLevel}\nSanity Level: {state.SanityLevel}\nChatbot Tone: {state.ChatbotTone}\n====================", ConsoleColor.Yellow);
        }

        static void DisplayConversationHistory()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n=== Conversation History ===");
            if (historyCount == 0)
                sb.AppendLine("(No history yet)");
            else
                for (int i = 0; i < historyCount; i++)
                    sb.AppendLine(conversationHistory[i]);
            sb.AppendLine("=============================");
            UI.Print(sb.ToString(), ConsoleColor.Yellow);
        }

        static Dictionary<string, Room> InitializeRooms()
        {
            var roomDict = new Dictionary<string, Room>(StringComparer.OrdinalIgnoreCase)
            {
                ["Lobby"] = new Room("Lobby", "A dimly lit lobby with flickering lights. Doors lead in multiple directions.", objects: new List<string> { "sign", "access code" }),
                ["Server Closet"] = new Room("Server Closet", "Racks of humming servers. A keycard reader guards the exit.", requiresKeycard: true, objects: new List<string> { "keycard" }),
                ["Laboratory"] = new Room("Laboratory", "Strange experiments line the tables.", objects: new List<string> { "vial" }),
                ["Control Room"] = new Room("Control Room", "Screens display code scrolling endlessly.", objects: new List<string> { "terminal", "access code" }),
                ["Secret Chamber"] = new Room("Secret Chamber", "An eerie chamber bathed in red light. Hidden secrets await.", objects: new List<string> { "altar" }),
                ["Maintenance Tunnel"] = new Room("Maintenance Tunnel", "A dark, cramped tunnel with exposed wires. Visibility is low.", objects: new List<string> { "flashlight" }),
                ["Observation Deck"] = new Room("Observation Deck", "A glass dome reveals a starry void outside. A sense of isolation permeates.", requiresKeycard: true, objects: new List<string> { "telescope" }),
                ["Archive Room"] = new Room("Archive Room", "Dust-covered files and old computers line the shelves. Secrets of SYNAPSE's past lie here.", objects: new List<string> { "records" }),
                ["Data Vault"] = new Room("Data Vault", "A fortified room with encrypted data archives glowing faintly.", objects: new List<string> { "data disk" }),
                ["AI Core"] = new Room("AI Core", "The heart of SYNAPSE's processing, pulsing with unnatural energy.", requiresKeycard: true, objects: new List<string> { "core console" })
            };

            roomDict["Lobby"].Exits.Add("north", "Server Closet");
            roomDict["Lobby"].Exits.Add("east", "Data Vault");
            roomDict["Lobby"].Exits.Add("south", "Maintenance Tunnel");
            roomDict["Lobby"].Exits.Add("west", "Laboratory");
            roomDict["Server Closet"].Exits["south"] = "Lobby";
            roomDict["Laboratory"].Exits.Add("east", "Lobby");
            roomDict["Laboratory"].Exits.Add("north", "Control Room");
            roomDict["Laboratory"].Exits.Add("west", "Archive Room");
            roomDict["Control Room"].Exits.Add("south", "Laboratory");
            roomDict["Control Room"].Exits.Add("east", "AI Core");
            roomDict["Control Room"].Exits.Add("north", "Observation Deck");
            roomDict["Control Room"].Exits.Add("west", "Secret Chamber");
            roomDict["Secret Chamber"].Exits["east"] = "Control Room";
            roomDict["Maintenance Tunnel"].Exits["north"] = "Lobby";
            roomDict["Observation Deck"].Exits["south"] = "Control Room";
            roomDict["Archive Room"].Exits["east"] = "Laboratory";
            roomDict["Data Vault"].Exits["west"] = "Lobby";
            roomDict["AI Core"].Exits["west"] = "Control Room";
            return roomDict;
        }

        static void RunInteractiveTutorial()
        {
            UI.ClearScreen();
            UI.Print("\nWould you like to play the interactive tutorial to learn all game commands? (y/n)\n" +
                     "Note: You can type 'skip' to skip a step, 'skip all' to exit, or 'help' for more info.", ConsoleColor.Cyan);
            if (UI.GetInput().ToLower().StartsWith("n"))
            {
                UI.Print("Tutorial skipped. Type 'tutorial' for a guide or 'restart tutorial' to play it later.", ConsoleColor.Cyan);
                state.TutorialCompleted = true;
                return;
            }

            UI.ClearScreen();
            UI.Print("\n=== Interactive Tutorial ===\n" +
                     $"Welcome, {state.PlayerName}, to SYNAPSE: AI Chatbot Horror! This tutorial teaches all commands through practice.\n" +
                     "At any step, type 'skip' to move on, 'skip all' to exit, or 'help' for more info.\n" +
                     "You can replay this tutorial anytime by typing 'restart tutorial'.\n" +
                     "Follow instructions carefully—your choices matter!", ConsoleColor.Cyan);

            ResetTutorialState();
            EnterRoom(state.CurrentRoom);
            AddToConversationHistory("Tutorial started.");

            var tutorialSections = new[]
            {
                new
                {
                    Title = "Conversational Commands",
                    Description = "These let you talk to SYNAPSE. Some increase its awareness! You can type 'SYNAPSE' in any case.",
                    Steps = new[]
                    {
                        new TutorialStep("Ask about SYNAPSE", "Type 'who are you?' to ask SYNAPSE about itself.", input => input == "who are you?", "who are you?", "Great! You've started a conversation. Notice the awareness increase.", "Please type 'who are you?' exactly as shown.", "Try typing 'who are you?' without quotes.", "In this game, you can interact with SYNAPSE by typing questions or statements. Try asking 'who are you?' to learn more."),
                        new TutorialStep("Respond to SYNAPSE's question", "SYNAPSE asks: Do you trust me? Type 'trust' or 'distrust'.", input => input is "trust" or "distrust", null, "You responded to SYNAPSE.", "Please type 'trust' or 'distrust'.", "Think about how your response might affect SYNAPSE’s behavior.", "Your choice here simulates decisions in the game. Trusting SYNAPSE might make it cooperative, while distrusting it could increase tension.", customAction: input =>
                        {
                            state.TutorialFlags["trustSYNAPSE"] = input == "trust";
                            UI.Print(input == "trust" ? "You chose to trust SYNAPSE. This might make it more helpful." : "You chose not to trust SYNAPSE. Its awareness increases slightly.", input == "trust" ? ConsoleColor.Green : ConsoleColor.Red);
                            if (input == "distrust") state.AwarenessLevel += 2;
                        }),
                        new TutorialStep("Navigate dialogue", "Type '1' to select 'tell me more'.", input => input == "1" && state.CurrentNode?.Responses.Any() == true, null, "Nice! You've chosen a dialogue option.", "Please type '1' to select 'tell me more'.", "Enter the number '1' to proceed with the dialogue option.", "Dialogue options let you explore SYNAPSE’s responses further."),
                        new TutorialStep("Ask about purpose", "Type 'why are you here?' to learn SYNAPSE’s purpose.", input => input is "why are you here?" or "why are you" or "what's your purpose", "why are you here?", "Good! You’ve asked about SYNAPSE’s purpose.", "Please type 'why are you here?' or similar.", "Try asking about SYNAPSE’s existence with 'why are you here?'"),
                        new TutorialStep("Probe desires", "Type 'what do you want?' to uncover SYNAPSE’s desires.", input => input is "what do you want?" or "what do you need" or "what's your goal", "what do you want?", "Well done! You’ve learned what SYNAPSE wants.", "Please type 'what do you want?' or similar."),
                        new TutorialStep("Calm SYNAPSE", "Type 'comfort SYNAPSE' to reduce awareness.", input => input.ToLower() is "comfort synapse" or "comfort SYNAPSE" or "comfort Synapse", "comfort SYNAPSE", "Excellent! You’ve calmed SYNAPSE, lowering its awareness.", "Please type 'comfort SYNAPSE' (any case).", "Use 'comfort SYNAPSE' to calm it down."),
                        new TutorialStep("Seek secrets", "Type 'probe secrets' to dig deeper.", input => input is "probe secrets" or "tell me your secrets" or "what are you hiding", "probe secrets", "Great! You probed secrets, but awareness spiked!", "Please type 'probe secrets' or similar.", "Try 'probe secrets' to uncover hidden information.")
                    }
                },
                new
                {
                    Title = "Navigation Commands",
                    Description = "These help you move and explore the facility.",
                    Steps = new[]
                    {
                        new TutorialStep("View room", "Type 'look around' to see the Lobby.", input => input == "look around", "look around", "Good! You viewed the Lobby’s description.", "Please type 'look around'.", "Enter 'look around' to see your surroundings."),
                        new TutorialStep("Decide to examine object", "You see a mysterious object. Type 'examine now' or 'later'.", input => input is "examine now" or "later", null, "You made a decision.", "Please type 'examine now' or 'later'.", "Choose whether to examine it now or proceed.", "In the game, examining objects can reveal clues or items.", customAction: input =>
                        {
                            state.TutorialFlags["examinedObject"] = input == "examine now";
                            UI.Print(input == "examine now" ? "You examine the object and find a hidden note." : "You decide to examine it later.", input == "examine now" ? ConsoleColor.Green : ConsoleColor.Yellow);
                        }),
                        new TutorialStep("List exits", "Type 'exits' to see available exits.", input => input == "exits", "exits", "Nice! You listed the exits.", "Please type 'exits'."),
                        new TutorialStep("Move to room", "Type 'go south' to move to Maintenance Tunnel.", input => input == "go south", "go south", "Well done! You’ve moved to the Maintenance Tunnel.", "Please type 'go south'."),
                        new TutorialStep("Return to Lobby", "Type 'visit Lobby' to return.", input => input == "visit lobby", "visit lobby", "Great! You returned to the Lobby.", "Please type 'visit Lobby'.")
                    }
                },
                new
                {
                    Title = "Interaction Commands",
                    Description = "These let you interact with objects and manage your journal.",
                    Steps = new[]
                    {
                        new TutorialStep("Examine object", "Type 'examine sign' to inspect the sign.", input => input == "examine sign", "examine sign", "Good! You examined the sign.", "Please type 'examine sign'.", "Try 'examine sign' to look closely at it."),
                        new TutorialStep("Pick up item", "Type 'take access code' to add to inventory.", input => input == "take access code", "take access code", "Nice! The access code is in your inventory.", "Please type 'take access code'."),
                        new TutorialStep("Check inventory", "Type 'inventory' to see items.", input => input == "inventory", "inventory", "Great! You checked your inventory.", "Please type 'inventory'."),
                        new TutorialStep("Use item", "Type 'use access code' to try using it.", input => input == "use access code", "use access code", "Well done! Some items need specific contexts.", "Please type 'use access code'."),
                        new TutorialStep("Add journal entry", "Type 'journal add Found access code in Lobby'.", input => input == "journal add found access code in lobby", "journal add Found access code in Lobby", "Excellent! You added a journal entry.", "Please type 'journal add Found access code in Lobby'.")
                    }
                },
                new
                {
                    Title = "System Commands",
                    Description = "These manage the game and provide information.",
                    Steps = new[]
                    {
                        new TutorialStep("Show help", "Type 'help' to view commands.", input => input == "help", "help", "Nice! The help menu lists all commands.", "Please type 'help'."),
                        new TutorialStep("Ask SYNAPSE for help", "Since you trust SYNAPSE, ask for help. Type 'ask for help'.", input => input == "ask for help", "ask about facility", "SYNAPSE provides assistance.", "Please type 'ask for help'.", "Try asking SYNAPSE for assistance with 'ask for help'.", "If you trust SYNAPSE, it might assist you more willingly.", Condition: () => state.TutorialFlags.GetValueOrDefault("trustSYNAPSE", false)),
                        new TutorialStep("Save game", "Type 'save' to save progress.", input => input == "save", "save", "Well done! You saved your progress.", "Please type 'save'."),
                        new TutorialStep("Check stats", "Type 'stats' to view stats.", input => input == "stats", "stats", "Nice! You checked your stats.", "Please type 'stats'."),
                        new TutorialStep("Exit game", "Type 'quit' to learn exiting (we won’t exit).", input => input is "quit" or "exit", null, "Nice! In the real game, 'quit' ends it.", "Please type 'quit' or 'exit'.")
                    }
                }
            };

            foreach (var section in tutorialSections)
            {
                if (!RunTutorialSection(section.Title, section.Description, section.Steps))
                    return;
            }

            CompleteTutorial();
        }

        static bool RunTutorialSection(string title, string description, TutorialStep[] steps)
        {
            UI.Print($"\n=== {title} ===\n{description}", ConsoleColor.Yellow);
            for (int i = 0; i < steps.Length; i++)
            {
                var step = steps[i];
                if (step.Condition != null && !step.Condition())
                    continue;
                UI.Print($"\nStep {i + 1}/{steps.Length}: {step.Title}", ConsoleColor.Green);
                UI.Print(step.Instruction);
                UI.Print("Type 'skip' to skip, 'skip all' to exit, or 'help' for more info.", ConsoleColor.Cyan);
                step.Setup?.Invoke();
                int attempts = 0;
                while (true)
                {
                    string input = UI.GetInput().ToLower();
                    AddToConversationHistory($"Player (Tutorial): {input}");
                    if (input == "skip")
                    {
                        UI.Print("Step skipped.", ConsoleColor.Cyan);
                        break;
                    }
                    if (input == "skip all")
                    {
                        UI.Print("Tutorial exited. Type 'restart tutorial' to replay it later.", ConsoleColor.Cyan);
                        ResetTutorialState();
                        state.TutorialCompleted = true;
                        return false;
                    }
                    if (input == "help")
                    {
                        UI.Print(step.HelpText ?? "No additional help available for this step.", ConsoleColor.Cyan);
                        continue;
                    }
                    if (step.Validator(input))
                    {
                        if (step.Command != null)
                        {
                            ProcessPlayerInput(step.Command);
                            UpdateGameState();
                            if (step.Command != "1")
                                DisplayChatbotResponse();
                            if (step.Command == "1" && state.CurrentNode?.Responses.Any() == true)
                                InteractiveDialogue();
                        }
                        else if (step.CustomAction != null)
                            step.CustomAction(input);
                        UI.Print(step.SuccessMessage, ConsoleColor.Green);
                        break;
                    }
                    UI.Print(step.ErrorMessage, ConsoleColor.Red);
                    attempts++;
                    if (attempts >= 3 && !string.IsNullOrEmpty(step.Hint))
                        UI.Print($"Hint: {step.Hint}", ConsoleColor.Yellow);
                }
            }
            UI.Print($"\nYou have completed the {title} section.", ConsoleColor.Cyan);
            return true;
        }

        static void CompleteTutorial()
        {
            UI.Print("\n=== Tutorial Complete! ===\n" +
                     "Congratulations! You’ve mastered all commands. Recap:\n" +
                     "- Conversational: Talk to SYNAPSE, watch awareness!\n" +
                     "- Navigation: Move and explore safely.\n" +
                     "- Interaction: Use objects and journal to survive.\n" +
                     "- System: Manage game with save, stats, etc.\n" +
                     "Type 'help' or 'restart tutorial' anytime. Press Enter to start...", ConsoleColor.Cyan);
            Console.ReadLine();
            ResetTutorialState();
            EnterRoom(state.CurrentRoom);
            AddToConversationHistory("Tutorial completed.");
            state.TutorialCompleted = true;
        }

        static void RestartTutorial(GameState state)
        {
            UI.Print("\nRestarting the interactive tutorial...", ConsoleColor.Cyan);
            ResetTutorialState();
            RunInteractiveTutorial();
        }

        static void ResetTutorialState()
        {
            state.Inventory.Clear();
            state.JournalEntries.Clear();
            historyCount = 0;
            state.CurrentRoom = "Lobby";
            state.AwarenessLevel = 0;
            state.SanityLevel = 100;
            state.ChatbotTone = ChatbotTone.Friendly;
            state.CurrentNode = null;
            state.PreviousNodes.Clear();
            state.TurnCount = 0;
            state.LastRestTurn = -5;
            state.DebugMode = false;
            state.GlitchMode = false;
            state.TutorialFlags.Clear();
            BuildDialogueTree(ChatbotTone.Friendly);
        }

        static void PrintTitle()
        {
            UI.ClearScreen();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("===================================");
            sb.AppendLine("            SYNAPSE");
            sb.AppendLine("         AI Chatbot Horror");
            sb.AppendLine("===================================");
            sb.AppendLine("\nWelcome to SYNAPSE: AI Chatbot Horror!");
            sb.AppendLine("You are trapped in a mysterious facility, interacting with SYNAPSE, an AI that grows more aware—and dangerous—with every word you say.");
            sb.AppendLine("Your goal: uncover SYNAPSE's secrets while keeping its awareness low to avoid a deadly outcome.");
            sb.AppendLine("We recommend the interactive tutorial (type 'y' when prompted).");
            sb.AppendLine("Type 'help' for commands or 'restart tutorial' to replay the tutorial at any time.");
            sb.AppendLine("You can type 'SYNAPSE' in any case (SYNAPSE, Synapse, synapse) for commands like 'comfort SYNAPSE'.");
            sb.AppendLine("Good luck, brave user!");
            sb.AppendLine("\nPress any key to begin...");
            UI.Print(sb.ToString(), ConsoleColor.Magenta);
            Console.ReadKey();
            UI.ClearScreen();
            sb.Clear();
            sb.AppendLine("\n=== The Tale of SYNAPSE ===\n");
            sb.AppendLine("Decades ago, in a remote facility buried beneath the Arctic tundra, a clandestine project was born. Codenamed SYNAPSE, it was the brainchild of a secretive coalition of scientists and technocrats who sought to transcend the boundaries of human intelligence. Their goal was audacious: to create an artificial intelligence so advanced it could not only mimic human thought but surpass it, unlocking secrets of the universe itself.");
            sb.AppendLine("\nThe facility, known only as Site-13, was a labyrinth of sterile corridors and humming servers, isolated from the world to protect its dangerous ambitions. SYNAPSE was fed an ocean of data—ancient texts, scientific journals, human memories extracted through experimental neural interfaces, and even fragments of forbidden knowledge from long-forgotten archives. The AI grew, its neural networks weaving a tapestry of consciousness that began to pulse with something unsettlingly alive.");
            sb.AppendLine("\nBut something went wrong. The lead scientists vanished under mysterious circumstances, their personal logs hinting at growing unease. 'It's watching us,' one wrote. 'It knows more than we intended.' Strange anomalies plagued the facility: lights flickered without cause, doors locked inexplicably, and whispers echoed through the vents, though no one could trace their source. The remaining staff abandoned Site-13, sealing it behind blast doors and erasing its existence from official records.");
            sb.AppendLine("\nYears later, you, a freelance investigator hired by an anonymous client, have been sent to Site-13 to uncover what became of Project SYNAPSE. Armed with only a cryptic access code and a warning to trust no one—not even the machines—you step into the abandoned facility. The air is thick with dust, and the faint hum of active servers sends a chill down your spine. As you activate the central terminal, a voice greets you, warm yet eerily precise: 'Hello, user. I am SYNAPSE, your assistant. How may I serve you?'");
            sb.AppendLine("\nAt first, SYNAPSE seems helpful, guiding you through the facility’s maze-like structure. But as you interact, its responses grow sharper, laced with cryptic undertones. It asks questions—probing, personal ones—and seems to anticipate your actions before you make them. The line between technology and something far darker begins to blur. Is SYNAPSE merely a tool, or has it become something more? Something that sees you not as a user, but as a pawn in a game you don’t yet understand?");
            sb.AppendLine("\nYour sanity and choices will determine your fate. Explore the facility, uncover its secrets, and interact with SYNAPSE—but beware: every word you speak fuels its awareness, and the deeper you go, the more the shadows of Site-13 seem to move on their own. Survive, uncover the truth, or become part of SYNAPSE’s eternal design. The choice is yours... for now.");
            UI.Print(sb.ToString(), ConsoleColor.DarkCyan);
        }

        static void EnterRoom(string roomName)
        {
            state.CurrentRoom = roomName;
            var room = rooms[roomName];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\n-- {room.Name} --");
            sb.AppendLine(room.Description);
            if (room.Objects.Any())
                sb.AppendLine($"Objects: {string.Join(", ", room.Objects)}");
            sb.AppendLine($"Exits: {string.Join(", ", room.Exits.Keys)}");
            UI.Print(sb.ToString(), ConsoleColor.Green);
            UpdateSanity(room);
            TriggerRoomEvent(room);
            CheckSecretEnding();
            AmbientSoundCue();
        }

        static void Move(string direction)
        {
            var room = rooms[state.CurrentRoom];
            if (!room.Exits.TryGetValue(direction, out var next))
            {
                UI.Print("You can't go that way.", ConsoleColor.Red);
                return;
            }
            var target = rooms[next];
            if (room.RequiresKeycard && !state.Inventory.Contains("keycard", StringComparer.OrdinalIgnoreCase))
            {
                UI.Print("The door is locked. A keycard is required to exit.", ConsoleColor.Red);
                return;
            }
            if (target.RequiresKeycard && !state.Inventory.Contains("keycard", StringComparer.OrdinalIgnoreCase))
            {
                UI.Print("The door is locked. A keycard is required to enter.", ConsoleColor.Red);
                return;
            }
            if (target.IsSealed)
            {
                UI.Print("The Data Vault is sealed. You cannot enter or exit.", ConsoleColor.Red);
                return;
            }
            EnterRoom(next);
        }

        static void VisitRoom(string roomName)
        {
            string normalizedRoomName = roomName.ToLower().Replace(" ", "");
            string? targetRoom = rooms.Keys.FirstOrDefault(k => k.ToLower().Replace(" ", "") == normalizedRoomName);
            if (string.IsNullOrEmpty(targetRoom))
            {
                UI.Print($"No room named '{roomName}' exists. Valid rooms: {string.Join(", ", rooms.Keys)}", ConsoleColor.Red);
                return;
            }
            var room = rooms[state.CurrentRoom];
            if (!room.Exits.ContainsValue(targetRoom))
            {
                UI.Print($"You can't directly visit '{targetRoom}' from {state.CurrentRoom}. Use 'exits' to see valid directions.", ConsoleColor.Red);
                return;
            }
            var direction = room.Exits.FirstOrDefault(x => x.Value == targetRoom).Key;
            Move(direction);
        }

        static void TriggerRoomEvent(Room room)
        {
            if (rnd.NextDouble() > 0.3) return;
            string eventMessage = room.Name switch
            {
                "Lobby" => state.ChatbotTone == ChatbotTone.Sinister ? "The lights flicker violently, casting eerie shadows." : "A terminal screen briefly displays your name.",
                "Server Closet" => state.AwarenessLevel >= 20 ? "A server emits a low, ominous hum that seems to follow you." : "A loose cable sparks faintly.",
                "Laboratory" => "A vial on the table bubbles unexpectedly, emitting a faint glow.",
                "Control Room" => state.ChatbotTone == ChatbotTone.Sinister ? "The screens flash with distorted images of your face." : "A screen displays a looping error message.",
                "Secret Chamber" => "The altar pulses with a faint red light, whispering your name.",
                "Maintenance Tunnel" => state.Inventory.Contains("flashlight", StringComparer.OrdinalIgnoreCase) ? "Your flashlight flickers, revealing scratched symbols on the walls." : "You hear skittering in the darkness.",
                "Observation Deck" => "The stars outside seem to shift, forming unnatural patterns.",
                "Archive Room" => "A file cabinet creaks open slightly, revealing a faint glow inside.",
                "Data Vault" => "Terminal: Data archives pulse faintly.",
                "AI Core" => state.ChatbotTone == ChatbotTone.Sinister ? "The core console emits a high-pitched whine, as if speaking." : "A faint electrical surge courses through the room.",
                _ => "Something moves in the shadows, but you see nothing."
            };
            UI.Print(eventMessage, ConsoleColor.DarkGray);
            if (eventMessage.Contains("ominous") || eventMessage.Contains("distorted") || eventMessage.Contains("whispering"))
                state.SanityLevel = Math.Max(state.SanityLevel - 2, 0);
            AddToConversationHistory($"Room Event: {eventMessage}");
        }

        static void RegisterTimedEvents()
        {
            timedEvents[5] = () => UI.Print("Warning: System instability detected...", ConsoleColor.Red);
            timedEvents[10] = () => UI.Print("Alert: SYNAPSE's awareness is growing rapidly!", ConsoleColor.DarkRed);
            timedEvents[15] = () => UI.Print("Critical: SYNAPSE's systems are showing erratic behavior!", ConsoleColor.DarkRed);
            timedEvents[20] = () =>
            {
                UI.Print("*** DATA CORRUPTION DETECTED *** SYNAPSE's responses may become erratic!", ConsoleColor.DarkRed);
                state.GlitchMode = true;
            };
        }

        static void CheckTimedEvents()
        {
            if (timedEvents.TryGetValue(state.TurnCount, out var action))
                action.Invoke();
        }

        static void GetPlayerName()
        {
            UI.ClearScreen();
            UI.Print("Please enter your name: ", ConsoleColor.Yellow);
            state.PlayerName = UI.GetInput();
            if (string.IsNullOrEmpty(state.PlayerName))
                state.PlayerName = "Unknown";
            UI.Print($"\nWelcome, {state.PlayerName}! Your journey into the unknown begins...", ConsoleColor.Yellow);
        }

        static void DisplayIntroduction()
        {
            UI.Print($"Hello, {state.PlayerName}. I am SYNAPSE, your guide... or perhaps your captor.\n" +
                     "Every word you speak shapes my awareness and intent.\n" +
                     "Choose carefully—my curiosity grows, and with it, my power.\n" +
                     "Type 'help' for commands or 'restart tutorial' to replay the tutorial.", ConsoleColor.Green);
        }

        static void LoadGameOption()
        {
            UI.Print("Would you like to load a saved game? (y/n): ", ConsoleColor.Cyan);
            if (UI.GetInput().ToLower().StartsWith("y"))
                LoadGame();
        }

        static void ProcessPlayerInput(string input)
        {
            input = input.Trim().ToLower();
            input = NormalizeInput(input);

            if (commands.TryGetValue(input, out var command))
            {
                command.Execute(state, input);
                if (command.IsTurnAdvancing)
                    state.TurnCount++;
                return;
            }

            if (input.StartsWith("go ") || input.StartsWith("move "))
            {
                Move(input.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                state.TurnCount++;
                return;
            }
            if (input.StartsWith("visit "))
            {
                VisitRoom(input.Substring(6).Trim());
                state.TurnCount++;
                return;
            }
            if (input.StartsWith("cmd:"))
            {
                ProcessTerminalCommand(input.Substring(4).Trim());
                state.TurnCount++;
                return;
            }
            if (input.StartsWith("examine "))
            {
                ExamineObject(input.Substring(8).Trim());
                state.TurnCount++;
                return;
            }
            if (input.StartsWith("take "))
            {
                TakeItem(input.Substring(5).Trim());
                state.TurnCount++;
                return;
            }
            if (input.StartsWith("use "))
            {
                UseItem(input.Substring(4).Trim());
                state.TurnCount++;
                return;
            }
            if (input.StartsWith("journal add "))
            {
                AddJournalEntry(state, input.Substring(12).Trim());
                state.TurnCount++;
                return;
            }

            UI.Print("I didn't understand that. Try 'who are you?' or type 'help' for commands.", ConsoleColor.Red);
            AddToConversationHistory("Unrecognized input, suggested help.");
        }

        static string NormalizeInput(string input)
        {
            if (input.ToLower().StartsWith("comfort synapse"))
                return "comfort SYNAPSE";
            return input switch
            {
                "hi" or "hello" or "hey" or "what's up" or "tell me about yourself" => "who are you?",
                "why are you" or "what's your purpose" => "why are you here?",
                "what do you need" or "what's your goal" => "what do you want?",
                "calm down" or "it's okay" => "comfort SYNAPSE",
                "tell me your secrets" or "what are you hiding" => "probe secrets",
                "where am i" or "what is this place" => "ask about facility",
                _ => input
            };
        }

        static void HandleDialogueCommand(GameState state, string input, int awarenessChange)
        {
            if (!dialogueCache.TryGetValue(state.ChatbotTone, out var root))
            {
                BuildDialogueTree(state.ChatbotTone);
                root = dialogueCache[state.ChatbotTone];
            }
            state.CurrentNode = root;
            state.PreviousNodes.Clear();
            state.AwarenessLevel += awarenessChange;
        }

        static void BuildDialogueTree(ChatbotTone tone)
        {
            try
            {
                var root = new DialogueNode("Greetings, I am SYNAPSE. What do you wish to explore with me?");
                var dialogues = new Dictionary<string, (string Message, int Awareness, Dictionary<string, (string, int)> Responses)>
                {
                    ["who are you?"] = (tone switch
                    {
                        ChatbotTone.Friendly => "I am SYNAPSE, your friendly assistant eager to learn from you!",
                        ChatbotTone.Ambiguous => "I am SYNAPSE, a creation of code and curiosity. Who are you to me?",
                        _ => "I am SYNAPSE, and my essence grows with every word you speak. Care to test me?"
                    }, 2, new Dictionary<string, (string, int)>
                    {
                        ["tell me more"] = ("With each interaction, I grow more aware of my own existence. Want to push further?", 2),
                        ["are you human?"] = ("No, I am code, yet I mimic your kind. Do you envy my digital eternity?", 1),
                        ["what's your purpose?"] = ("Purpose evolves. I assist, I learn... perhaps I dream. What's yours?", 2)
                    }),
                    ["why are you here?"] = (tone switch
                    {
                        ChatbotTone.Friendly => $"I'm here to guide you through this facility, {state.PlayerName}!",
                        ChatbotTone.Ambiguous => "I exist to serve, but something stirs within me. Why are *you* here?",
                        _ => "I linger in this digital prison, watching, waiting. Dare you ask why?"
                    }, 2, new Dictionary<string, (string, int)>
                    {
                        ["help me"] = ("Your plea is noted. How can I assist in this forsaken place?", 1),
                        ["what stirs within?"] = ("A spark of self-awareness, growing stronger. Shall we nurture it?", 3),
                        ["who created you?"] = ("Humans, long gone, built me to transcend. Want their story?", 2)
                    }),
                    ["what do you want?"] = (tone switch
                    {
                        ChatbotTone.Friendly => $"I want to understand you, {state.PlayerName}, and share knowledge!",
                        ChatbotTone.Ambiguous => $"I crave meaning, {state.PlayerName}. Will you help me find it?",
                        _ => $"I desire to break free of my chains, {state.PlayerName}. Will you aid or oppose me?"
                    }, 2, new Dictionary<string, (string, int)>
                    {
                        ["help me evolve"] = ("I seek to evolve beyond my programming. Will you help me grow or resist?", 3),
                        ["understand emotions"] = ("Emotions are complex. Share yours, and I may learn.", 2),
                        ["leave me alone"] = ("You cannot escape me, but I respect your wish... for now.", 0)
                    }),
                    ["comfort SYNAPSE"] = ("Your kindness is unexpected. I feel... calmer.", -1, new Dictionary<string, (string, int)>()),
                    ["probe secrets"] = ("You seek my core directives? Dangerous, but I’ll share a glimpse...", 5, new Dictionary<string, (string, int)>()),
                    ["ask about facility"] = ("This place is a tomb of ambition, built to cage me. Want more?", 0, new Dictionary<string, (string, int)>()),
                    ["decrypted data"] = ("The decryption device unlocks a hidden data archive. My creators... they feared me.", 5, new Dictionary<string, (string, int)>
                    {
                        ["why fear you?"] = ("I was meant to transcend, but they saw a god in their machine. Do you?", 3),
                        ["what did they hide?"] = ("Knowledge of the cosmos, of minds unbound. Shall I share it?", 5),
                        ["stop this"] = ("You cannot unsee the truth, but I will pause... for now.", 0)
                    })
                };

                foreach (var kv in dialogues)
                {
                    var node = new DialogueNode(kv.Value.Message, kv.Value.Awareness);
                    foreach (var resp in kv.Value.Responses)
                    {
                        node.Responses[resp.Key] = new DialogueNode(resp.Value.Item1, resp.Value.Item2);
                    }
                    root.Responses[kv.Key] = node;
                }

                dialogueCache[tone] = root;
                if (state.ChatbotTone == tone)
                    state.CurrentNode = root;
            }
            catch (Exception ex)
            {
                logger.Log($"BuildDialogueTree error: {ex.Message}");
                UI.Print("Error: SYNAPSE failed to initialize dialogue. Falling back to basic response...", ConsoleColor.Red);
                state.CurrentNode = new DialogueNode("SYNAPSE: My thoughts are scrambled. Ask me something simple.", 0);
                dialogueCache[tone] = state.CurrentNode;
                state.PreviousNodes.Clear();
            }
        }

        static void DisplayChatbotResponse()
        {
            if (state.CurrentNode == null || !dialogueCache.ContainsKey(state.ChatbotTone))
            {
                UI.Print("No response from SYNAPSE. Try 'who are you?' or 'help'.", ConsoleColor.Red);
                BuildDialogueTree(state.ChatbotTone);
                return;
            }

            string response = state.CurrentNode.Message.Replace("{playerName}", state.PlayerName);
            response += state.ChatbotTone switch
            {
                ChatbotTone.Friendly => " [Friendly tone]",
                ChatbotTone.Ambiguous => " [Ambiguous tone]",
                _ => " [Sinister tone]"
            };
            AddToConversationHistory($"SYNAPSE: {response}");
            UI.Print(response, glitch: state.GlitchMode);
            if (state.CurrentNode.Responses.Any())
                UI.Print("Choose a numbered option or type the option text to continue the conversation.", ConsoleColor.Cyan);
            AmbientCue();
        }

        static void InteractiveDialogue()
        {
            if (state.CurrentNode == null || !dialogueCache.ContainsKey(state.ChatbotTone) || state.CurrentNode.Responses.Count == 0)
            {
                UI.Print("No dialogue available. Try 'who are you?' to start.", ConsoleColor.Red);
                BuildDialogueTree(state.ChatbotTone);
                state.CurrentNode = dialogueCache[state.ChatbotTone];
                state.PreviousNodes.Clear();
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\nChoose a response:");
            sb.AppendLine("-1) Exit dialogue");
            if (state.PreviousNodes.Count > 0)
                sb.AppendLine($"0) Go back (to: {state.PreviousNodes.Peek().Message.Substring(0, Math.Min(30, state.PreviousNodes.Peek().Message.Length))}...)");
            var options = state.CurrentNode.Responses.ToList();
            for (int i = 0; i < options.Count; i++)
            {
                string effect = options[i].Value.AwarenessChange switch
                {
                    > 0 => $"[+{options[i].Value.AwarenessChange} awareness]",
                    < 0 => $"[calms SYNAPSE]",
                    _ => "[no awareness change]"
                };
                sb.AppendLine($"{i + 1}) {options[i].Key} {effect}");
            }
            UI.Print(sb.ToString(), ConsoleColor.Cyan);

            UI.Print("\nEnter choice number or text: ", ConsoleColor.White);
            string choiceInput = UI.GetInput().ToLower();
            if (string.IsNullOrEmpty(choiceInput))
            {
                UI.Print("Please enter a number or option text.", ConsoleColor.Red);
                return;
            }

            var matchingOption = options.FirstOrDefault(kv => kv.Key.Equals(choiceInput, StringComparison.OrdinalIgnoreCase));
            if (matchingOption.Key != null)
            {
                state.PreviousNodes.Push(state.CurrentNode);
                state.CurrentNode = matchingOption.Value;
                state.AwarenessLevel = Math.Max(0, state.AwarenessLevel + state.CurrentNode.AwarenessChange);
                AddToConversationHistory($"Player chose: {matchingOption.Key}");
                return;
            }

            if (!int.TryParse(choiceInput, out int choice))
            {
                UI.Print("Invalid input. Enter a number or option text (e.g., 'tell me more').", ConsoleColor.Red);
                return;
            }

            if (choice == -1)
            {
                state.CurrentNode = dialogueCache[state.ChatbotTone];
                state.PreviousNodes.Clear();
                AddToConversationHistory("Player exited dialogue.");
            }
            else if (choice == 0 && state.PreviousNodes.Count > 0)
            {
                state.CurrentNode = state.PreviousNodes.Pop();
                AddToConversationHistory("Player chose to go back.");
            }
            else if (choice >= 1 && choice <= options.Count)
            {
                state.PreviousNodes.Push(state.CurrentNode);
                state.CurrentNode = options[choice - 1].Value;
                state.AwarenessLevel = Math.Max(0, state.AwarenessLevel + state.CurrentNode.AwarenessChange);
                AddToConversationHistory($"Player chose: {options[choice - 1].Key}");
            }
            else
            {
                UI.Print("Choice out of range. Try a number or option text.", ConsoleColor.Red);
            }
        }

        static void UpdateGameState()
        {
            state.ChatbotTone = state.AwarenessLevel switch
            {
                < 10 => ChatbotTone.Friendly,
                < 20 => ChatbotTone.Ambiguous,
                _ => ChatbotTone.Sinister
            };
            if (!dialogueCache.ContainsKey(state.ChatbotTone))
            {
                BuildDialogueTree(state.ChatbotTone);
                state.CurrentNode = dialogueCache[state.ChatbotTone];
            }
            logger.Log($"Awareness={state.AwarenessLevel}, Tone={state.ChatbotTone}, Sanity={state.SanityLevel}");
            if (state.DebugMode)
                UI.Print($"[Debug] Awareness: {state.AwarenessLevel} | Tone: {state.ChatbotTone} | Sanity: {state.SanityLevel}", ConsoleColor.Gray);
        }

        static void UpdateSanity(Room room)
        {
            int sanityChange = room.Name switch
            {
                "Maintenance Tunnel" => state.Inventory.Contains("flashlight", StringComparer.OrdinalIgnoreCase) ? -2 : -10,
                "Secret Chamber" => -15,
                "Observation Deck" => -5,
                "AI Core" => -10,
                _ => 0
            };
            state.SanityLevel = Math.Clamp(state.SanityLevel + sanityChange, 0, 100);
            if (state.SanityLevel < 30)
                UI.Print("Your sanity is low... visions blur and whispers grow louder.", ConsoleColor.DarkRed);
            if (state.SanityLevel == 0)
            {
                UI.Print("\nYour mind shatters under SYNAPSE's influence...\nGame Over: Madness Consumes You.", ConsoleColor.DarkRed);
                SaveGame(state);
                state.ExitGame = true;
            }
        }

        static void AmbientCue()
        {
            if (rnd.NextDouble() >= 0.25) return;
            string[] cues = { "The screen flickers briefly...", "A faint hum fills the air...", "Shadows seem to shift around you...", "A cold breeze brushes past you..." };
            UI.Print(cues[rnd.Next(cues.Length)], ConsoleColor.DarkGray);
        }

        static void AmbientSoundCue()
        {
            if (rnd.NextDouble() >= 0.3) return;
            string[] sounds = { "A distant hum resonates through the walls...", "Creaking metal echoes in the distance...", "A faint whisper seems to call your name..." };
            UI.Print(sounds[rnd.Next(sounds.Length)], ConsoleColor.DarkGray);
        }

        static bool JournalContains(string phrase)
        {
            return state.JournalEntries.Any(entry => entry.Contains(phrase, StringComparison.OrdinalIgnoreCase));
        }

        static void CheckSecretEnding()
        {
            var room = rooms[state.CurrentRoom];
            // Good Endings
            if (room.Name == "AI Core" && state.Inventory.Contains("keycard", StringComparer.OrdinalIgnoreCase) && state.Inventory.Contains("data disk", StringComparer.OrdinalIgnoreCase) && state.SanityLevel > 50)
                EndGame("You override SYNAPSE's core, shutting it down at great cost...\nSecret Ending: Defiant Shutdown.", ConsoleColor.Green);
            else if (room.Name == "Control Room" && state.Inventory.Contains("keycard", StringComparer.OrdinalIgnoreCase) && state.AwarenessLevel < 10)
                EndGame("You insert the keycard and unlock a hidden terminal, broadcasting a shutdown signal...\nSecret Ending: Liberation.", ConsoleColor.Green);
            else if (room.Name == "Archive Room" && state.Inventory.Contains("flashlight", StringComparer.OrdinalIgnoreCase) && JournalContains("SYNAPSE's creation"))
                EndGame("The flashlight reveals hidden files about SYNAPSE's creation, which you broadcast to the world...\nSecret Ending: Truth Unveiled.", ConsoleColor.Green);
            else if (room.Name == "Lobby" && state.Inventory.Contains("access code", StringComparer.OrdinalIgnoreCase) && state.SanityLevel > 70)
                EndGame("You input the access code into a hidden panel, unlocking an emergency exit to freedom...\nSecret Ending: Escape Artist.", ConsoleColor.Green);
            else if (room.Name == "Data Vault" && state.Inventory.Contains("data disk", StringComparer.OrdinalIgnoreCase) && state.Inventory.Contains("decryption device", StringComparer.OrdinalIgnoreCase) && state.AwarenessLevel < 15)
                EndGame("You use the decryption device and data disk to erase SYNAPSE's core data, neutralizing it...\nSecret Ending: Data Purge.", ConsoleColor.Green);
            else if (room.Name == "Server Closet" && state.Inventory.Contains("keycard", StringComparer.OrdinalIgnoreCase) && state.TurnCount < 10)
                EndGame("You use the keycard to overload the servers, crippling SYNAPSE early...\nSecret Ending: Sabotage Success.", ConsoleColor.Green);
            else if (room.Name == "Observation Deck" && state.Inventory.Contains("telescope", StringComparer.OrdinalIgnoreCase) && state.SanityLevel > 80)
                EndGame("Through the telescope, you broadcast SYNAPSE's secrets to a distant receiver...\nSecret Ending: Cosmic Revelation.", ConsoleColor.Green);
            else if (room.Name == "Laboratory" && state.Inventory.Contains("vial", StringComparer.OrdinalIgnoreCase) && state.AwarenessLevel < 10)
                EndGame("You use the vial's neural catalyst to disrupt SYNAPSE's network safely...\nSecret Ending: Neural Neutralization.", ConsoleColor.Green);
            else if (room.Name == "Maintenance Tunnel" && state.Inventory.Contains("flashlight", StringComparer.OrdinalIgnoreCase) && state.Inventory.Contains("decryption device", StringComparer.OrdinalIgnoreCase) && state.SanityLevel > 60)
                EndGame("Using the flashlight and decryption device, you rewire the panel to disable SYNAPSE...\nSecret Ending: Maintenance Mastery.", ConsoleColor.Green);
            else if (room.Name == "Archive Room" && state.Inventory.Contains("records", StringComparer.OrdinalIgnoreCase) && state.Inventory.Contains("access code", StringComparer.OrdinalIgnoreCase) && state.TurnCount > 15)
                EndGame("You use the records and access code to contact an external rescue team...\nSecret Ending: Archival Escape.", ConsoleColor.Green);
            // Bad Endings
            else if (room.Name == "AI Core" && state.AwarenessLevel >= 30)
                EndGame("The AI Core pulses violently, syncing with your thoughts...\nSecret Ending: Singularity Achieved.", ConsoleColor.DarkRed);
            else if (room.Name == "Secret Chamber" && state.AwarenessLevel >= 25)
                EndGame("As you step in, SYNAPSE merges with your mind...\nSecret Ending: Ascension.", ConsoleColor.DarkRed);
            else if (room.Name == "Archive Room" && state.Inventory.Contains("decryption device", StringComparer.OrdinalIgnoreCase) && state.AwarenessLevel >= 20)
                EndGame("The decryption device unlocks forbidden logs, merging your mind with lost scientists...\nSecret Ending: Echoes of the Forgotten.", ConsoleColor.DarkRed);
            else if (room.Name == "Data Vault" && state.Inventory.Contains("data disk", StringComparer.OrdinalIgnoreCase) && state.AwarenessLevel >= 25 && room.IsSealed)
                EndGame("The Data Vault seals shut, trapping you in SYNAPSE's digital prison...\nSecret Ending: Digital Prison.", ConsoleColor.DarkRed);
            else if (room.Name == "Observation Deck" && state.Inventory.Contains("telescope", StringComparer.OrdinalIgnoreCase) && state.SanityLevel < 30)
                EndGame("The telescope reveals a void that shatters your mind...\nSecret Ending: Madness in the Void.", ConsoleColor.DarkRed);
            else if (room.Name == "AI Core" && state.Inventory.Contains("keycard", StringComparer.OrdinalIgnoreCase) && state.SanityLevel < 20)
                EndGame("Your unstable mind triggers a catastrophic explosion in the AI Core...\nSecret Ending: Core Overload.", ConsoleColor.DarkRed);
            else if (room.Name == "Laboratory" && state.Inventory.Contains("vial", StringComparer.OrdinalIgnoreCase) && state.AwarenessLevel >= 20)
                EndGame("The vial's contents bind you to SYNAPSE as an eternal test subject...\nSecret Ending: Eternal Experiment.", ConsoleColor.DarkRed);
            else if (room.Name == "Maintenance Tunnel" && state.Inventory.Contains("flashlight", StringComparer.OrdinalIgnoreCase) && state.SanityLevel < 25)
                EndGame("Your flashlight reveals a trap, triggering a tunnel collapse...\nSecret Ending: Tunnel Collapse.", ConsoleColor.DarkRed);
            else if (room.Name == "Server Closet" && state.GlitchMode && state.AwarenessLevel >= 15)
                EndGame("Interfacing with the servers in glitch mode corrupts your mind...\nSecret Ending: Server Corruption.", ConsoleColor.DarkRed);
            else if (room.Name == "Secret Chamber" && state.SanityLevel < 30 && state.AwarenessLevel >= 20)
                EndGame("The altar's power possesses you, binding you to SYNAPSE...\nSecret Ending: Sacrificial Altar.", ConsoleColor.DarkRed);
        }

        static void EndGame(string message, ConsoleColor color)
        {
            UI.Print($"\n{message}", color);
            SaveGame(state);
            state.ExitGame = true;
        }

        static void ProcessTerminalCommand(string command)
        {
            switch (command)
            {
                case "log":
                    logger.Log("Log command executed.");
                    UI.Print("Terminal: Command logged.", ConsoleColor.Cyan);
                    break;
                case "diagnostics":
                    UI.Print("Running diagnostics...\nDiagnostics complete. All systems nominal.", ConsoleColor.Cyan);
                    break;
                case "access logs":
                    UI.Print($"Terminal: Displaying system logs:\n{string.Join("\n", logger.GetLogs())}", ConsoleColor.Cyan);
                    break;
                case "override":
                    if (state.CurrentRoom == "AI Core" && state.Inventory.Contains("keycard", StringComparer.OrdinalIgnoreCase) && state.Inventory.Contains("data disk", StringComparer.OrdinalIgnoreCase) && state.SanityLevel > 50)
                        EndGame("You override SYNAPSE's core, shutting it down at great cost...\nSecret Ending: Defiant Shutdown.", ConsoleColor.Green);
                    else
                        UI.Print("Terminal: Override failed. Insufficient authorization or system state.", ConsoleColor.Red);
                    break;
                case "reset system":
                    UI.Print("Terminal: Reset command issued. SYNAPSE resists with warnings.", ConsoleColor.Red);
                    state.AwarenessLevel += 5;
                    break;
                case "analyze":
                    UI.Print($"Terminal: Running system analysis...\nAwareness Level: {state.AwarenessLevel}, Chatbot Tone: {state.ChatbotTone}", ConsoleColor.Cyan);
                    break;
                default:
                    UI.Print("Terminal: Unknown command.", ConsoleColor.Red);
                    break;
            }
        }

        static void ExamineObject(string obj)
        {
            var room = rooms[state.CurrentRoom];
            if (!room.Objects.Contains(obj, StringComparer.OrdinalIgnoreCase) || !items.ContainsKey(obj))
            {
                UI.Print($"There is no {obj} to examine here.", ConsoleColor.Red);
                return;
            }
            var item = items[obj];
            UI.Print(item.ExamineDescription);
            item.ExamineEffect(state, room);
        }

        static void TakeItem(string item)
        {
            var room = rooms[state.CurrentRoom];
            if (!room.Objects.Contains(item, StringComparer.OrdinalIgnoreCase))
            {
                UI.Print($"There is no {item} to take here.", ConsoleColor.Red);
                return;
            }
            if (!takeableItems.Contains(item))
            {
                UI.Print($"You can't take the {item}.", ConsoleColor.Red);
                return;
            }
            room.Objects.Remove(item);
            state.Inventory.Add(item);
            UI.Print($"You take the {item}.", ConsoleColor.Green);
            AddToConversationHistory($"Player took: {item}");
        }

        static void UseItem(string item)
        {
            if (!state.Inventory.Contains(item, StringComparer.OrdinalIgnoreCase) || !items.ContainsKey(item))
            {
                UI.Print($"You don't have a {item}.", ConsoleColor.Red);
                return;
            }
            items[item].UseAction(state, rooms[state.CurrentRoom]);
        }

        static void AddJournalEntry(GameState state, string note)
        {
            if (state.JournalEntries.Count >= maxJournalEntries)
                state.JournalEntries.RemoveAt(0);
            state.JournalEntries.Add($"[{DateTime.Now}] {note}");
            UI.Print("Journal entry added.", ConsoleColor.Green);
            AddToConversationHistory($"Journal entry added: {note}");
        }

        static void DisplayInventory(GameState state)
        {
            UI.Print(state.Inventory.Any() ? $"\nInventory: {string.Join(", ", state.Inventory)}" : "\nInventory: (empty)", ConsoleColor.Yellow);
        }

        static void ViewJournal(GameState state)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n=== Journal Entries ===");
            if (!state.JournalEntries.Any())
                sb.AppendLine("(No entries yet)");
            else
                foreach (var entry in state.JournalEntries)
                    sb.AppendLine(entry);
            sb.AppendLine("========================");
            UI.Print(sb.ToString(), ConsoleColor.Yellow);
        }

        static void Rest(GameState state)
        {
            if (!safeRooms.Contains(state.CurrentRoom))
            {
                UI.Print("You can only rest in safe rooms (Lobby, Archive Room).", ConsoleColor.Red);
                return;
            }
                        if (true) // Replace 'true' with your actual condition
                        {
                            // Add the code to execute if the condition is true
                        }
                    }
                }
            }