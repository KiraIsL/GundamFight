using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mech.Models
{
    public class Weapon
    {
        public required string Name { get; set; }
        public int AttackPower { get; set; }
        public int EnergyCost { get; set; } = 0;

        public override string ToString() => $"{Name} (+{AttackPower} ATK)";
    }
}