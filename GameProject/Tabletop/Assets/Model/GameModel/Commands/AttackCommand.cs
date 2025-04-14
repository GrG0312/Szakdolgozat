using Model.Interfaces;
using Model.Weapons;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Model.GameModel.Commands
{
    /// <summary>
    /// This class represents a command where a unit attacks an other
    /// </summary>
    public class AttackCommand<PlayerIdType> : IUnitCommand where PlayerIdType : IComparable<PlayerIdType>
    {
        /// <summary>
        /// The Player who starts the attacking
        /// </summary>
        public IWeaponUser<PlayerIdType> Initiator { get; }
        /// <summary>
        /// The unit taking the damage
        /// </summary>
        public IDamagable<PlayerIdType> Target { get; }
        /// <summary>
        /// The unit dealing the damage
        /// </summary>
        public IDamageDealer? Used { get; private set; }

        public Phase ExecutingPhase { get; }

        public IDiceRoller Roller { get; }

        public AttackCommand(IWeaponUser<PlayerIdType> biggerfish, IDamagable<PlayerIdType> smallerFish, IDiceRoller roller)
        {
            Initiator = biggerfish;
            Used = null;
            Target = smallerFish;
            Roller = roller;
            ExecutingPhase = Phase.Fighting;
        }
        public void SetUsedWeapon(WeaponIdentifier id)
        {
            if (Used != null)
            {
                throw new Exception("Used weapon is already selected!");
            }
            Used = Initiator.EquippedWeapons.Single(w => w.Weapon.Identity == id);
        }
        public async Task Execute()
        {
            await Used.Damage(Target, Roller);
        }
        public bool Preconditions(Phase current)
        {
            return 
                Initiator.Owner.CompareTo(Target.Owner) != 0 &&
                ExecutingPhase == current;
        }
        public bool CanExecute(Phase current)
        {
            return
                Preconditions(current) &&
                Used != null &&
                Used.CanDamage;
        }
    }
}
