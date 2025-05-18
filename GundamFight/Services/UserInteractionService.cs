using Mech.Models;
using Microsoft.Extensions.Logging;
using Simulation;

namespace GundamFight.Services
{
    public static class UserInteractionService
    {
        private static readonly ILogger Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("UserInteractionService");

        /// <summary>
        /// Get User input
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetUserInput(string prompt, string defaultValue = "")
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
        }

        /// <summary>
        /// Display the loadout of the mech after the user has selected their options
        /// </summary>
        /// <param name="mech"></param>
        public static void DisplayLoadout(Mecha mech)
        {
            Console.WriteLine("\nCurrent Loadout:");
            mech.DisplayStats();
        }

        /// <summary>
        /// Display the loadout of both mechs before the battle
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="enemy"></param>
        public static void DisplayBattleLoadout(Mecha mech, Mecha enemy)
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
        public static void DisplayBattleResult(Mecha mech, Mecha enemy)
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
            BattleSimulator simulator = new BattleSimulator(new Random(), Logger);
            BattleResult result = simulator.Simulate(mech, enemy, strategy);

            Console.WriteLine($"\nVictory: {result.Winner?.Pilot ?? "Unknown Pilot"}'s {result.Winner?.Name ?? "Unknown Mecha"} wins the battle in {result.Rounds} rounds!");
        }

        private static int ReadIntInput(string prompt, string warning, Func<int, bool>? validator = null)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int value) && (validator == null || validator(value)))
                    return value;
                Console.WriteLine(warning);
            }
        }

    }
}
