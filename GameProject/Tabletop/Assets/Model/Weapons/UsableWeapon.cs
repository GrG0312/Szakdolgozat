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
    public class UsableWeapon : IDamageDealer, IUsable
    {
        public bool CanDamage { get; set; }
        public UnitWeapon Weapon { get; }

        public UsableWeapon(UnitWeapon w)
        {
            CanDamage = true;
            Weapon = w;
        }

        public async Task<int> Damage(IDamagable target, IDiceRoller roller)
        {
            CanDamage = false;

            int numberOfDice = Weapon.Count * Weapon.Constants.Attacks;
            int[] result = await roller.RollDice(numberOfDice);

            string debuglog = "Roll complete: [ ";
            foreach (int item in result)
            {
                debuglog += $"{item} ";
            }
            debuglog += "]";
            Debug.Log(debuglog);
            
            int attacksHit = result.Count(roll => roll > Weapon.Constants.BallisticSkill);
            Debug.Log($"Attacks hit: {attacksHit} (with a {Weapon.Constants.BallisticSkill}+ BS)");
            if (target is IArmored armored)
            {
                int attacksEvaded = await armored.ArmorSave(attacksHit, Weapon.Constants.ArmorPiercing, roller);
                attacksHit -= attacksEvaded;
            }

            int totalDamage = attacksHit * Weapon.Constants.Damage;
            Debug.Log($"Damaging enemy for: {attacksHit} * {Weapon.Constants.Damage} = {totalDamage}");
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
