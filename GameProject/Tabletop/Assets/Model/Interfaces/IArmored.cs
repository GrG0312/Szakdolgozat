using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    public interface IArmored
    {
        public Task<int> ArmorSave(int amount, int armorPiercing, IDiceRoller roller);
    }
}
