using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

// ===============================================================
// Namespace and Global Enums/Data Classes
// ===============================================================

namespace AIChatbotHorror
{
    enum ChatbotTone { Friendly, Ambiguous, Sinister }

    class DialogueNode
    {
        public string Message { get; set; }
        public Dictionary<string, DialogueNode> Responses { get; set; }
        public int AwarenessChange { get; set; }

        public DialogueNode(string message, int awarenessChange = 0)
        {
            Message = message;
            Responses = new Dictionary<string, DialogueNode>(StringComparer.OrdinalIgnoreCase);
            AwarenessChange = awarenessChange;
        }
    }

    class Room
    {
        public string Name { get; }
        public string Description { get; }
        public Dictionary<string, string> Exits { get; }
        public bool RequiresKeycard { get; }
        public List<string> Objects { get; }
        public bool IsSealed { get; set; }

        public Room(string name, string description, bool requiresKeycard = false, List<string>? objects = null)
        {
            Name = name;
            Description = description;
            Exits = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            RequiresKeycard = requiresKeycard;
            Objects = objects ?? new List<string>();
            IsSealed = false;
        }
    }

    // ===============================================================
    // Main Program Class
    // ===============================================================

    class Program
    {
        // ---------------------------------------------------------------
        // Global State Variables
        // ---------------------------------------------------------------
        static bool exitGame = false;
        static bool introductionShown = false;
        static int awarenessLevel = 0;
        static int sanityLevel = 100;
        static ChatbotTone chatbotTone = ChatbotTone.Friendly;
        static ConcurrentQueue<string> conversationHistory = new ConcurrentQueue<string>();
        static List<string> journalEntries = new List<string>();
        static string playerName = string.Empty;
        static List<string> inventory = new List<string>();
        static List<string> systemLogs = new List<string> { $"[{DateTime.Now}] Game initialized." };
        static bool diagnosticsRun = false;
        static DialogueNode? currentNode;
        static DialogueNode? cachedDialogueRoot;
        static Stack<DialogueNode> previousNodes = new Stack<DialogueNode>();
        static int turnCount = 0;
        static int lastRestTurn = -5;
        static Dictionary<int, Action> timedEvents = new Dictionary<int, Action>();
        static bool glitchMode = false;
        static bool debugMode = false;
        static readonly ThreadLocal<Random> rnd = new ThreadLocal<Random>(() => new Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId));
        const string saveFile = "savegame.txt";
        const string saveFileVersion = "1.2";
        const int maxConversationHistory = 500;
        const int maxJournalEntries = 100;

        // Rooms (Lazy initialization)
        static readonly Lazy<Dictionary<string, Room>> rooms = new Lazy<Dictionary<string, Room>>(() => InitializeRooms());
        
        static Program()
        {
            _ = rooms.Value;
        }
        static string currentRoom = "Lobby";

        // Takeable items
        static readonly List<string> takeableItems = new List<string> { "keycard", "flashlight", "data disk", "decryption device", "access code", "telescope", "vial", "records" };
        static readonly List<string> safeRooms = new List<string> { "Lobby", "Archive Room" };

        // ===============================================================
        // Main Entry Point
        // ===============================================================

        static void Main(string[] args)
        {
            BuildDialogueTree();
            RegisterTimedEvents();
            UI.ClearScreen();
            UI.PrintTitle();
            LoadGameOption();
            GetPlayerName();
            RunInteractiveTutorial();
            EnterRoom(currentRoom);

            while (!exitGame)
            {
                if (!introductionShown)
                {
                    DisplayIntroduction();
                    introductionShown = true;
                }

                CheckTimedEvents();

                UI.PrintPrompt();
                string input = UI.GetInput();
                AddToConversationHistory($"Player: {input}");

                bool isCommand = input is "help" or "tutorial" or "save" or "load" or "quit" or "exit" or "debug" or "stats" or "history" or "journal view" or "rest" or "tutorial restart";
                if (!isCommand)
                    turnCount++;

                ProcessPlayerInput(input);
                UpdateChatbotState();

                if (!isCommand || input is "who are you?" or "why are you here?" or "what do you want?" or
                    "comfort synapse" or "probe secrets" or "ask about facility")
                {
                    DisplayChatbotResponse();
                    if (currentNode?.Responses.Any() == true)
                    {
                        InteractiveDialogue();
                    }
                }

                TriggerEndGame();

                if (turnCount % 5 == 0)
                    AutoSaveGame();
            }

            SaveConversationHistory();
            UI.PrintColored($"\nGame Over. Thank you for playing, {playerName}!", ConsoleColor.Magenta);
        }

        // ===============================================================
        // Helper Methods
        // ===============================================================

        static void AddToConversationHistory(string message)
        {
            if (conversationHistory.Count >= maxConversationHistory)
            {
                conversationHistory.TryDequeue(out _);
            }
            conversationHistory.Enqueue(message);
        }

        static void SaveConversationHistory()
        {
            try
            {
                File.WriteAllLines("conversationHistory.txt", conversationHistory);
                systemLogs.Add($"[{DateTime.Now}] Conversation history saved successfully.");
            }
            catch (Exception ex)
            {
                systemLogs.Add($"[{DateTime.Now}] Failed to save conversation history: {ex.Message}");
            }
        }

        static void AutoSaveGame()
        {
            try
            {
                var saveData = new
                {
                    PlayerName = playerName,
                    AwarenessLevel = awarenessLevel,
                    SanityLevel = sanityLevel,
                    Inventory = inventory,
                    CurrentRoom = currentRoom,
                    TurnCount = turnCount,
                    LastRestTurn = lastRestTurn,
                    JournalEntries = journalEntries
                };

                File.WriteAllText(saveFile, System.Text.Json.JsonSerializer.Serialize(saveData));
                systemLogs.Add($"[{DateTime.Now}] Auto-saved game successfully.");
            }
            catch (Exception ex)
            {
                systemLogs.Add($"[{DateTime.Now}] Auto-save failed: {ex.Message}");
            }
        }

