using System.Text.Json;
using Mech.Models;
using Microsoft.Extensions.Logging;
using Simulation;
using GundamFight.Services;

namespace MechFight
{
    class Program
    {
        private static readonly ILogger<Program> Logger;

        static Program()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();   // Logs to the debug output
                builder.AddFile("logs/mechfight.log"); // Log to a file
            });
            Logger = loggerFactory.CreateLogger<Program>();
        }

        static void Main(string[] args)
        {
            bool keepRunning;
            do
            {
                try
                {
                    Logger.LogInformation("Application started.");
                    Console.WriteLine("=== Mech LOADOUT MANAGER ===");
                    Console.Write("Enter pilot name: ");
                    string? pilotInput = Console.ReadLine();
                    string pilot = pilotInput ?? "Amuro Ray";

                    Logger.LogInformation("Pilot selected: {Pilot}", pilot);

                    Mecha mech = Mecha.CreateDefault(pilot);

                    mech = ChooseMech(mech, pilot);

                    DisplayLoadout(mech);

                    Console.WriteLine("\nWould you like to simulate a battle? (y/n)");
                    string? confirmBattle = Console.ReadLine();
                    string confirmation = confirmBattle ?? "n";
                    if (confirmation.Equals("y", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Console.WriteLine("Choose your opponent:");
                        Mecha enemy = ChooseOpponent(mech);

                        DisplayBattleLoadout(mech, enemy);

                        Console.WriteLine("\nPress any key to begin the battle...");
                        Console.ReadKey(true); // Waits for any key press without showing the key

                        DisplayBattleResult(mech, enemy);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "An error occurred during execution.");
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                // Ask if they want to continue
                Console.WriteLine("\nDo you want to run again? (y/n)");
                string? input = Console.ReadLine();
                keepRunning = input?.Trim().ToLower() == "y";
            } while (keepRunning);

            Logger.LogInformation("Application exited.");
        }

        /// <summary>
        /// Choose a mech based on user input
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="pilot"></param>
        /// <returns></returns>
        private static Mecha ChooseMech(Mecha mech, string pilot)
        {
            Logger.LogInformation("Choosing mech for pilot: {Pilot}", pilot);
            mech = ChooseMechOrOpponent(mech, pilot, isOpponent: false);
            mech.Pilot = pilot;
            return mech;
        }

        /// <summary>
        /// Choose an opponent mech based on user input
        /// </summary>
        /// <param name="mech"></param>
        /// <returns></returns>
        private static Mecha ChooseOpponent(Mecha mech)
        {
            Logger.LogInformation("Choosing opponent for mech: {MechName}", mech.Name);
            return ChooseMechOrOpponent(mech, "Enemy AI", isOpponent: true);
        }

        /// <summary>
        /// Choose a mech or opponent based on user input
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="pilot"></param>
        /// <param name="isOpponent"></param>
        /// <returns></returns>
        private static Mecha ChooseMechOrOpponent(Mecha mech, string pilot, bool isOpponent)
        {
            Logger.LogInformation("Loading mech options for {Pilot}. IsOpponent: {IsOpponent}", pilot, isOpponent);
            string dataPath = Path.Combine("..", "..", "..", "Data");
            string[] mechFiles = Directory.GetFiles(dataPath, "*.json");

            Dictionary<int, string> menuOptions = new()
            {
                { 1, isOpponent ? "Mirror match (fight your own mech)" : "Random mech" }
            };

            int index = 2;
            foreach (string file in mechFiles)
            {
                string name = Path.GetFileNameWithoutExtension(file).Replace("_", " ");
                menuOptions[index++] = name;
            }

            int customCreateIndex = index++;
            menuOptions[customCreateIndex] = "Create a custom mech";

            foreach (var kvp in menuOptions)
            {
                Console.WriteLine($"{kvp.Key}. {kvp.Value}");
            }

            Console.Write("Enter Selection: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int selection))
            {
                Logger.LogInformation("User selected option: {Selection}", selection);
                return selection switch
                {
                    1 when isOpponent => CreateMirrorMatch(mech),
                    1 => LoadMechFromFile(mechFiles[new Random().Next(mechFiles.Length)]),
                    int n when n >= 2 && n < customCreateIndex => LoadMechFromFile(mechFiles[selection - 2]),
                    int n when n == customCreateIndex => CreateCustomMechFromInput(pilot),
                    _ => DefaultMechFallback(mechFiles)
                };
            }

            Logger.LogWarning("Invalid input. Defaulting to random mech.");
            Console.WriteLine("Invalid input. Defaulting to random mech.");
            return DefaultMechFallback(mechFiles);
        }

        /// <summary>
        /// Fallback to a default mech if no valid mech files are found
        /// </summary>
        /// <param name="mechFiles"></param>
        /// <returns></returns>
        private static Mecha DefaultMechFallback(string[] mechFiles)
        {
            if (mechFiles == null || mechFiles.Length == 0)
            {
                Logger.LogWarning("No mech files found. Falling back to a hardcoded default mech.");
                Console.WriteLine("No mech files found. Falling back to a hardcoded default mech.");
                return Mecha.CreateDefault("Fallback AI");
            }

            try
            {
                string randomFile = mechFiles[new Random().Next(mechFiles.Length)];
                Logger.LogInformation("Defaulting to random mech from file: {FileName}", Path.GetFileName(randomFile));
                Console.WriteLine($"Defaulting to random mech from file: {Path.GetFileName(randomFile)}");
                return LoadMechFromFile(randomFile);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load a random mech.");
                Console.WriteLine($"Failed to load a random mech. Error: {ex.Message}");
                Console.WriteLine("Falling back to a hardcoded default mech.");
                return Mecha.CreateDefault("Fallback AI");
            }
        }

        /// <summary>
        /// Load the default mech from a JSON file
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="pilot"></param>
        /// <returns></returns>
        private static Mecha ChooseDefaultMech(Mecha mech, string pilot)
        {
            try
            {
                string filePath = @"..\..\..\Data\RX_78_2.json";
                Console.WriteLine("Looking for file at: " + Path.GetFullPath(filePath));
                string json = File.ReadAllText(filePath);
                mech = JsonSerializer.Deserialize<Mecha>(json) ?? new Mecha
                {
                    Name = "Default",
                    Pilot = pilot,
                    Weapons = [],
                    SystemUpgrades = []
                };

                //Keep user input pilotName
                mech.Pilot = pilot;

                Console.WriteLine("\n--- Mech Loaded ---");
                Console.WriteLine($"Name: {mech.Name}");
                Console.WriteLine($"Pilot: {mech.Pilot}");
                Console.WriteLine($"Attack: {mech.Attack}");
                Console.WriteLine($"Defense: {mech.Defense}");
                Console.WriteLine("Weapons:");
                foreach (Weapon weapon in mech.Weapons)
                {
                    Console.WriteLine($"- {weapon}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load MEch: {ex.Message}");
            }

            return mech;
        }

        /// <summary>
        /// Display the current loadout of the mech
        /// </summary>
        /// <param name="mech"></param>
        private static void DisplayLoadout(Mecha mech)
        {
            Console.WriteLine("\nCurrent Loadout:");
            mech.DisplayStats();
        }

        /// <summary>
        /// Display the loadout of both mechs before the battle
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="enemy"></param>
        private static void DisplayBattleLoadout(Mecha mech, Mecha enemy)
        {
            Console.WriteLine("\nCurrent Loadout:");
            mech.DisplayStats();
            Console.WriteLine("\nEnemy Loadout:");
            enemy.DisplayStats();
        }

        /// <summary>
        /// Display the battle result after the simulation
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="enemy"></param>
        private static void DisplayBattleResult(Mecha mech, Mecha enemy)
        {
            Console.WriteLine("\nChoose a battle strategy:");
            Console.WriteLine("1. Aggressive");
            Console.WriteLine("2. Defensive");
            Console.WriteLine("3. Balanced");

            string? input = Console.ReadLine();
            IBattleStrategy strategy = input switch
            {
                "1" => new AggressiveStrategy(),
                "2" => new DefensiveStrategy(),
                "3" => new BalancedStrategy(),
                _ => new BalancedStrategy()
            };

            Console.WriteLine("\nBattle Begins:");
            BattleResult result = BattleSimulator.Simulate(mech, enemy, strategy);

            // Announce the winner
            Console.WriteLine($"\nVictory: {result.Winner.Pilot}'s {result.Winner.Name} wins the battle in {result.Rounds} rounds!");

        }

        /// <summary>
        /// Create a custom mech from user input
        /// </summary>
        /// <param name="pilot"></param>
        /// <returns></returns>
        public static Mecha CreateCustomMechFromInput(string pilot)
        {
            Console.Write("Enter the name of your Mech: ");
            string mechName = Console.ReadLine() ?? "Unnamed Mech";

            var mech = new Mecha
            {
                Name = mechName,
                Pilot = string.IsNullOrWhiteSpace(pilot) ? "Unknown Pilot" : pilot
            };

            // Weapons
            int weaponCount;
            while (true)
            {
                Console.Write("How many weapons does your Mech have? ");
                if (int.TryParse(Console.ReadLine(), out weaponCount) && weaponCount >= 0)
                    break;
                Console.WriteLine("Please enter a valid non-negative integer.");
            }
            for (int i = 0; i < weaponCount; i++)
            {
                Console.WriteLine($"\nWeapon {i + 1}:");

                Console.Write("  Name: ");
                string weaponName = Console.ReadLine() ?? "Unnamed Weapon";

                int attackPower;
                while (true)
                {
                    Console.Write("  Attack Power: ");
                    if (int.TryParse(Console.ReadLine(), out attackPower))
                        break;
                    Console.WriteLine("  Please enter a valid integer for attack power.");
                }

                int energyCost;
                while (true)
                {
                    Console.Write("  Energy Cost: ");
                    if (int.TryParse(Console.ReadLine(), out energyCost))
                        break;
                    Console.WriteLine("  Please enter a valid integer for energy cost.");
                }

                mech.Weapons.Add(new Weapon
                {
                    Name = weaponName,
                    AttackPower = attackPower,
                    EnergyCost = energyCost
                });
            }

            // System Upgrades
            int upgradeCount;
            while (true)
            {
                Console.Write("How many system upgrades does your Mech have? ");
                if (int.TryParse(Console.ReadLine(), out upgradeCount) && upgradeCount >= 0)
                    break;
                Console.WriteLine("Please enter a valid non-negative integer.");
            }
            for (int i = 0; i < upgradeCount; i++)
            {
                Console.WriteLine($"\nSystem Upgrade {i + 1}:");

                Console.Write("  Name: ");
                string upgradeName = Console.ReadLine() ?? "Unnamed Upgrade";

                int defenseBoost;
                while (true)
                {
                    Console.Write("  Defense Boost: ");
                    if (int.TryParse(Console.ReadLine(), out defenseBoost))
                        break;
                    Console.WriteLine("  Please enter a valid integer for defense boost.");
                }

                int mobilityBoost;
                while (true)
                {
                    Console.Write("  Mobility Boost: ");
                    if (int.TryParse(Console.ReadLine(), out mobilityBoost))
                        break;
                    Console.WriteLine("  Please enter a valid integer for mobility boost.");
                }

                mech.SystemUpgrades.Add(new SystemUpgrade
                {
                    Name = upgradeName,
                    DefenseBoost = defenseBoost,
                    MobilityBoost = mobilityBoost
                });
            }

            // Save the custom mech to a JSON file
            SaveCustomMech(mech);
            return mech;
        }

        /// <summary>
        /// Load a mech from the JSON files
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Mecha LoadMechFromFile(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<Mecha>(json) ?? Mecha.CreateDefault("Dummy AI");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load enemy config from {filePath}: {ex.Message}");
                return Mecha.CreateDefault("Fallback AI");
            }
        }

        /// <summary>
        /// Helper to create a mirror match (fighting your own mech)
        /// </summary>
        /// <param name="mech"></param>
        /// <returns></returns>
        public static Mecha CreateMirrorMatch(Mecha mech)
        {
            return new Mecha
            {
                Name = mech.Name + " (Enemy)",
                Pilot = "Mirror Pilot",
                Weapons = mech.Weapons.Select(w => new Weapon { Name = w.Name, AttackPower = w.AttackPower }).ToList(),
                SystemUpgrades = mech.SystemUpgrades.Select(s => new SystemUpgrade { Name = s.Name, DefenseBoost = s.DefenseBoost, MobilityBoost = s.MobilityBoost }).ToList()
            };
        }

        /// <summary>
        /// Load a random enemy mech from the JSON files
        /// </summary>
        /// <returns></returns>
        public static Mecha LoadRandomEnemy()
        {
            string[] mechFiles = new[]
            {
            @"..\..\..\Data\Zaku_II.json",
            @"..\..\..\Data\Gouf_Custom.json",
            @"..\..\..\Data\Wing_Gundam_Zero.json",
            @"..\..\..\Data\Gundam_Barbatos.json"
        };

            // Pick one at random
            Random rand = new Random();
            string selectedPath = mechFiles[rand.Next(mechFiles.Length)];
            Console.WriteLine($"Selected enemy file: {Path.GetFullPath(selectedPath)}");

            return LoadMechFromFile(selectedPath);
        }

        /// <summary>
        /// Saves the custom mech to a JSON file
        /// </summary>
        /// <param name="mech"></param>
        public static void SaveCustomMech(Mecha mech)
        {
            string safeFileName = mech.Name.Replace(" ", "_").Replace("/", "_");
            string path = $@"..\..\..\Data\{safeFileName}.json";

            try
            {
                string json = JsonSerializer.Serialize(mech, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                Console.WriteLine($"Custom mech saved to: {Path.GetFullPath(path)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save custom mech: {ex.Message}");
            }
        }

        /// <summary>
        /// Load a custom mech from a JSON file
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="pilot"></param>
        /// <returns></returns>

    }
}