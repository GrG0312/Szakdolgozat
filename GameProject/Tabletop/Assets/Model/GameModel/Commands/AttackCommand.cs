using Model.Interfaces;
using Model.Weapons;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model.GameModel.Commands
{
    /// <summary>
    /// This class represents a command where a unit attacks an other with a specified weapon
    /// </summary>
    public class AttackCommand<WorldPositionType> : IUnitCommand
    {
        /// <summary>
        /// The Unit who starts the attacking
        /// </summary>
        public IWeaponUser<WorldPositionType> Initiator { get; }
        /// <summary>
        /// The unit taking the damage
        /// </summary>
        public IDamageable<WorldPositionType> Target { get; }
        /// <summary>
        /// The weapon dealing the damage
        /// </summary>
        public IDamageDealer? Used { get; private set; }

        public Phase ExecutingPhase { get; }

        /// <summary>
        /// The dice roller object used to generate the random roll numbers
        /// </summary>
        public IDiceRoller Roller { get; }

        public IReadOnlyList<UsableWeapon> UsableWeapons { get; }

        public AttackCommand(IWeaponUser<WorldPositionType> biggerfish, IDamageable<WorldPositionType> smallerFish, IDiceRoller roller, List<UsableWeapon> weapons)
        {
            Initiator = biggerfish;
            Used = null;
            Target = smallerFish;
            Roller = roller;
            ExecutingPhase = Phase.Fighting;
            UsableWeapons = weapons;
        }

        public void RegisterUsedWeapon(WeaponIdentifier id)
        {
            Used = UsableWeapons.Single(w => w.Weapon.Identity == id);
        }

        public bool ValidCommand(Phase current)
        {
            return
                UsableWeapons.Count > 0 &&
                Initiator.CanTarget(Target) &&
                ExecutingPhase == current;
        }

        public bool CanExecute(Phase current)
        {
            return
                ValidCommand(current) &&
                Used.CanDamage;
        }

        public async Task Execute()
        {
            await Used.Damage(Target, Roller);
        }
    }
}
