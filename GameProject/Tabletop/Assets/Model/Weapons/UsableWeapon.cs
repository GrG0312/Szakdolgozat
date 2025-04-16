using Model.GameModel;
using Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Model.Weapons
{
    public class UsableWeapon : IDamageDealer, IUsable, IRanged<int>
    {
        public bool CanDamage { get; set; }

        public int Range { get => Weapon.Constants.Range; }

        public UnitWeapon Weapon { get; }

        public UsableWeapon(UnitWeapon w)
        {
            CanDamage = true;
            Weapon = w;
        }

        public async Task<int> Damage<T>(IDamageable<T> target, IDiceRoller roller)
        {
            CanDamage = false;

            int numberOfDice = Weapon.Count * Weapon.Constants.Attacks;
            int[] result = await roller.RollDice(numberOfDice);
            
            int attacksHit = result.Count(roll => roll > Weapon.Constants.BallisticSkill);
            if (target is IArmored armored)
            {
                int attacksEvaded = await armored.ArmorSave(attacksHit, Weapon.Constants.ArmorPiercing, roller);
                attacksHit -= attacksEvaded;
            }

            int totalDamage = attacksHit * Weapon.Constants.Damage;
            target.TakeDamage(totalDamage);
            return totalDamage;
        }

        public bool IsUsable(Phase where)
        {
            if (where == Phase.Fighting)
            {
                return CanDamage;
            }
            return false;
        }
    }
}
