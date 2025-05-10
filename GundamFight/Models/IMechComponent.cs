using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mech.Models
{
    public interface IMechComponent
    {
        string Name { get; set; }
        string ToString();
    }
}