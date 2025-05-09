using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mech.Models
{
    public class Mecha
    {
        public required string Name { get; set; }
        public string? Pilot { get; set; }
        public int Energy { get; set; } = 100;
        public int Armour { get; set; } = 500;
        public List<Weapon> Weapons { get; set; } = new();
        public List<SystemUpgrade> SystemUpgrades { get; set; } = new();

        public int Attack => Weapons.Sum(w => w.AttackPower);
        public int Defense => SystemUpgrades.Sum(u => u.DefenseBoost);
        public int Mobility => SystemUpgrades.Sum(u => u.MobilityBoost);

        /// <summary>
        /// Displays the stats of the Mecha instance.
        /// </summary>
        public void DisplayStats()
        {
            Console.WriteLine($"Gundam: {Name} | Pilot: {Pilot}");
            Console.WriteLine($"Attack: {Attack} | Defense: {Defense} | Mobility: {Mobility}");
        }

        /// <summary>
        /// Creates a default Mecha instance with a given pilot name.
        /// </summary>
        /// <param name="pilot"></param>
        /// <returns></returns>
        public static Mecha CreateDefault(String pilot)
        {
            return new Mecha
            {
                Name = "Default",
                Pilot = pilot,
                Weapons = new List<Weapon>
                {
                },
                SystemUpgrades = new List<SystemUpgrade>
                {
                }
            };
        }

        /// <summary>
        /// Modifies the stats of the Mecha instance.
        /// </summary>
        /// <param name="attackDelta"></param>
        /// <param name="defenseDelta"></param>
        public void ModifyStats(int attackDelta, int defenseDelta)
        {
            // e.g. bump the first weapon
            if (Weapons.Count > 0)
                Weapons[0].AttackPower += attackDelta;

            // and maybe add a defensive upgrade
            SystemUpgrades.Add(new SystemUpgrade
            {
                Name = "Temp Shield",
                DefenseBoost = defenseDelta,
                MobilityBoost = 0
            });
        }
    }
}
