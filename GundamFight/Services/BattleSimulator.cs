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
        public static BattleResult Simulate(Mecha g1, Mecha g2, IBattleStrategy strategy)
        {
            var rng = new Random();

            // Validate inputs
            if (g1.Energy <= 0 || g2.Energy <= 0 || g1.Armour <= 0 || g2.Armour <= 0)
                throw new ArgumentException("Mecha must have positive Energy and Armour to participate in a battle.");

            // Apply pre-battle adjustments
            strategy.Execute(g1, g2);

            int round = 1;
            Mecha? winner = null;

            while (winner == null)
            {
                Console.WriteLine($"\n--- Round {round} ---");

                // Process attacks
                ProcessAttack(g1, g2, rng);
                if (IsDefeated(g2)) winner = g1;

                if (winner == null)
                {
                    ProcessAttack(g2, g1, rng);
                    if (IsDefeated(g1)) winner = g2;
                }

                round++;
            }

            return new BattleResult
            {
                Winner = winner,
                Rounds = round - 1
            };
        }

        /// <summary>
        /// Processes the attack from one Mecha to another.
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <param name="rng"></param>
        private static void ProcessAttack(Mecha attacker, Mecha defender, Random rng)
        {
            int attack = CalculateAttack(attacker, rng, attacker.Energy);
            int defense = CalculateDefense(defender, rng);
            int damage = Math.Max(0, attack - defense);

            Console.WriteLine($"{attacker.Name} attacks {defender.Name} for {damage} damage!");
            defender.Armour -= damage;
        }

        /// <summary>
        /// Checks if a Mecha is defeated based on its Armour and Energy.
        /// </summary>
        /// <param name="mech"></param>
        /// <returns></returns>
        private static bool IsDefeated(Mecha mech)
        {
            return mech.Armour <= 0 || mech.Energy <= 0;
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