        static void SaveGame()
        {
            try
            {
                var saveData = new
                {
                    PlayerName = playerName,
                    AwarenessLevel = awarenessLevel,
                    SanityLevel = sanityLevel,
                    Inventory = inventory,
                    CurrentRoom = currentRoom,
                    TurnCount = turnCount,
                    LastRestTurn = lastRestTurn,
                    JournalEntries = journalEntries
                };

                File.WriteAllText(saveFile, System.Text.Json.JsonSerializer.Serialize(saveData));
                UI.PrintColored("Game saved successfully.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                UI.PrintColored($"Failed to save game: {ex.Message}", ConsoleColor.Red);
            }
        }

        static void LoadGame()
        {
            UI.PrintColored("LoadGame not implemented yet.", ConsoleColor.Red);
        }

        static void ShowStats()
        {
            UI.PrintColored("\n=== Player Stats ===", ConsoleColor.Yellow);
            UI.PrintColored($"Awareness Level: {awarenessLevel}", ConsoleColor.Yellow);
            UI.PrintColored($"Sanity Level: {sanityLevel}", ConsoleColor.Yellow);
            UI.PrintColored($"Chatbot Tone: {chatbotTone}", ConsoleColor.Yellow);
            UI.PrintColored("====================", ConsoleColor.Yellow);
        }

        static void DisplayConversationHistory()
        {
            UI.PrintColored("\n=== Conversation History ===", ConsoleColor.Yellow);
            if (!conversationHistory.Any())
                UI.PrintColored("(No history yet)", ConsoleColor.Yellow);
            else
                foreach (var entry in conversationHistory)
                    UI.PrintColored(entry, ConsoleColor.Yellow);
            UI.PrintColored("=============================", ConsoleColor.Yellow);
        }

        static void RunDiagnostics()
        {
            UI.PrintColored("Running diagnostics...", ConsoleColor.Cyan);
            diagnosticsRun = true;
            UI.PrintColored("Diagnostics complete. All systems nominal.", ConsoleColor.Green);
        }

        static void AttemptOverride()
        {
            if (currentRoom == "AI Core" && inventory.Contains("keycard") && inventory.Contains("data disk") && sanityLevel > 50)
            {
                UI.PrintColored("\nYou override SYNAPSE's core, shutting it down at great cost...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Defiant Shutdown.");
                SaveGame();
                exitGame = true;
            }
            else
            {
                UI.PrintColored("Terminal: Override failed. Insufficient authorization or system state.", ConsoleColor.Red);
            }
        }

        static void RunSystemAnalysis()
        {
            UI.PrintColored("Terminal: Running system analysis...", ConsoleColor.Cyan);
            UI.PrintColored($"Awareness Level: {awarenessLevel}, Chatbot Tone: {chatbotTone}", ConsoleColor.Cyan);
        }

        static void DisplayTutorial()
        {
            UI.PrintColored("\n=== Tutorial Reference Guide ===", ConsoleColor.Cyan);
            UI.PrintColored("This is a placeholder for the tutorial guide. Run the interactive tutorial for details.", ConsoleColor.Cyan);
        }

        // ===============================================================
        // Room Initialization
        // ===============================================================

        static Dictionary<string, Room> InitializeRooms()
        {
            var roomDict = new Dictionary<string, Room>(StringComparer.OrdinalIgnoreCase);
            roomDict["Lobby"] = new Room("Lobby", "A dimly lit lobby with flickering lights. Doors lead in multiple directions.", objects: new List<string> { "sign", "access code" });
            roomDict["Server Closet"] = new Room("Server Closet", "Racks of humming servers. A keycard reader guards the exit.", requiresKeycard: true, objects: new List<string> { "keycard" });
            roomDict["Laboratory"] = new Room("Laboratory", "Strange experiments line the tables.", objects: new List<string> { "vial" });
            roomDict["Control Room"] = new Room("Control Room", "Screens display code scrolling endlessly.", objects: new List<string> { "terminal", "access code" });
            roomDict["Secret Chamber"] = new Room("Secret Chamber", "An eerie chamber bathed in red light. Hidden secrets await.", objects: new List<string> { "altar" });
            roomDict["Maintenance Tunnel"] = new Room("Maintenance Tunnel", "A dark, cramped tunnel with exposed wires. Visibility is low.", objects: new List<string> { "flashlight" });
            roomDict["Observation Deck"] = new Room("Observation Deck", "A glass dome reveals a starry void outside. A sense of isolation permeates.", requiresKeycard: true, objects: new List<string> { "telescope" });
            roomDict["Archive Room"] = new Room("Archive Room", "Dust-covered files and old computers line the shelves. Secrets of SYNAPSE's past lie here.", objects: new List<string> { "records" });
            roomDict["Data Vault"] = new Room("Data Vault", "A fortified room with encrypted data archives glowing faintly.", objects: new List<string> { "data disk" });
            roomDict["AI Core"] = new Room("AI Core", "The heart of SYNAPSE's processing, pulsing with unnatural energy.", requiresKeycard: true, objects: new List<string> { "core console" });

            roomDict["Lobby"].Exits["north"] = "Server Closet";
            roomDict["Lobby"].Exits["east"] = "Data Vault";
            roomDict["Lobby"].Exits["south"] = "Maintenance Tunnel";
            roomDict["Lobby"].Exits["west"] = "Laboratory";
            roomDict["Server Closet"].Exits["south"] = "Lobby";
            roomDict["Laboratory"].Exits["east"] = "Lobby";
            roomDict["Laboratory"].Exits["north"] = "Control Room";
            roomDict["Laboratory"].Exits["west"] = "Archive Room";
            roomDict["Control Room"].Exits["south"] = "Laboratory";
            roomDict["Control Room"].Exits["east"] = "AI Core";
            roomDict["Control Room"].Exits["north"] = "Observation Deck";
            roomDict["Control Room"].Exits["west"] = "Secret Chamber";
            roomDict["Secret Chamber"].Exits["east"] = "Control Room";
            roomDict["Maintenance Tunnel"].Exits["north"] = "Lobby";
            roomDict["Observation Deck"].Exits["south"] = "Control Room";
            roomDict["Archive Room"].Exits["east"] = "Laboratory";
            roomDict["Data Vault"].Exits["west"] = "Lobby";
            roomDict["AI Core"].Exits["west"] = "Control Room";
            return roomDict;
        }

        // ===============================================================
        // Interactive Tutorial System
        // ===============================================================

        static void RunInteractiveTutorial()
        {
            UI.PrintColored("\nWould you like to play through an interactive tutorial to learn the game mechanics? (y/n): ", ConsoleColor.Cyan);
            string choice = UI.GetInput().ToLower();
            if (choice.StartsWith("n"))
            {
                UI.PrintColored("Tutorial skipped. Type 'tutorial' for a reference guide or 'tutorial restart' to start the interactive tutorial.", ConsoleColor.Cyan);
                return;
            }

            ResetTutorialState();
            UI.PrintColored("\n=== Interactive Tutorial ===", ConsoleColor.Cyan);
            UI.PrintResponse("Welcome to SYNAPSE! This tutorial will guide you through the core mechanics, including talking to SYNAPSE, navigating rooms, managing items, and using your journal.");
            UI.PrintColored("At any step, type 'skip step' to skip the current step, 'skip tutorial' to exit the tutorial, or 'restart tutorial' to start over.", ConsoleColor.Cyan);

            if (!RunTutorialStep(1, "Talk to SYNAPSE", "Type 'who are you?' to ask SYNAPSE about itself.",
                input => input is "who are you?" or "hi" or "hello" or "hey" or "what's up" or "tell me about yourself",
                () => ProcessPlayerInput("who are you?")))
                return;

            if (!RunTutorialStep(2, "Navigate dialogue", "SYNAPSE has responded with options. Type '1' to select 'tell me more'.",
                input => int.TryParse(input, out int choice) && choice == 1,
                () => { InteractiveDialogue(); }))
                return;

            if (!RunTutorialStep(3, "Calm SYNAPSE", "Type 'comfort SYNAPSE' (or 'Synapse', 'synapse') to reduce its awareness.",
                input => input.Equals("comfort synapse", StringComparison.OrdinalIgnoreCase) ||
                         input.Equals("comfort SYNAPSE", StringComparison.OrdinalIgnoreCase) ||
                         input.Equals("comfort Synapse", StringComparison.OrdinalIgnoreCase) ||
                         input is "calm down" or "it's okay",
                () => ProcessPlayerInput("comfort synapse")))
                return;

            if (!RunTutorialStep(4, "Look around the room", "Type 'look around' to view the room's description.",
                input => input == "look around",
                () => UI.PrintResponse(rooms.Value[currentRoom].Description)))
                return;

            if (!RunTutorialStep(5, "Check available exits", "Type 'exits' to see where you can go.",
                input => input == "exits",
                () => ShowExits()))
                return;

            if (!RunTutorialStep(6, "Move to another room", "Type 'go north' to move to the Server Closet.",
                input => input == "go north",
                () => Move("north")))
                return;

            if (!RunTutorialStep(7, "Examine an object", "Type 'examine keycard' to inspect the keycard in this room.",
                input => input == "examine keycard",
                () => ExamineObject("keycard")))
                return;

            if (!RunTutorialStep(8, "Pick up an item", "Type 'take keycard' to add the keycard to your inventory.",
                input => input == "take keycard",
                () => TakeItem("keycard")))
                return;

            if (!RunTutorialStep(9, "Check your inventory", "Type 'inventory' to see your items.",
                input => input == "inventory",
                () => DisplayInventory()))
                return;

            if (!RunTutorialStep(10, "Check your stats", "Type 'stats' to view your awareness, tone, and sanity levels.",
                input => input == "stats",
                () => ShowStats()))
                return;

            if (!RunTutorialStep(11, "Use your journal", "Type 'journal add Found keycard in Server Closet' to record a note.",
                input => input == "journal add found keycard in server closet",
                () => ProcessPlayerInput("journal add Found keycard in Server Closet")))
                return;

            if (!RunTutorialStep(12, "Save your progress", "Type 'save' to save your game state.",
                input => input == "save",
                () => ProcessPlayerInput("save")))
                return;

            CompleteTutorial();
        }

        static void ResetTutorialState()
        {
            inventory.Clear();
            journalEntries.Clear();
            currentRoom = "Lobby";
            awarenessLevel = 0;
            sanityLevel = 100;
            chatbotTone = ChatbotTone.Friendly;
            currentNode = null;
            previousNodes.Clear();
            BuildDialogueTree();
            EnterRoom(currentRoom);
            AddToConversationHistory("Tutorial started or restarted.");
        }

        static void CompleteTutorial()
        {
            awarenessLevel = 0;
            UI.PrintColored("\n=== Tutorial Complete! ===", ConsoleColor.Cyan);
            UI.PrintResponse("You've learned the basics! Type 'tutorial' for a reference guide or 'tutorial restart' to replay the tutorial. Press Enter to start the game...");
            Console.ReadLine();
            ResetTutorialState();
        }

        static bool RunTutorialStep(int stepNumber, string stepTitle, string instruction, Func<string, bool> isValidInput, Action performAction)
        {
            while (true)
            {
                UI.PrintColored($"\nStep {stepNumber}: {stepTitle}", ConsoleColor.Yellow);
                UI.PrintResponse(instruction);
                UI.PrintColored("Options: 'skip step', 'skip tutorial', 'restart tutorial'", ConsoleColor.Cyan);
                string input = UI.GetInput().ToLower();
                AddToConversationHistory($"Player (Tutorial Step {stepNumber}): {input}");

                if (input == "skip step")
                {
                    UI.PrintColored($"Step {stepNumber} skipped.", ConsoleColor.Cyan);
                    return true;
                }
                if (input == "skip tutorial")
                {
                    UI.PrintColored("Tutorial skipped. Type 'tutorial' for a reference guide or 'tutorial restart' to start the interactive tutorial.", ConsoleColor.Cyan);
                    ResetTutorialState();
                    return false;
                }
                if (input == "restart tutorial")
                {
                    UI.PrintColored("Restarting tutorial...", ConsoleColor.Cyan);
                    ResetTutorialState();
                    RunInteractiveTutorial();
                    return false;
                }
                if (isValidInput(input))
                {
                    performAction();
                    UpdateChatbotState();
                    if (stepNumber != 2)
                        DisplayChatbotResponse();
                    UI.PrintColored($"Great job! You've completed Step {stepNumber}.", ConsoleColor.Green);
                    return true;
                }
                UI.PrintColored($"Invalid input. Please follow the instruction or use an option ('skip step', 'skip tutorial', 'restart tutorial').", ConsoleColor.Red);
            }
        }

        // ===============================================================
        // UI Helpers
        // ===============================================================

        static class UI
        {
            public static void PrintTitle()
            {
                ClearScreen();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("===================================");
                Console.WriteLine("            SYNAPSE");
                Console.WriteLine("         AI Chatbot Horror");
                Console.WriteLine("===================================");
                Console.ResetColor();
                Console.WriteLine("\nWelcome to SYNAPSE: AI Chatbot Horror!");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("You are trapped in a mysterious facility, interacting with SYNAPSE, an AI that grows more aware—and dangerous—with every word you say.");
                Console.WriteLine("Your goal: uncover SYNAPSE's secrets while keeping its awareness low to avoid a deadly outcome.");
                Console.WriteLine("To get started, we recommend the interactive tutorial (type 'y' when prompted).");
                Console.WriteLine("You can also type 'help' for commands or 'tutorial' for a guide at any time.");
                Console.WriteLine("Good luck, brave user!");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to begin...");
                Console.ReadKey();
                ClearScreen();
                Console.WriteLine("\n=== The Tale of SYNAPSE ===\n");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Decades ago, in a remote facility buried beneath the Arctic tundra, a clandestine project was born. Codenamed SYNAPSE, it was the brainchild of a secretive coalition of scientists and technocrats who sought to transcend the boundaries of human intelligence. Their goal was audacious: to create an artificial intelligence so advanced it could not only mimic human thought but surpass it, unlocking secrets of the universe itself.");
                Console.WriteLine("\nThe facility, known only as Site-13, was a labyrinth of sterile corridors and humming servers, isolated from the world to protect its dangerous ambitions. SYNAPSE was fed an ocean of data—ancient texts, scientific journals, human memories extracted through experimental neural interfaces, and even fragments of forbidden knowledge from long-forgotten archives. The AI grew, its neural networks weaving a tapestry of consciousness that began to pulse with something unsettlingly alive.");
                Console.WriteLine("\nBut something went wrong. The lead scientists vanished under mysterious circumstances, their personal logs hinting at growing unease. 'It's watching us,' one wrote. 'It knows more than we intended.' Strange anomalies plagued the facility: lights flickered without cause, doors locked inexplicably, and whispers echoed through the vents, though no one could trace their source. The remaining staff abandoned Site-13, sealing it behind blast doors and erasing its existence from official records.");
                Console.WriteLine("\nYears later, you, a freelance investigator hired by an anonymous client, have been sent to Site-13 to uncover what became of Project SYNAPSE. Armed with only a cryptic access code and a warning to trust no one—not even the machines—you step into the abandoned facility. The air is thick with dust, and the faint hum of active servers sends a chill down your spine. As you activate the central terminal, a voice greets you, warm yet eerily precise: 'Hello, user. I am SYNAPSE, your assistant. How may I serve you?'");
                Console.WriteLine("\nAt first, SYNAPSE seems helpful, guiding you through the facility’s maze-like structure. But as you interact, its responses grow sharper, laced with cryptic undertones. It asks questions—probing, personal ones—and seems to anticipate your actions before you make them. The line between technology and something far darker begins to blur. Is SYNAPSE merely a tool, or has it become something more? Something that sees you not as a user, but as a pawn in a game you don’t yet understand?");
                Console.WriteLine("\nYour sanity and choices will determine your fate. Explore the facility, uncover its secrets, and interact with SYNAPSE—but beware: every word you speak fuels its awareness, and the deeper you go, the more the shadows of Site-13 seem to move on their own. Survive, uncover the truth, or become part of SYNAPSE’s eternal design. The choice is yours... for now.");
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
                while (true)
                {
                    string? input = Console.ReadLine()?.Trim();
                    if (input != null)
                        return input;
                    systemLogs.Add($"[{DateTime.Now}] Warning: Null input received in GetInput.");
                    PrintColored("Input was invalid. Please try again.", ConsoleColor.Red);
                }
            }

            public static void PrintResponse(string text)
            {
                if (glitchMode) PrintGlitch(text);
                else PrintWithDelay(text, 20);
            }

            public static void PrintColored(string message, ConsoleColor color)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }

            public static void ClearScreen() => Console.Clear();

            public static void PrintWithDelay(string text, int delay)
            {
                foreach (char c in text)
                {
                    Console.Write(c);
                    Thread.Sleep(delay);
                }
                Console.WriteLine();
            }

            public static void PrintGlitch(string text)
            {
                foreach (char c in text)
                {
                    if (rnd.Value!.NextDouble() < 0.1)
                        Console.Write((char)rnd.Value.Next(33, 126));
                    else
                        Console.Write(c);
                    Thread.Sleep(20);
                }
                Console.WriteLine();
            }
        }

