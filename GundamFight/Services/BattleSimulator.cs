using Microsoft.Extensions.Logging;
using Mech.Models;

namespace Simulation
{
    public static class BattleSimulator
    {
        /// <summary>
        /// Simulates a battle between two Mecha instances using the specified strategy.
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <param name="strategy"></param>
        /// <param name="rng"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static BattleResult Simulate(Mecha g1, Mecha g2, IBattleStrategy strategy, ILogger logger)
        {
            var rng = new Random();

            // Validate inputs
            if (g1.TotalEnergy <= 0 || g2.TotalEnergy <= 0 || g1.TotalArmour <= 0 || g2.TotalArmour <= 0)
            {
                logger.LogError("Invalid Mecha stats: Both Mecha must have positive Energy and Armour.");
                throw new ArgumentException("Mecha must have positive Energy and Armour to participate in a battle.");
            }

            logger.LogInformation("Battle started between {Mech1} and {Mech2}.", g1.Name, g2.Name);

            // Apply pre-battle adjustments
            strategy.Execute(g1, g2);
            logger.LogInformation("Pre-battle adjustments applied using strategy: {Strategy}.", strategy.GetType().Name);

            int round = 1;
            Mecha? winner = null;

            while (winner == null)
            {
                logger.LogInformation("Starting round {Round}.", round);

                // Process attacks
                ProcessAttack(g1, g2, rng, logger);
                if (IsDefeated(g2))
                {
                    winner = g1;
                    break;
                }

                ProcessAttack(g2, g1, rng, logger);
                if (IsDefeated(g1))
                {
                    winner = g2;
                    break;
                }

                round++;
            }

            logger.LogInformation("Battle ended after {Rounds} rounds. Winner: {Winner} (Pilot: {Pilot}).", round, winner.Name, winner.Pilot);

            return new BattleResult
            {
                Winner = winner,
                Rounds = round
            };
        }

        /// <summary>
        /// Processes the attack from one Mecha to another.
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <param name="rng"></param>
        private static void ProcessAttack(Mecha attacker, Mecha defender, Random rng, ILogger logger)
        {
            int attack = CalculateAttack(attacker, rng, attacker.TotalEnergy);
            int defense = CalculateDefense(defender, rng);
            int damage = Math.Max(0, attack - defense);

            logger.LogInformation("{Attacker} attacks {Defender} for {Damage} damage!", attacker.Name, defender.Name, damage);
            Console.WriteLine($"{attacker.Name} attacks {defender.Name} for {damage} damage!");

            // Adjusting the defender's armor using a method instead of direct assignment
            defender.ModifyStats(0, -damage);
        }

        /// <summary>
        /// Checks if a Mecha is defeated based on its Armour and Energy.
        /// </summary>
        /// <param name="mech"></param>
        /// <returns></returns>
        private static bool IsDefeated(Mecha mech)
        {
            return mech.TotalArmour <= 0 || mech.TotalEnergy <= 0;
        }

        /// <summary>
        /// Calculates the attack power of a Mecha based on its weapons and energy.
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="rng"></param>
        /// <param name="energy"></param>
        /// <returns></returns>
        private static int CalculateAttack(Mecha mech, Random rng, int energy)
        {
            int totalAttack = 0;
            foreach (var weapon in mech.Weapons)
            {
                if (energy >= weapon.EnergyCost)
                {
                    totalAttack += weapon.AttackPower;
                    energy -= weapon.EnergyCost;
                }
            }
            return ApplyVariation(totalAttack, rng);
        }

        /// <summary>
        /// Calculates the defense power of a Mecha based on its system upgrades.
        /// </summary>
        /// <param name="mech"></param>
        /// <param name="rng"></param>
        /// <returns></returns>
        private static int CalculateDefense(Mecha mech, Random rng)
        {
            return ApplyVariation(mech.Defense, rng);
        }

        /// <summary>
        /// Applies a random variation to a value to simulate unpredictability in battle.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rng"></param>
        /// <returns></returns>
        private static int ApplyVariation(int value, Random rng)
        {
            double variation = 0.9 + rng.NextDouble() * 0.2;
            return (int)(value * variation);
        }
    }

    /// <summary>
    /// Represents the result of a battle between two Mecha instances.
    /// </summary>
    public class BattleResult
    {
        public required Mecha Winner { get; set; }
        public int Rounds { get; set; }
    }
}
