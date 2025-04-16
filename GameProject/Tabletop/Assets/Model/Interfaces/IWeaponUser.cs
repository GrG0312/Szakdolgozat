using Model.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    public interface IWeaponUser<WorldPositionType> : ISidedObject, IMapObject<WorldPositionType>
    {
        public IReadOnlyList<UsableWeapon> UsableWeapons { get; }

        public bool CanTarget(IDamageable<WorldPositionType> target);
    }
}