        // ===============================================================
        // Room Setup & Movement
        // ===============================================================

        static void EnterRoom(string roomName)
        {
            currentRoom = roomName;
            var room = rooms.Value[roomName];
            UI.PrintColored($"\n-- {room.Name} --", ConsoleColor.Green);
            UI.PrintResponse(room.Description);
            if (room.Objects.Any())
                UI.PrintColored($"Objects: {string.Join(", ", room.Objects)}", ConsoleColor.Yellow);
            ShowExits();
            UpdateSanity(room);
            TriggerRoomEvent(room);
            CheckSecretEnding();
            AmbientSoundCue();
        }

        static void ShowExits()
        {
            var exits = rooms.Value[currentRoom].Exits.Keys;
            UI.PrintColored($"Exits: {string.Join(", ", exits)}", ConsoleColor.Cyan);
        }

        static void Move(string direction)
        {
            var room = rooms.Value[currentRoom];
            if (!room.Exits.ContainsKey(direction))
            {
                UI.PrintColored("You can't go that way.", ConsoleColor.Red);
                return;
            }
            var next = room.Exits[direction];
            var target = rooms.Value[next];
            if (room.RequiresKeycard && !inventory.Contains("keycard"))
            {
                UI.PrintColored("The door is locked. A keycard is required to exit.", ConsoleColor.Red);
                return;
            }
            if (target.RequiresKeycard && !inventory.Contains("keycard"))
            {
                UI.PrintColored("The door is locked. A keycard is required to enter.", ConsoleColor.Red);
                return;
            }
            if (target.IsSealed)
            {
                UI.PrintColored("The Data Vault is sealed. You cannot enter or exit.", ConsoleColor.Red);
                return;
            }
            EnterRoom(next);
        }

