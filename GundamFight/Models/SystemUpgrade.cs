using Mech.Models;

public class SystemUpgrade : IMechComponent
{
    public required string Name { get; set; }
    public int DefenseBoost { get; set; }
    public int MobilityBoost { get; set; }
    public int ArmourBoost { get; set; }
    public int EnergyBoost { get; set; } = 0;

    public override string ToString() =>
        $"{Name} (DEF +{DefenseBoost}, MOB +{MobilityBoost}, ARM +{ArmourBoost}, EN +{EnergyBoost})";
}