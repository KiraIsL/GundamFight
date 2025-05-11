using Microsoft.Extensions.Logging;
using Mech.Models;

namespace Simulation
{
    public class BattleSimulator
    {
        private readonly Random _rng;
        private readonly ILogger _logger;

        public BattleSimulator(Random rng, ILogger logger)
        {
            _rng = rng;
            _logger = logger;
        }

        /// <summary>
        /// Simulates a battle between two Mecha instances using the provided battle strategy.
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <param name="strategy"></param>
        /// <returns></returns>
public BattleResult Simulate(Mecha g1, Mecha g2, IBattleStrategy strategy)
{
    try
    {
        // Validate Mecha
        ValidateMecha(g1, g2);
    }
    catch (ArgumentException ex)
    {
        _logger.LogError(ex, "Battle validation failed: {Message}", ex.Message);

        // Inform the user and return a result indicating failure
        Console.WriteLine("Battle cannot proceed: " + ex.Message);
        return new BattleResult
        {
            Winner = null, // No winner since the battle didn't happen
            Rounds = 0,
            IsDraw = false
        };
    }

    _logger.LogInformation("Battle started between {Mech1} and {Mech2}.", g1.Name, g2.Name);

    strategy.Execute(g1, g2);
    _logger.LogInformation("Pre-battle adjustments applied using strategy: {Strategy}.", strategy.GetType().Name);

    int round = 1;
    Mecha? winner = null;

    while (winner == null)
    {
        _logger.LogInformation("Starting round {Round}.", round);

        winner = ProcessRound(g1, g2);

        // Check for a draw
        if (winner == null && IsDefeated(g1) && IsDefeated(g2))
        {
            _logger.LogInformation("The battle ends in a draw after {Rounds} rounds.", round);
            return new BattleResult
            {
                Winner = null,
                Rounds = round,
                IsDraw = true
            };
        }

        if (winner == null)
        {
            round++;
        }
    }

    _logger.LogInformation("Battle ended after {Rounds} rounds. Winner: {Winner} (Pilot: {Pilot}).", round, winner.Name, winner.Pilot);

    return new BattleResult
    {
        Winner = winner,
        Rounds = round,
        IsDraw = false
    };
}

        /// <summary>
        /// Validates the Mecha instances to ensure they have positive Energy and Armour.
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ValidateMecha(Mecha g1, Mecha g2)
        {
            if (g1.TotalEnergy <= 0 || g2.TotalEnergy <= 0 || g1.TotalArmour <= 0 || g2.TotalArmour <= 0)
            {
                _logger.LogError("Invalid Mecha stats: Both Mecha must have positive Energy and Armour.");
                throw new ArgumentException("Mecha must have positive Energy and Armour to participate in a battle.");
            }
        }

        /// <summary>
        /// Processes a round of battle between two Mecha instances.
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        private Mecha? ProcessRound(Mecha g1, Mecha g2)
        {
            ProcessAttack(g1, g2);
            ProcessAttack(g2, g1);

            // Check if both Mecha are defeated
            if (IsDefeated(g1) && IsDefeated(g2))
            {
                _logger.LogInformation("Both Mecha are defeated. The battle ends in a draw.");
                return null; // Indicate a draw
            }

            // Check if only one Mecha is defeated
            if (IsDefeated(g2)) return g1;
            if (IsDefeated(g1)) return g2;

            return null; // No winner yet, continue to the next round
        }

        /// <summary>
        /// Processes an attack from one Mecha to another.
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        private void ProcessAttack(Mecha attacker, Mecha defender)
        {
            int attack = CalculateAttack(attacker);
            int defense = CalculateDefense(defender);
            int damage = Math.Max(0, attack - defense);

            _logger.LogInformation("{Attacker} attacks {Defender} for {Damage} damage!", attacker.Name, defender.Name, damage);

            defender.ModifyStats(0, -damage);
        }

        /// <summary>
        /// Checks if a Mecha is defeated based on its total energy and armour.
        /// </summary>
        /// <param name="mech"></param>
        /// <returns></returns>
        private bool IsDefeated(Mecha mech)
        {
            return mech.TotalArmour <= 0 || mech.TotalEnergy <= 0;
        }

        /// <summary>
        /// Calculates the attack power of a Mecha based on its weapons and energy.
        /// </summary>
        /// <param name="mech"></param>
        /// <returns></returns>
        private int CalculateAttack(Mecha mech)
        {
            int totalAttack = mech.Weapons
                .Where(w => mech.TotalEnergy >= w.EnergyCost)
                .Sum(w => w.AttackPower);

            return ApplyVariation(totalAttack);
        }

        /// <summary>
        /// Calculates the defense power of a Mecha based on its system upgrades and energy.
        /// </summary>
        /// <param name="mech"></param>
        /// <returns></returns>
        private int CalculateDefense(Mecha mech)
        {
            return ApplyVariation(mech.Defense);
        }

        /// <summary>
        /// Applies a random variation to the given value to simulate unpredictability in battle.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int ApplyVariation(int value)
        {
            double variation = 0.9 + _rng.NextDouble() * 0.2;
            return (int)(value * variation);
        }
    }

    /// <summary>
    /// Represents the result of a battle between two Mecha instances.
    /// </summary>
    public class BattleResult
    {
        public Mecha? Winner { get; set; } // Nullable to handle no winner
        public int Rounds { get; set; }
        public bool IsDraw { get; set; } = false; // Indicates if the battle ended in a draw
    }
}