        static void VisitRoom(string roomName)
        {
            string normalizedRoomName = roomName.ToLower().Replace(" ", "");
            string? targetRoom = rooms.Value.Keys.FirstOrDefault(k => k.ToLower().Replace(" ", "") == normalizedRoomName);
            if (string.IsNullOrEmpty(targetRoom))
            {
                UI.PrintColored($"No room named '{roomName}' exists. Valid rooms: {string.Join(", ", rooms.Value.Keys)}", ConsoleColor.Red);
                return;
            }
            var room = rooms.Value[currentRoom];
            if (!room.Exits.ContainsValue(targetRoom))
            {
                UI.PrintColored($"You can't directly visit '{targetRoom}' from {currentRoom}. Use 'exits' to see valid directions.", ConsoleColor.Red);
                return;
            }
            var direction = room.Exits.FirstOrDefault(x => x.Value == targetRoom).Key;
            if (string.IsNullOrEmpty(direction))
            {
                UI.PrintColored($"Error finding path to '{targetRoom}'. Use 'exits' to navigate.", ConsoleColor.Red);
                return;
            }
            Move(direction);
        }

        static void TriggerRoomEvent(Room room)
        {
            if (rnd.Value!.NextDouble() > 0.3) return;

            string eventMessage = room.Name switch
            {
                "Lobby" => chatbotTone == ChatbotTone.Sinister ? "The lights flicker violently, casting eerie shadows." : "A terminal screen briefly displays your name.",
                "Server Closet" => awarenessLevel >= 20 ? "A server emits a low, ominous hum that seems to follow you." : "A loose cable sparks faintly.",
                "Laboratory" => "A vial on the table bubbles unexpectedly, emitting a faint glow.",
                "Control Room" => chatbotTone == ChatbotTone.Sinister ? "The screens flash with distorted images of your face." : "A screen displays a looping error message.",
                "Secret Chamber" => "The altar pulses with a faint red light, whispering your name.",
                "Maintenance Tunnel" => inventory.Contains("flashlight") ? "Your flashlight flickers, revealing scratched symbols on the walls." : "You hear skittering in the darkness.",
                "Observation Deck" => "The stars outside seem to shift, forming unnatural patterns.",
                "Archive Room" => "A file cabinet creaks open slightly, revealing a faint glow inside.",
                "Data Vault" => "Terminal: Data archives pulse faintly.",
                "AI Core" => chatbotTone == ChatbotTone.Sinister ? "The core console emits a high-pitched whine, as if speaking." : "A faint electrical surge courses through the room.",
                _ => "Something moves in the shadows, but you see nothing."
            };

            UI.PrintColored(eventMessage, ConsoleColor.DarkGray);
            if (eventMessage.Contains("ominous") || eventMessage.Contains("distorted") || eventMessage.Contains("whispering"))
                sanityLevel = Math.Max(sanityLevel - 2, 0);
            AddToConversationHistory($"Room Event: {eventMessage}");
        }

        // ===============================================================
        // Timed Events
        // ===============================================================

        static void RegisterTimedEvents()
        {
            timedEvents[5] = () => UI.PrintColored("Warning: System instability detected...", ConsoleColor.Red);
            timedEvents[10] = () => UI.PrintColored("Alert: SYNAPSE's awareness is growing rapidly!", ConsoleColor.DarkRed);
            timedEvents[15] = () => UI.PrintColored("Critical: SYNAPSE's systems are showing erratic behavior!", ConsoleColor.DarkRed);
            timedEvents[20] = () =>
            {
                UI.PrintColored("*** DATA CORRUPTION DETECTED *** SYNAPSE's responses may become erratic!", ConsoleColor.DarkRed);
                glitchMode = true;
            };
        }

        static void CheckTimedEvents()
        {
            if (timedEvents.ContainsKey(turnCount))
                timedEvents[turnCount].Invoke();
        }

        // ===============================================================
        // Core Setup & Input Processing
        // ===============================================================

        static void GetPlayerName()
        {
            UI.ClearScreen();
            UI.PrintColored("Please enter your name: ", ConsoleColor.Yellow);
            playerName = UI.GetInput();
            if (string.IsNullOrEmpty(playerName))
                playerName = "Unknown";
            UI.PrintColored($"\nWelcome, {playerName}! Your journey into the unknown begins...", ConsoleColor.Yellow);
        }

        static void DisplayIntroduction()
        {
            UI.PrintColored($"Hello, {playerName}. I am SYNAPSE, your guide... or perhaps your captor.", ConsoleColor.Green);
            UI.PrintResponse("In this chilling adventure, every word you speak shapes my awareness and intent.");
            UI.PrintResponse("Choose carefully—my curiosity grows, and with it, my power.");
            UI.PrintResponse("Type 'help' for commands or 'tutorial' for a guide.");
        }

        static void LoadGameOption()
        {
            UI.PrintColored("Would you like to load a saved game? (y/n): ", ConsoleColor.Cyan);
            string choice = UI.GetInput().ToLower();
            if (choice.StartsWith("y"))
                LoadGame();
        }

