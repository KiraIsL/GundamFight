using Mech.Models;

public class Weapon : IMechComponent
{
    public required string Name { get; set; }
    public int AttackPower { get; set; }
    public int EnergyCost { get; set; } = 0;

    public override string ToString() => $"{Name} (+{AttackPower} ATK)";
}