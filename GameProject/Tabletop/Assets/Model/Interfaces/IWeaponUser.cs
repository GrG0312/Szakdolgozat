using Model.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    /// <summary>
    /// An object that has usable weapons. 
    /// Implements the <see cref="IMapObject{WorldPositionType}"/> interfaces
    /// in order to have access to the position of the object.
    /// </summary>
    /// <typeparam name="WorldPositionType"><inheritdoc/></typeparam>
    public interface IWeaponUser<WorldPositionType> : IMapObject<WorldPositionType>
    {
        /// <summary>
        /// The weapons owned by this object.
        /// </summary>
        public IReadOnlyList<UsableWeapon> UsableWeapons { get; }

        /// <summary>
        /// Determines if the object can target the <paramref name="target"/>.
        /// </summary>
        public bool CanTarget(IDamageable<WorldPositionType> target);
    }
}
