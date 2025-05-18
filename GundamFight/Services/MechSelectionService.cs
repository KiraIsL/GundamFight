using System.Text.Json;
using Mech.Models;
using Microsoft.Extensions.Logging;
using Simulation;
using GundamFight.Services;

namespace GundamFight.Services
{
    public static class MechSelectionService
    {
        public static ILogger Logger { get; set; } = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("UserInteractionService");

        /// <summary>
        /// Choose a mech based on user input
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="pilot"></param>
        /// <returns></returns>
        public static Mecha ChooseMech(Mecha mech, string pilot)
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
        public static Mecha ChooseOpponent(Mecha mech)
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

            string? input = UserInteractionService.GetUserInput("Enter Selection: ", "");

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
                Console.WriteLine($"Defaulting to random mech: {Path.GetFileNameWithoutExtension(randomFile).Replace("_", " ")}");
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
        /// Create a custom mech from user input
        /// </summary>
        /// <param name="pilot"></param>
        /// <returns></returns>
        public static Mecha CreateCustomMechFromInput(string pilot)
        {
            Logger.LogInformation("Starting custom mech creation for pilot: {Pilot}", pilot);

            Console.Write("Enter the name of your Mech: ");
            string mechName = Console.ReadLine() ?? "Unnamed Mech";
            Logger.LogInformation("Mech name entered: {MechName}", mechName);

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
                {
                    Logger.LogInformation("Number of weapons entered: {WeaponCount}", weaponCount);
                    break;
                }
                Console.WriteLine("Please enter a valid non-negative integer.");
                Logger.LogWarning("Invalid input for weapon count.");
            }
            for (int i = 0; i < weaponCount; i++)
            {
                Console.WriteLine($"\nWeapon {i + 1}:");

                Console.Write("  Name: ");
                string weaponName = Console.ReadLine() ?? "Unnamed Weapon";
                Logger.LogInformation("Weapon {Index} name entered: {WeaponName}", i + 1, weaponName);

                int attackPower;
                while (true)
                {
                    attackPower = UserInteractionService.ReadIntInput(
                        "  Attack Power: ",
                        "  Please enter a valid integer for attack power."
                    );
                    Logger.LogInformation("Weapon {Index} attack power entered: {AttackPower}", i + 1, attackPower);
                }

                int energyCost;
                while (true)
                {
                    attackPower = UserInteractionService.ReadIntInput(
                        "  Attack Power: ",
                        "  Please enter a valid integer for energy cost."
                    );
                    Logger.LogInformation("Weapon {Index} energy cost entered: {EnergyCost}", i + 1, energyCost);
                }

                mech.AddWeaponPublic(new Weapon
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
                {
                    Logger.LogInformation("Number of system upgrades entered: {UpgradeCount}", upgradeCount);
                    break;
                }
                Console.WriteLine("Please enter a valid non-negative integer.");
                Logger.LogWarning("Invalid input for system upgrade count.");
            }
            for (int i = 0; i < upgradeCount; i++)
            {
                Console.WriteLine($"\nSystem Upgrade {i + 1}:");

                Console.Write("  Name: ");
                string upgradeName = Console.ReadLine() ?? "Unnamed Upgrade";
                Logger.LogInformation("System upgrade {Index} name entered: {UpgradeName}", i + 1, upgradeName);

                int defenseBoost;
                while (true)
                {
                    Console.Write("  Defense Boost: ");
                    if (int.TryParse(Console.ReadLine(), out defenseBoost))
                    {
                        Logger.LogInformation("System upgrade {Index} defense boost entered: {DefenseBoost}", i + 1, defenseBoost);
                        break;
                    }
                    Console.WriteLine("  Please enter a valid integer for defense boost.");
                    Logger.LogWarning("Invalid input for defense boost of system upgrade {Index}.", i + 1);
                }

                int mobilityBoost;
                while (true)
                {
                    Console.Write("  Mobility Boost: ");
                    if (int.TryParse(Console.ReadLine(), out mobilityBoost))
                    {
                        Logger.LogInformation("System upgrade {Index} mobility boost entered: {MobilityBoost}", i + 1, mobilityBoost);
                        break;
                    }
                    Console.WriteLine("  Please enter a valid integer for mobility boost.");
                    Logger.LogWarning("Invalid input for mobility boost of system upgrade {Index}.", i + 1);
                }

                int armourBoost;
                while (true)
                {
                    Console.Write("  Armour Boost: ");
                    if (int.TryParse(Console.ReadLine(), out armourBoost))
                    {
                        Logger.LogInformation("System upgrade {Index} armour boost entered: {armourBoost}", i + 1, armourBoost);
                        break;
                    }
                    Console.WriteLine("  Please enter a valid integer for armour boost.");
                    Logger.LogWarning("Invalid input for armour boost of system upgrade {Index}.", i + 1);
                }

                int energyBoost;
                while (true)
                {
                    Console.Write("  Energy Boost: ");
                    if (int.TryParse(Console.ReadLine(), out energyBoost))
                    {
                        Logger.LogInformation("System upgrade {Index} energy boost entered: {energyBoost}", i + 1, energyBoost);
                        break;
                    }
                    Console.WriteLine("  Please enter a valid integer for energy boost.");
                    Logger.LogWarning("Invalid input for energy boost of system upgrade {Index}.", i + 1);
                }

                mech.AddSystemUpgradePublic(new SystemUpgrade
                {
                    Name = upgradeName,
                    DefenseBoost = defenseBoost,
                    MobilityBoost = mobilityBoost,
                    ArmourBoost = armourBoost,
                    EnergyBoost = energyBoost
                });
            }

            // Save the custom mech to a JSON file
            try
            {
                SaveCustomMech(mech);
                Logger.LogInformation("Custom mech successfully created and saved: {MechName}", mech.Name);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save custom mech: {MechName}", mech.Name);
            }

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

                // Deserialize into a temporary object to extract weapons and system upgrades
                var tempMech = JsonSerializer.Deserialize<TempMecha>(json);

                if (tempMech == null)
                {
                    Console.WriteLine("Failed to deserialize Mecha. Returning default Mecha.");
                    return Mecha.CreateDefault("Dummy AI");
                }

                // Create a new Mecha instance
                var mech = new Mecha
                {
                    Name = tempMech.Name,
                    Pilot = tempMech.Pilot
                };

                // Add weapons using AddWeapon
                foreach (var weapon in tempMech.Weapons)
                {
                    mech.AddWeaponPublic(new Weapon
                    {
                        Name = weapon.Name,
                        AttackPower = weapon.AttackPower,
                        EnergyCost = weapon.EnergyCost
                    });
                }

                // Add system upgrades using AddSystemUpgrade
                foreach (var upgrade in tempMech.SystemUpgrades)
                {
                    mech.AddSystemUpgradePublic(new SystemUpgrade
                    {
                        Name = upgrade.Name,
                        DefenseBoost = upgrade.DefenseBoost,
                        MobilityBoost = upgrade.MobilityBoost,
                        ArmourBoost = upgrade.ArmourBoost,
                        EnergyBoost = upgrade.EnergyBoost
                    });
                }

                return mech;
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
            var mirrorMech = new Mecha
            {
                Name = mech.Name + " (Enemy)",
                Pilot = "Mirror Pilot"
            };

            // Add weapons using AddWeapon
            foreach (var weapon in mech.Weapons)
            {
                mirrorMech.AddWeaponPublic(new Weapon
                {
                    Name = weapon.Name,
                    AttackPower = weapon.AttackPower,
                    EnergyCost = weapon.EnergyCost
                });
            }

            // Add system upgrades using AddSystemUpgrade
            foreach (var upgrade in mech.SystemUpgrades)
            {
                mirrorMech.AddSystemUpgradePublic(new SystemUpgrade
                {
                    Name = upgrade.Name,
                    DefenseBoost = upgrade.DefenseBoost,
                    MobilityBoost = upgrade.MobilityBoost,
                    ArmourBoost = upgrade.ArmourBoost,
                    EnergyBoost = upgrade.EnergyBoost
                });
            }

            return mirrorMech;
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
            // Ask the user for confirmation
            string input = UserInteractionService.GetUserInput($"Do you want to save your custom Mech '{mech.Name}'? (y/n)", "n");

            if (input.Trim().ToLower() != "y")
            {
                Console.WriteLine("Mech was not saved.");
                return; // Exit without saving
            }

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
    }
}
