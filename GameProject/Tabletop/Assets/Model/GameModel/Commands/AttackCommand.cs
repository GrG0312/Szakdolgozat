using Model.Units.Interfaces;

namespace Model.Commands
{
    /// <summary>
    /// This class represents a command where a unit attacks an other
    /// </summary>
    public class AttackCommand : Command
    {
        /// <summary>
        /// The unit taking the damage
        /// </summary>
        public IDamagable Target { get; private set; }
        /// <summary>
        /// The unit dealing the damage
        /// </summary>
        public IWeaponUser BiggerFish { get; private set; }

        public AttackCommand(IWeaponUser biggerFish, IDamagable smallerFish)
        {
            BiggerFish = biggerFish;
            Target = smallerFish;
        }

        public override void Execute()
        {
            BiggerFish.Damage(Target);
        }
    }
}
