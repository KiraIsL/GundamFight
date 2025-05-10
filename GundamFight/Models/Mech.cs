using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mech.Models
{

    public abstract class BaseMecha
    {
        public required string Name { get; set; }
        public string? Pilot { get; set; }
        public int Energy { get; set; } = 100;
        public int Armour { get; set; } = 500;
        public List<Weapon> Weapons { get; set; } = [];
        public List<SystemUpgrade> SystemUpgrades { get; set; } = [];
        public int Attack => Weapons.Sum(w => w.AttackPower);
        public int Defense => SystemUpgrades.Sum(u => u.DefenseBoost);
        public int Mobility => SystemUpgrades.Sum(u => u.MobilityBoost);
        public int TotalArmour => Armour + SystemUpgrades.Sum(u => u.ArmourBoost);
        public int TotalEnergy => Energy + SystemUpgrades.Sum(u => u.EnergyBoost);


        public virtual void DisplayStats()
        {
            Console.WriteLine($"Gundam: {Name} | Pilot: {Pilot}");
            Console.WriteLine($"Energy: {TotalEnergy} | Armour: {TotalArmour}");
            Console.WriteLine($"Attack: {Attack} | Defense: {Defense} | Mobility: {Mobility}");
        }
    }
    public class Mecha : BaseMecha
    {
        /// <summary>
        /// Displays the stats of the Mecha instance, including weapons and system upgrades.
        /// </summary>
        public override void DisplayStats()
        {
            base.DisplayStats();
            Console.WriteLine("Weapons:");
            foreach (var weapon in Weapons)
            {
                Console.WriteLine($"- {weapon}");
            }
            Console.WriteLine("System Upgrades:");
            foreach (var upgrade in SystemUpgrades)
            {
                Console.WriteLine($"- {upgrade}");
            }
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
                Weapons = [],
                SystemUpgrades = []
            };
        }

        /// <summary>
        /// Modifies the stats of the Mecha instance.
        /// </summary>
        /// <param name="attackDelta"></param>
        /// <param name="defenseDelta"></param>
        public void ModifyStats(int attackDelta, int defenseDelta)
        {
            if (attackDelta < 0)
            {
                // Apply damage to weapons  
                int damage = Math.Abs(attackDelta);
                Console.WriteLine($"Applying {damage} damage to attack power.");
                Weapons.ForEach(w => w.AttackPower = Math.Max(0, w.AttackPower - damage));
            }
            else
            {
                // Add a temp weapon with the attackDelta  
                Weapons.Add(new Weapon
                {
                    Name = "Buffed Weapon",
                    AttackPower = attackDelta,
                    EnergyCost = 0
                });
            }

            if (defenseDelta < 0)
            {
                // Apply damage to defense  
                int damage = Math.Abs(defenseDelta);
                Console.WriteLine($"Applying {damage} damage to defense.");
                Armour = Math.Max(0, Armour - damage); // Reduce Armour directly  
            }
            else
            {
                // Add Temp Shield system upgrade  
                SystemUpgrades.Add(new SystemUpgrade
                {
                    Name = "Temp Shield",
                    DefenseBoost = defenseDelta,
                    MobilityBoost = 0
                });
            }
        }
    }
}
