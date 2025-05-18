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
            Logger = InitializeLogger();
        }

        private static ILogger<Program> InitializeLogger()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
                builder.AddFile("logs/mechfight.log");
            });
            return loggerFactory.CreateLogger<Program>();
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
                    string pilot = UserInteractionService.GetUserInput("Enter pilot name: ", "Amuro Ray");

                    Logger.LogInformation("Pilot selected: {Pilot}", pilot);

                    Mecha mech = Mecha.CreateDefault(pilot);

                    mech = MechSelectionService.ChooseMech(mech, pilot);

                    UserInteractionService.DisplayLoadout(mech);

                    string confirmation = UserInteractionService.GetUserInput("\nWould you like to simulate a battle? (y/n)", "n");
                    if (confirmation.Equals("y", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Console.WriteLine("Choose your opponent:");
                        Mecha enemy = MechSelectionService.ChooseOpponent(mech);

                        UserInteractionService.DisplayBattleLoadout(mech, enemy);

                        Console.WriteLine("\nPress any key to begin the battle...");
                        Console.ReadKey(true); // Waits for any key press without showing the key

                        UserInteractionService.DisplayBattleResult(mech, enemy);
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
    }
}