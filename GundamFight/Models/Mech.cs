
using System.Text;

namespace Mech.Models
{
    /// <summary>
    /// Base class for all Mecha types.
    /// </summary>
    public abstract class BaseMecha
    {
        public required string Name { get; set; }
        public string? Pilot { get; set; }

        private int _energy = 100;
        public int Energy
        {
            get => _energy;
            set => _energy = Math.Max(0, value);
        }
        private int _armour = 500;
        public int Armour
        {
            get => _armour;
            set => _armour = Math.Max(0, value);
        }

        private readonly List<Weapon> _weapons = new();
        private readonly List<SystemUpgrade> _systemUpgrades = new();

        public IReadOnlyList<Weapon> Weapons => _weapons.AsReadOnly();
        public IReadOnlyList<SystemUpgrade> SystemUpgrades => _systemUpgrades.AsReadOnly();

        public int Attack => _weapons.Sum(w => w.AttackPower);
        public int Defense => _systemUpgrades.Sum(u => u.DefenseBoost);
        public int Mobility => _systemUpgrades.Sum(u => u.MobilityBoost);
        public int TotalArmour => Armour + _systemUpgrades.Sum(u => u.ArmourBoost);
        public int TotalEnergy => Energy + _systemUpgrades.Sum(u => u.EnergyBoost);

        /// <summary>
        /// Displays the stats of the Mecha instance.
        /// </summary>
        public virtual void DisplayStats()
        {
            var stats = new StringBuilder();
            stats.AppendLine($"Gundam: {Name} | Pilot: {Pilot ?? "Unassigned"}");
            stats.AppendLine($"Energy: {TotalEnergy} | Armour: {TotalArmour}");
            stats.AppendLine($"Attack: {Attack} | Defense: {Defense} | Mobility: {Mobility}");
            Console.WriteLine(stats.ToString());
        }

        protected void AddWeapon(Weapon weapon)
        {
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));
            _weapons.Add(weapon);
        }

        protected void AddSystemUpgrade(SystemUpgrade upgrade)
        {
            if (upgrade == null) throw new ArgumentNullException(nameof(upgrade));
            _systemUpgrades.Add(upgrade);
        }
    }

    /// <summary>
    /// Represents a Mecha with weapons and system upgrades.
    /// </summary>
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
        /// Creates a default Mecha instance with a given name.
        /// </summary>
        /// <param name="pilot"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Mecha CreateDefault(string pilot)
        {
            if (string.IsNullOrWhiteSpace(pilot))
                throw new ArgumentException("Pilot name cannot be null or empty.", nameof(pilot));

            return new Mecha
            {
                Name = "Default",
                Pilot = pilot
            };
        }

        /// <summary>
        /// Modifies the Mecha's stats based on the provided deltas.
        /// </summary>
        /// <param name="attackDelta"></param>
        /// <param name="defenseDelta"></param>
        public void ModifyStats(int attackDelta, int defenseDelta)
        {
            AdjustAttack(attackDelta);
            AdjustDefense(defenseDelta);
        }

        /// <summary>
        /// Adjusts the attack power of the Mecha based on the provided delta.
        /// </summary>
        /// <param name="attackDelta"></param>
        private void AdjustAttack(int attackDelta)
        {
            if (attackDelta < 0)
            {
                int damage = Math.Abs(attackDelta);
                foreach (var weapon in Weapons)
                {
                    weapon.AttackPower = Math.Max(0, weapon.AttackPower - damage);
                }
            }
            else
            {
                AddWeapon(new Weapon
                {
                    Name = "Buffed Weapon",
                    AttackPower = attackDelta,
                    EnergyCost = 0
                });
            }
        }

        /// <summary>
        /// Adjusts the defense of the Mecha based on the provided delta.
        /// </summary>
        /// <param name="defenseDelta"></param>
        private void AdjustDefense(int defenseDelta)
        {
            if (defenseDelta < 0)
            {
                Armour = Math.Max(0, Armour - Math.Abs(defenseDelta));
            }
            else
            {
                AddSystemUpgrade(new SystemUpgrade
                {
                    Name = "Temp Shield",
                    DefenseBoost = defenseDelta,
                    MobilityBoost = 0
                });
            }
        }

        /// <summary>
        /// Adds a weapon to the Mecha using the protected AddWeapon method.
        /// </summary>
        /// <param name="weapon">The weapon to add.</param>
        public void AddWeaponPublic(Weapon weapon) => AddWeapon(weapon);

        /// <summary>
        /// Adds a system upgrade to the Mecha using the protected AddSystemUpgrade method.
        /// </summary>
        /// <param name="systemUpgrade"></param>
        public void AddSystemUpgradePublic(SystemUpgrade systemUpgrade) => AddSystemUpgrade(systemUpgrade);
    }

    /// <summary>
    /// Temporary class for Mecha creation.
    /// </summary>
    internal class TempMecha
    {
        public TempMecha(string name, string? pilot, List<Weapon> weapons, List<SystemUpgrade> systemUpgrades)
        {
            Name = name;
            Pilot = pilot;
            Weapons = weapons;
            SystemUpgrades = systemUpgrades;
        }

        public required string Name { get; set; }
        public string? Pilot { get; set; }
        public List<Weapon> Weapons { get; set; } = new();
        public List<SystemUpgrade> SystemUpgrades { get; set; } = new();
    }
}
