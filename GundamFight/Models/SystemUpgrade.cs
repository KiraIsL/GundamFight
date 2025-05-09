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

        public override string ToString()
        {
            return $"{Name} (DEF +{DefenseBoost}, MOB +{MobilityBoost})";
        }
    }
}