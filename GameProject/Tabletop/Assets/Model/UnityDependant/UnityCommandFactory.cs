using Model.GameModel.Commands;
using Model.Interfaces;
using Model.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model.UnityDependant
{
    public class UnityCommandFactory : ICommandFactory
    {
        private Dictionary<Type, Func<object[], IUnitCommand>> creationCommands;

        public UnityCommandFactory()
        {
            creationCommands = new Dictionary<Type, Func<object[], IUnitCommand>>();

            creationCommands[typeof(MoveCommand<Vector3>)]
                // MoveCommand: selectedUnit, {targetLocation}
                = args => new MoveCommand<Vector3>((IMovable<Vector3>)args[0], (Vector3)args[1]);

            creationCommands[typeof(AttackCommand<Vector3>)]
                // AttackCommand : selectedUnit, {targetUnit}, roller
                = (args) =>
                {
                    IWeaponUser<Vector3> initiator = (IWeaponUser<Vector3>)args[0];
                    IDamageable<Vector3> target = (IDamageable<Vector3>)args[1];
                    List<UsableWeapon> usableWeapons = new List<UsableWeapon>();
                    foreach (UsableWeapon usableWeapon in initiator.UsableWeapons)
                    {
                        if (usableWeapon.CanDamage && initiator.DistanceTo(target) <= usableWeapon.Weapon.Constants.Range * Defines.RANGE_ADJUSTMENT)
                        {
                            usableWeapons.Add(usableWeapon);
                        }
                    }
                    IDiceRoller roller = (IDiceRoller)args[2];
                    return new AttackCommand<Vector3>(initiator, target, roller, usableWeapons);
                };
        }

        public IUnitCommand Produce<T>(params object[] args) where T : IUnitCommand
        {
            if (creationCommands.TryGetValue(typeof(T), out Func<object[], IUnitCommand> func))
            {
                object[] arr = FlattenArray(args).ToArray(); // flatten the array becasue in the model we also get a params object[] args
                return func(arr);
            }
            throw new ArgumentException("Not supported command type found.");
        }

        private static IEnumerable<object> FlattenArray(IEnumerable<object> array)
        {
            foreach (var item in array)
            {
                if (item is IEnumerable<object> enumerable && !(item is string))
                {
                    foreach (var subItem in FlattenArray(enumerable))
                        yield return subItem;
                }
                else
                {
                    yield return item;
                }
            }
        }
    }
}