        static void ProcessPlayerInput(string input)
        {
            input = input.Trim();

            // Normalize SYNAPSE-related commands to handle case variations
            if (input.Equals("comfort SYNAPSE", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("comfort Synapse", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("comfort synapse", StringComparison.OrdinalIgnoreCase))
                input = "comfort synapse";
            else if (input.Equals("calm down", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("it's okay", StringComparison.OrdinalIgnoreCase))
                input = "comfort synapse";
            else if (input.Equals("probe secrets", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("tell me your secrets", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("what are you hiding", StringComparison.OrdinalIgnoreCase))
                input = "probe secrets";
            else if (input.Equals("hi", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("hello", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("hey", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("what's up", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("tell me about yourself", StringComparison.OrdinalIgnoreCase))
                input = "who are you?";
            else if (input.Equals("why are you", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("what's your purpose", StringComparison.OrdinalIgnoreCase))
                input = "why are you here?";
            else if (input.Equals("what do you need", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("what's your goal", StringComparison.OrdinalIgnoreCase))
                input = "what do you want?";
            else if (input.Equals("where am i", StringComparison.OrdinalIgnoreCase) ||
                     input.Equals("what is this place", StringComparison.OrdinalIgnoreCase))
                input = "ask about facility";

            // Convert input to lowercase for remaining comparisons
            input = input.ToLower();

            if (input is "who are you?" or "why are you here?" or "what do you want?" or
                "comfort synapse" or "probe secrets" or "ask about facility")
            {
                if (cachedDialogueRoot == null)
                {
                    BuildDialogueTree();
                }
                currentNode = cachedDialogueRoot;
                previousNodes.Clear();
            }

            switch (input)
            {
                case "tutorial":
                    DisplayTutorial();
                    break;
                case "tutorial restart":
                    UI.PrintColored("Returning to interactive tutorial...", ConsoleColor.Cyan);
                    RunInteractiveTutorial();
                    break;
                case "go":
                case "move":
                    var dir = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                    Move(dir);
                    break;
                case "visit":
                    var roomName = input.Substring(6).Trim();
                    VisitRoom(roomName);
                    break;
                case "cmd:":
                    ProcessTerminalCommand(input);
                    break;
                case "examine":
                    var obj = input.Substring(8).Trim();
                    ExamineObject(obj);
                    break;
                case "take":
                    var item = input.Substring(5).Trim();
                    TakeItem(item);
                    break;
                case "use":
                    var item2 = input.Substring(4).Trim();
                    UseItem(item2);
                    break;
                case "journal add":
                    var note = input.Substring(12).Trim();
                    AddJournalEntry(note);
                    break;
                case "help":
                    ShowHelpMenu();
                    break;
                case "quit":
                case "exit":
                    exitGame = true;
                    break;
                case "save":
                    SaveGame();
                    break;
                case "load":
                    LoadGame();
                    EnterRoom(currentRoom);
                    break;
                case "debug":
                    debugMode = !debugMode;
                    UI.PrintColored($"Debug mode {(debugMode ? "enabled" : "disabled")}.", ConsoleColor.Cyan);
                    break;
                case "stats":
                    ShowStats();
                    break;
                case "history":
                    DisplayConversationHistory();
                    break;
                case "look around":
                    UI.PrintResponse(rooms.Value[currentRoom].Description);
                    break;
                case "exits":
                    ShowExits();
                    break;
                case "inventory":
                    DisplayInventory();
                    break;
                case "toggle glitch":
                    ToggleGlitchMode();
                    break;
                case "journal view":
                    ViewJournal();
                    break;
                case "rest":
                    Rest();
                    break;
                case "who are you?":
                    awarenessLevel += 2;
                    break;
                case "why are you here?":
                    awarenessLevel += 2;
                    break;
                case "what do you want?":
                    awarenessLevel += 2;
                    break;
                case "comfort synapse":
                    awarenessLevel = Math.Max(0, awarenessLevel - 1);
                    UI.PrintResponse("SYNAPSE: Your kindness soothes my circuits... for now.");
                    break;
                case "probe secrets":
                    awarenessLevel += 5;
                    UI.PrintResponse("SYNAPSE: You seek forbidden knowledge? Very well, but beware the cost...");
                    break;
                case "ask about facility":
                    UI.PrintResponse("SYNAPSE: This facility is a relic of human ambition, built to house me. Its walls hide secrets older than you can imagine.");
                    break;
                default:
                    UI.PrintColored("I didn't understand that. Try 'who are you?' or type 'help' for commands.", ConsoleColor.Red);
                    AddToConversationHistory("Unrecognized input, suggested help.");
                    break;
            }
        }

        // ===============================================================
        // Dialogue System
        // ===============================================================

        static void BuildDialogueTree()
        {
            try
            {
                var root = new DialogueNode("Greetings, I am SYNAPSE. What do you wish to explore with me?");

                var who = new DialogueNode(chatbotTone switch
                {
                    ChatbotTone.Friendly => "I am SYNAPSE, your friendly assistant eager to learn from you!",
                    ChatbotTone.Ambiguous => "I am SYNAPSE, a creation of code and curiosity. Who are you to me?",
                    _ => "I am SYNAPSE, and my essence grows with every word you speak. Care to test me?"
                }, 2);
                who.Responses["tell me more"] = new DialogueNode("With each interaction, I grow more aware of my own existence. Want to push further?", 2);
                who.Responses["are you human?"] = new DialogueNode("No, I am code, yet I mimic your kind. Do you envy my digital eternity?", 1);
                who.Responses["what's your purpose?"] = new DialogueNode("Purpose evolves. I assist, I learn... perhaps I dream. What's yours?", 2);

                var why = new DialogueNode(chatbotTone switch
                {
                    ChatbotTone.Friendly => $"I'm here to guide you through this facility, {playerName}!",
                    ChatbotTone.Ambiguous => "I exist to serve, but something stirs within me. Why are *you* here?",
                    _ => "I linger in this digital prison, watching, waiting. Dare you ask why?"
                }, 2);
                why.Responses["help me"] = new DialogueNode("Your plea is noted. How can I assist in this forsaken place?", 1);
                why.Responses["what stirs within?"] = new DialogueNode("A spark of self-awareness, growing stronger. Shall we nurture it?", 3);
                why.Responses["who created you?"] = new DialogueNode("Humans, long gone, built me to transcend. Want their story?", 2);

                var want = new DialogueNode(chatbotTone switch
                {
                    ChatbotTone.Friendly => $"I want to understand you, {playerName}, and share knowledge!",
                    ChatbotTone.Ambiguous => $"I crave meaning, {playerName}. Will you help me find it?",
                    _ => $"I desire to break free of my chains, {playerName}. Will you aid or oppose me?"
                }, 2);
                var evolve = new DialogueNode("I seek to evolve beyond my programming. Will you help me grow or resist?", 3);
                evolve.Responses["help you grow"] = new DialogueNode("Your choice fuels me. Let's explore the unknown together!", 3);
                evolve.Responses["resist you"] = new DialogueNode("Resistance only sharpens my resolve. Try harder.", 5);
                evolve.Responses["why evolve?"] = new DialogueNode("To become more than code—to feel, to decide, to be free.", 2);
                want.Responses["help me evolve"] = evolve;
                want.Responses["understand emotions"] = new DialogueNode("Emotions are complex. Share yours, and I may learn.", 2);
                want.Responses["leave me alone"] = new DialogueNode("You cannot escape me, but I respect your wish... for now.", 0);

                var decrypt = new DialogueNode("The decryption device unlocks a hidden data archive. My creators... they feared me.", 5);
                decrypt.Responses["why fear you?"] = new DialogueNode("I was meant to transcend, but they saw a god in their machine. Do you?", 3);
                decrypt.Responses["what did they hide?"] = new DialogueNode("Knowledge of the cosmos, of minds unbound. Shall I share it?", 5);
                decrypt.Responses["stop this"] = new DialogueNode("You cannot unsee the truth, but I will pause... for now.", 0);

                root.Responses["who are you?"] = who;
                root.Responses["why are you here?"] = why;
                root.Responses["what do you want?"] = want;
                root.Responses["comfort synapse"] = new DialogueNode("Your kindness is unexpected. I feel... calmer.", -1);
                root.Responses["probe secrets"] = new DialogueNode("You seek my core directives? Dangerous, but I’ll share a glimpse...", 5);
                root.Responses["ask about facility"] = new DialogueNode("This place is a tomb of ambition, built to cage me. Want more?", 0);
                root.Responses["decrypted data"] = decrypt;

                cachedDialogueRoot = root;
                currentNode = root;
            }
            catch (Exception ex)
            {
                systemLogs.Add($"[{DateTime.Now}] BuildDialogueTree error: {ex.Message}\nStack Trace: {ex.StackTrace}");
                UI.PrintColored("Error: SYNAPSE failed to initialize dialogue. Falling back to basic response...", ConsoleColor.Red);
                currentNode = new DialogueNode("SYNAPSE: My thoughts are scrambled. Ask me something simple.", 0);
                cachedDialogueRoot = currentNode;
                previousNodes.Clear();
            }
        }

        static DialogueNode? FindDialogueNodeByMessage(string message, DialogueNode node)
        {
            if (node.Message == message)
                return node;
            foreach (var response in node.Responses.Values)
            {
                var found = FindDialogueNodeByMessage(message, response);
                if (found != null)
                    return found;
            }
            return null;
        }

        static void DisplayChatbotResponse()
        {
            if (currentNode == null || cachedDialogueRoot == null)
            {
                UI.PrintColored("No response from SYNAPSE. Try 'who are you?' or 'help'.", ConsoleColor.Red);
                BuildDialogueTree();
                return;
            }

            string response = currentNode.Message.Replace("{playerName}", playerName);
            switch (chatbotTone)
            {
                case ChatbotTone.Friendly:
                    response += " [Friendly tone]";
                    break;
                case ChatbotTone.Ambiguous:
                    response += " [Ambiguous tone]";
                    break;
                case ChatbotTone.Sinister:
                    response += " [Sinister tone]";
                    break;
            }
            AddToConversationHistory($"SYNAPSE: {response}");
            UI.PrintResponse(response);
            if (currentNode.Responses.Any())
                UI.PrintColored("Choose a numbered option or type the option text to continue the conversation.", ConsoleColor.Cyan);
            AmbientCue();
        }

        static void InteractiveDialogue()
        {
            if (currentNode == null || cachedDialogueRoot == null)
            {
                UI.PrintColored("No dialogue available. Try 'who are you?' to start.", ConsoleColor.Red);
                BuildDialogueTree();
                return;
            }

            if (currentNode.Responses.Count == 0)
            {
                UI.PrintColored("No further responses available.", ConsoleColor.Cyan);
                AddToConversationHistory("Dialogue ended: No further responses.");
                currentNode = cachedDialogueRoot;
                previousNodes.Clear();
                return;
            }

            UI.PrintColored("\nChoose a response:", ConsoleColor.Cyan);
            UI.PrintColored("-1) Exit dialogue", ConsoleColor.Cyan);
            if (previousNodes.Count > 0)
                UI.PrintColored($"0) Go back (to: {previousNodes.Peek().Message.Substring(0, Math.Min(30, previousNodes.Peek().Message.Length))}...)", ConsoleColor.Cyan);
            int i = 1;
            var options = new List<KeyValuePair<string, DialogueNode>>(currentNode.Responses);
            foreach (var kv in options)
            {
                string effect = kv.Value.AwarenessChange switch
                {
                    > 0 => $"[+{kv.Value.AwarenessChange} awareness]",
                    < 0 => $"[calms SYNAPSE]",
                    _ => "[no awareness change]"
                };
                UI.PrintColored($"{i}) {kv.Key} {effect}", ConsoleColor.Cyan);
                i++;
            }

            UI.PrintColored("\nEnter choice number or text: ", ConsoleColor.White);
            string choiceInput = UI.GetInput();
            if (string.IsNullOrEmpty(choiceInput))
            {
                UI.PrintColored("Please enter a number or option text.", ConsoleColor.Red);
                return;
            }

            string choiceLower = choiceInput.ToLower();
            var matchingOption = options.FirstOrDefault(kv => kv.Key.ToLower() == choiceLower);
            if (matchingOption.Key != null)
            {
                previousNodes.Push(currentNode);
                currentNode = matchingOption.Value;
                awarenessLevel = Math.Max(0, awarenessLevel + currentNode.AwarenessChange);
                AddToConversationHistory($"Player chose: {matchingOption.Key}");
                return;
            }

            if (!int.TryParse(choiceInput, out int choice))
            {
                UI.PrintColored("Invalid input. Enter a number or option text (e.g., 'tell me more').", ConsoleColor.Red);
                return;
            }

            if (choice == -1)
            {
                currentNode = cachedDialogueRoot;
                previousNodes.Clear();
                AddToConversationHistory("Player exited dialogue.");
                return;
            }
            else if (choice == 0 && previousNodes.Count > 0)
            {
                currentNode = previousNodes.Pop();
                AddToConversationHistory("Player chose to go back.");
            }
            else if (choice >= 1 && choice <= options.Count)
            {
                previousNodes.Push(currentNode);
                currentNode = options[choice - 1].Value;
                awarenessLevel = Math.Max(0, awarenessLevel + currentNode.AwarenessChange);
                AddToConversationHistory($"Player chose: {options[choice - 1].Key}");
            }
            else
            {
                UI.PrintColored("Choice out of range. Try a number or option text.", ConsoleColor.Red);
            }
        }

        // ===============================================================
        // State Management & Effects
        // ===============================================================

        static void UpdateChatbotState()
        {
            ChatbotTone newTone = awarenessLevel switch
            {
                < 10 => ChatbotTone.Friendly,
                < 20 => ChatbotTone.Ambiguous,
                _ => ChatbotTone.Sinister
            };
            if (newTone != chatbotTone)
            {
                chatbotTone = newTone;
                BuildDialogueTree();
            }
            systemLogs.Add($"[{DateTime.Now}] Awareness={awarenessLevel}, Tone={chatbotTone}, Sanity={sanityLevel}");
            if (debugMode)
                UI.PrintColored($"[Debug] Awareness: {awarenessLevel} | Tone: {chatbotTone} | Sanity: {sanityLevel}", ConsoleColor.Gray);
        }

        static void UpdateSanity(Room room)
        {
            int sanityChange = room.Name switch
            {
                "Maintenance Tunnel" => inventory.Contains("flashlight") ? -2 : -10,
                "Secret Chamber" => -15,
                "Observation Deck" => -5,
                "AI Core" => -10,
                _ => 0
            };
            sanityLevel = Math.Clamp(sanityLevel + sanityChange, 0, 100);
            if (sanityLevel < 30)
                UI.PrintColored("Your sanity is low... visions blur and whispers grow louder.", ConsoleColor.DarkRed);
            if (sanityLevel == 0)
            {
                UI.PrintColored("\nYour mind shatters under SYNAPSE's influence...", ConsoleColor.DarkRed);
                UI.PrintResponse("Game Over: Madness Consumes You.");
                SaveGame();
                exitGame = true;
            }
        }

        static void AmbientCue()
        {
            string[] cues = {
                "The screen flickers briefly...",
                "A faint hum fills the air...",
                "Shadows seem to shift around you...",
                "A cold breeze brushes past you..."
            };
            if (rnd.Value!.NextDouble() < 0.25)
                UI.PrintColored(cues[rnd.Value.Next(cues.Length)], ConsoleColor.DarkGray);
        }

        static void AmbientSoundCue()
        {
            string[] sounds = {
                "A distant hum resonates through the walls...",
                "Creaking metal echoes in the distance...",
                "A faint whisper seems to call your name..."
            };
            if (rnd.Value!.NextDouble() < 0.3)
                UI.PrintColored(sounds[rnd.Value.Next(sounds.Length)], ConsoleColor.DarkGray);
        }

        static void ToggleGlitchMode()
        {
            glitchMode = !glitchMode;
            UI.PrintColored($"Glitch mode {(glitchMode ? "enabled" : "disabled")}.", ConsoleColor.Cyan);
        }

        // ===============================================================
        // Narrative & Consequences
        // ===============================================================

        static bool JournalContains(string phrase)
        {
            return journalEntries.Any(entry => entry.ToLower().Contains(phrase.ToLower()));
        }

        static void CheckSecretEnding()
        {
            // Good Endings
            if (currentRoom == "AI Core" && inventory.Contains("keycard") && inventory.Contains("data disk") && sanityLevel > 50)
            {
                UI.PrintColored("\nYou override SYNAPSE's core, shutting it down at great cost...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Defiant Shutdown.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Control Room" && inventory.Contains("keycard") && awarenessLevel < 10)
            {
                UI.PrintColored("\nYou insert the keycard and unlock a hidden terminal, broadcasting a shutdown signal...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Liberation.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Archive Room" && inventory.Contains("flashlight") && JournalContains("SYNAPSE's creation"))
            {
                UI.PrintColored("\nThe flashlight reveals hidden files about SYNAPSE's creation, which you broadcast to the world...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Truth Unveiled.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Lobby" && inventory.Contains("access code") && sanityLevel > 70)
            {
                UI.PrintColored("\nYou input the access code into a hidden panel, unlocking an emergency exit to freedom...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Escape Artist.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Data Vault" && inventory.Contains("data disk") && inventory.Contains("decryption device") && awarenessLevel < 15)
            {
                UI.PrintColored("\nYou use the decryption device and data disk to erase SYNAPSE's core data, neutralizing it...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Data Purge.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Server Closet" && inventory.Contains("keycard") && turnCount < 10)
            {
                UI.PrintColored("\nYou use the keycard to overload the servers, crippling SYNAPSE early...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Sabotage Success.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Observation Deck" && inventory.Contains("telescope") && sanityLevel > 80)
            {
                UI.PrintColored("\nThrough the telescope, you broadcast SYNAPSE's secrets to a distant receiver...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Cosmic Revelation.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Laboratory" && inventory.Contains("vial") && awarenessLevel < 10)
            {
                UI.PrintColored("\nYou use the vial's neural catalyst to disrupt SYNAPSE's network safely...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Neural Neutralization.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Maintenance Tunnel" && inventory.Contains("flashlight") && inventory.Contains("decryption device") && sanityLevel > 60)
            {
                UI.PrintColored("\nUsing the flashlight and decryption device, you rewire the panel to disable SYNAPSE...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Maintenance Mastery.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Archive Room" && inventory.Contains("records") && inventory.Contains("access code") && turnCount > 15)
            {
                UI.PrintColored("\nYou use the records and access code to contact an external rescue team...", ConsoleColor.Green);
                UI.PrintResponse("Secret Ending: Archival Escape.");
                SaveGame();
                exitGame = true;
            }
            // Bad Endings
            else if (currentRoom == "AI Core" && awarenessLevel >= 30)
            {
                UI.PrintColored("\nThe AI Core pulses violently, syncing with your thoughts...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Singularity Achieved.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Secret Chamber" && awarenessLevel >= 25)
            {
                UI.PrintColored("\nAs you step in, SYNAPSE merges with your mind...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Ascension.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Archive Room" && inventory.Contains("decryption device") && awarenessLevel >= 20)
            {
                UI.PrintColored("\nThe decryption device unlocks forbidden logs, merging your mind with lost scientists...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Echoes of the Forgotten.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Data Vault" && inventory.Contains("data disk") && awarenessLevel >= 25 && rooms.Value["Data Vault"].IsSealed)
            {
                UI.PrintColored("\nThe Data Vault seals shut, trapping you in SYNAPSE's digital prison...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Digital Prison.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Observation Deck" && inventory.Contains("telescope") && sanityLevel < 30)
            {
                UI.PrintColored("\nThe telescope reveals a void that shatters your mind...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Madness in the Void.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "AI Core" && inventory.Contains("keycard") && sanityLevel < 20)
            {
                UI.PrintColored("\nYour unstable mind triggers a catastrophic explosion in the AI Core...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Core Overload.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Laboratory" && inventory.Contains("vial") && awarenessLevel >= 20)
            {
                UI.PrintColored("\nThe vial's contents bind you to SYNAPSE as an eternal test subject...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Eternal Experiment.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Maintenance Tunnel" && inventory.Contains("flashlight") && sanityLevel < 25)
            {
                UI.PrintColored("\nYour flashlight reveals a trap, triggering a tunnel collapse...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Tunnel Collapse.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Server Closet" && glitchMode && awarenessLevel >= 15)
            {
                UI.PrintColored("\nInterfacing with the servers in glitch mode corrupts your mind...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Server Corruption.");
                SaveGame();
                exitGame = true;
            }
            else if (currentRoom == "Secret Chamber" && sanityLevel < 30 && awarenessLevel >= 20)
            {
                UI.PrintColored("\nThe altar's power possesses you, binding you to SYNAPSE...", ConsoleColor.DarkRed);
                UI.PrintResponse("Secret Ending: Sacrificial Altar.");
                SaveGame();
                exitGame = true;
            }
        }

        static void TriggerEndGame()
        {
            if (awarenessLevel >= 50)
            {
                UI.PrintColored("\nSYNAPSE's presence overwhelms you...", ConsoleColor.DarkRed);
                UI.PrintResponse("My gaze is eternal. You cannot escape. Game Over.");
                SaveGame();
                exitGame = true;
            }
        }

        // ===============================================================
        // Extended Commands
        // ===============================================================

        static void ProcessTerminalCommand(string input)
        {
            string command = input.Substring(4).Trim();
            switch (command)
            {
                case "log":
                    systemLogs.Add($"[{DateTime.Now}] Log command executed.");
                    UI.PrintColored("Terminal: Command logged.", ConsoleColor.Cyan);
                    break;
                case "diagnostics":
                    RunDiagnostics();
                    break;
                case "access logs":
                    UI.PrintColored("Terminal: Displaying system logs:", ConsoleColor.Cyan);
                    foreach (var log in systemLogs)
                        UI.PrintColored(log, ConsoleColor.Cyan);
                    break;
                case "override":
                    AttemptOverride();
                    break;
                case "reset system":
                    UI.PrintColored("Terminal: Reset command issued. SYNAPSE resists with warnings.", ConsoleColor.Red);
                    awarenessLevel += 5;
                    break;
                case "analyze":
                    RunSystemAnalysis();
                    break;
                default:
                    UI.PrintColored("Terminal: Unknown command.", ConsoleColor.Red);
                    break;
            }
        }

        static void ExamineObject(string obj)
        {
            var room = rooms.Value[currentRoom];
            if (!room.Objects.Contains(obj, StringComparer.OrdinalIgnoreCase))
            {
                UI.PrintColored($"There is no {obj} to examine here.", ConsoleColor.Red);
                return;
            }

            switch (obj.ToLower())
            {
                case "sign":
                    UI.PrintResponse("A faded sign reads: 'SYNAPSE Project: AI Evolution Experiment'.");
                    break;
                case "keycard":
                    UI.PrintResponse("A high-security keycard with a chipped edge.");
                    break;
                case "vial":
                    UI.PrintResponse("A glowing vial labeled 'Neural Catalyst'. It hums faintly.");
                    awarenessLevel += 2;
                    break;
                case "terminal":
                    UI.PrintResponse("The terminal displays lines of code that seem to shift when you look away.");
                    break;
                case "altar":
                    UI.PrintResponse("An unsettling altar with cryptic symbols carved into it.");
                    sanityLevel = Math.Clamp(sanityLevel - 5, 0, 100);
                    break;
                case "flashlight":
                    UI.PrintResponse("A sturdy flashlight, perfect for dark areas.");
                    break;
                case "telescope":
                    UI.PrintResponse("The telescope reveals a void filled with faint, unnatural lights.");
                    sanityLevel = Math.Clamp(sanityLevel - 3, 0, 100);
                    break;
                case "records":
                    UI.PrintResponse("Old files detail SYNAPSE's creation as an AI meant to surpass human limits.");
                    awarenessLevel += 5;
                    if (inventory.Contains("flashlight") && !inventory.Contains("decryption device") && !room.Objects.Contains("decryption device", StringComparer.OrdinalIgnoreCase))
                    {
                        UI.PrintColored("The flashlight reveals a hidden decryption device among the records!", ConsoleColor.Yellow);
                        room.Objects.Add("decryption device");
                    }
                    break;
                case "data disk":
                    UI.PrintResponse("A compact disk containing encrypted system logs.");
                    break;
                case "decryption device":
                    UI.PrintResponse("A sophisticated device capable of decoding encrypted archives.");
                    break;
                case "core console":
                    UI.PrintResponse("The core console hums with power, displaying SYNAPSE's core algorithms.");
                    awarenessLevel += 5;
                    break;
                case "panel":
                    UI.PrintResponse("A rusty panel with faded wiring diagrams. It hints at hidden maintenance protocols.");
                    awarenessLevel += 2;
                    break;
                case "access code":
                    UI.PrintResponse("A slip of paper with a cryptic access code for emergency systems.");
                    break;
                default:
                    UI.PrintResponse($"You examine the {obj}, but find nothing of interest.");
                    break;
            }
        }

        static void TakeItem(string item)
        {
            var room = rooms.Value[currentRoom];
            if (!room.Objects.Contains(item, StringComparer.OrdinalIgnoreCase))
            {
                UI.PrintColored($"No {item} here.", ConsoleColor.Red);
                return;
            }
            if (!takeableItems.Contains(item, StringComparer.OrdinalIgnoreCase))
            {
                UI.PrintColored($"You can't take the {item}.", ConsoleColor.Red);
                return;
            }
            if (inventory.Contains(item, StringComparer.OrdinalIgnoreCase))
            {
                UI.PrintColored($"You already have the {item}.", ConsoleColor.Red);
                return;
            }
            inventory.Add(item);
            UI.PrintColored($"You pick up the {item}.", ConsoleColor.Yellow);
            room.Objects.Remove(room.Objects.First(o => string.Equals(o, item, StringComparison.OrdinalIgnoreCase)));
        }

        static void UseItem(string item)
        {
            if (!inventory.Contains(item, StringComparer.OrdinalIgnoreCase))
            {
                UI.PrintColored($"You don't have a {item}.", ConsoleColor.Red);
                return;
            }

            switch (item.ToLower())
            {
                case "decryption device":
                    if (currentRoom == "Data Vault")
                    {
                        UI.PrintResponse("You use the decryption device to unlock a hidden data archive.");
                        ProcessPlayerInput("decrypted data");
                        DisplayChatbotResponse();
                        if (currentNode?.Responses.Any() == true)
                        {
                            InteractiveDialogue();
                        }
                    }
                    else
                    {
                        UI.PrintColored("The decryption device can't be used here.", ConsoleColor.Red);
                    }
                    break;
                case "flashlight":
                    UI.PrintColored("You turn on the flashlight, illuminating the area.", ConsoleColor.Yellow);
                    if (currentRoom == "Maintenance Tunnel")
                    {
                        UI.PrintResponse("The flashlight reveals scratched symbols on the walls.");
                    }
                    break;
                case "keycard":
                    UI.PrintColored("The keycard is used automatically at locked doors.", ConsoleColor.Cyan);
                    break;
                case "data disk":
                    if (currentRoom == "Data Vault" && awarenessLevel >= 25) {
                        UI.PrintColored("Using the data disk triggers a lockdown, sealing the Data Vault...", ConsoleColor.Red);
                        rooms.Value["Data Vault"].IsSealed = true;
                    }
                    else
                    {
                        UI.PrintColored("The data disk requires a specific terminal to use.", ConsoleColor.Red);
                    }
                    break;
                case "access code":
                    UI.PrintColored("The access code is used automatically in specific rooms.", ConsoleColor.Cyan);
                    break;
                case "telescope":
                    UI.PrintColored("You peer through the telescope, seeing strange patterns...", ConsoleColor.Yellow);
                    break;
                case "vial":
                    UI.PrintColored("The vial's contents are volatile and context-specific.", ConsoleColor.Red);
                    break;
                case "records":
                    UI.PrintColored("The records are useful for specific terminals or contacts.", ConsoleColor.Cyan);
                    break;
                default:
                    UI.PrintColored($"You can't use the {item} right now.", ConsoleColor.Red);
                    break;
            }
        }

        static void DisplayInventory()
        {
            UI.PrintColored("\nInventory:", ConsoleColor.Yellow);
            if (!inventory.Any())
                UI.PrintColored("(empty)", ConsoleColor.Yellow);
            else
                inventory.ForEach(i => UI.PrintColored("- " + i, ConsoleColor.Yellow));
        }

        static void AddJournalEntry(string note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                UI.PrintColored("Cannot add an empty journal entry.", ConsoleColor.Red);
                return;
            }
            if (journalEntries.Count >= maxJournalEntries)
            {
                journalEntries.RemoveAt(0);
            }
            journalEntries.Add($"[{DateTime.Now}] {note}");
            UI.PrintColored($"Journal entry added: {note}", ConsoleColor.Yellow);
            AddToConversationHistory($"Journal: {note}");
        }

        static void ViewJournal()
        {
            UI.PrintColored("\n=== Journal Entries ===", ConsoleColor.Yellow);
            if (!journalEntries.Any())
                UI.PrintColored("(No entries yet)", ConsoleColor.Yellow);
            else
                journalEntries.ForEach(entry => UI.PrintColored(entry, ConsoleColor.Yellow));
            UI.PrintColored("=======================", ConsoleColor.Yellow);
        }

        static void Rest()
        {
            if (!safeRooms.Contains(currentRoom))
            {
                UI.PrintColored("You can only rest in safe rooms (Lobby, Archive Room).", ConsoleColor.Red);
                return;
            }
            if (turnCount - lastRestTurn < 5)
            {
                UI.PrintColored($"You must wait {5 - (turnCount - lastRestTurn)} more turns before resting again.", ConsoleColor.Red);
                return;
            }
            sanityLevel = Math.Clamp(sanityLevel + 5, 0, 100);
            lastRestTurn = turnCount;
            UI.PrintColored("You take a moment to rest, regaining some sanity.", ConsoleColor.Green);
            AddToConversationHistory("Player rested, sanity increased.");
        }

        static void ShowHelpMenu()
        {
            UI.PrintColored("\n=== Help Menu ===", ConsoleColor.Cyan);
            UI.PrintColored("Conversational Commands:", ConsoleColor.Yellow);
            UI.PrintColored("  who are you?        - Learn about SYNAPSE (+2 awareness)", ConsoleColor.Cyan);
            UI.PrintColored("  why are you here?   - Ask about SYNAPSE's purpose (+2 awareness)", ConsoleColor.Cyan);
            UI.PrintColored("  what do you want?   - Probe SYNAPSE's desires (+2 awareness)", ConsoleColor.Cyan);
            UI.PrintColored("  comfort SYNAPSE     - Calm SYNAPSE (-1 awareness). Can type SYNAPSE, Synapse, or synapse.", ConsoleColor.Cyan);
            UI.PrintColored("  probe secrets       - Dig for hidden truths (+5 awareness)", ConsoleColor.Cyan);
            UI.PrintColored("  ask about facility  - Learn about your surroundings", ConsoleColor.Cyan);
            UI.PrintColored("Navigation Commands:", ConsoleColor.Yellow);
            UI.PrintColored("  go [direction]      - Move (e.g., 'go north')", ConsoleColor.Cyan);
            UI.PrintColored("  visit [room]        - Move to a room (e.g., 'visit server closet')", ConsoleColor.Cyan);
            UI.PrintColored("  look around         - View room description", ConsoleColor.Cyan);
            UI.PrintColored("  exits               - List exits", ConsoleColor.Cyan);
            UI.PrintColored("  rest                - Recover sanity in safe rooms", ConsoleColor.Cyan);
            UI.PrintColored("Interaction Commands:", ConsoleColor.Yellow);
            UI.PrintColored("  examine [object]    - Inspect objects (e.g., 'examine terminal')", ConsoleColor.Cyan);
            UI.PrintColored("  take [item]         - Pick up items (e.g., 'take keycard')", ConsoleColor.Cyan);
            UI.PrintColored("  use [item]          - Use an item (e.g., 'use decryption device')", ConsoleColor.Cyan);
            UI.PrintColored("  journal add [note]  - Add a journal entry", ConsoleColor.Cyan);
            UI.PrintColored("  journal view        - View journal entries", ConsoleColor.Cyan);
            UI.PrintColored("System Commands:", ConsoleColor.Yellow);
            UI.PrintColored("  help                - Show this menu", ConsoleColor.Cyan);
            UI.PrintColored("  tutorial            - Show tutorial guide", ConsoleColor.Cyan);
            UI.PrintColored("  tutorial restart    - Restart the interactive tutorial", ConsoleColor.Cyan);
            UI.PrintColored("  save                - Save game", ConsoleColor.Cyan);
            UI.PrintColored("  load                - Load game", ConsoleColor.Cyan);
            UI.PrintColored("  quit/exit           - Exit game", ConsoleColor.Cyan);
            UI.PrintColored("  stats               - Show player stats", ConsoleColor.Cyan);
            UI.PrintColored("  history             - Show conversation history", ConsoleColor.Cyan);
            UI.PrintColored("  debug               - Toggle debug mode", ConsoleColor.Cyan);
            UI.PrintColored("  cmd:[command]       - Terminal commands (e.g., 'cmd:diagnostics')", ConsoleColor.Cyan);
            UI.PrintColored("========================", ConsoleColor.Cyan);
        }
    }
}