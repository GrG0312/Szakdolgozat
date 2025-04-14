using Model.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    public interface IWeaponUser
    {
        public IReadOnlyList<UsableWeapon> EquippedWeapons { get; }
    }

    public interface IWeaponUser<T> : IWeaponUser, IOwned<T> { }
}
