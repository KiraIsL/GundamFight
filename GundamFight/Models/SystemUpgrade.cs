using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mech.Models
{
    public class SystemUpgrade
    {
        public required string Name { get; set; }
        public int DefenseBoost { get; set; } = 0;
        public int MobilityBoost { get; set; } = 0;
        public int ArmourBoost { get; set; } = 0; // New property for Armour
        public int EnergyBoost { get; set; } = 0; // New property for Energy

        public override string ToString()
        {
            return $"{Name} (DEF +{DefenseBoost}, MOB +{MobilityBoost}), ARM +{ArmourBoost}, ENE +{EnergyBoost}";
        }
    }
}